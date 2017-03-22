using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using vm.Aspects.Exceptions;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;
using vm.Aspects.Policies;

namespace vm.Aspects.Model
{
    /// <summary>
    /// The class PerCallContextRepositoryCallHandler is meant to be used as a policy (AOP aspect) in the call context of a WCF call.
    /// It is assumed that the repository is resolved from the DI container and has <see cref="T:vm.Aspects.Wcf.PerCallContextLifetimeManager"/>, i.e. all
    /// resolutions for <see cref="IRepository"/> with the same resolve name in the same WCF call context will return one and the same repository object.
    /// This handler implements two post-call actions: if there are no exceptions, it calls <see cref="IRepository.CommitChanges"/> to commit the unit 
    /// of work, otherwise rolls back the current transaction and then removes the repository's lifetime manager from the container. In other words,
    /// the application developer does not need to worry about saving changes in the repository, committing and rolling-back transactions, 
    /// error handling, repository disposal, etc.
    /// </summary>
    public class UnitOfWorkCallHandler : BaseCallHandler<UnitOfWorkData>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to create explicitly transaction scope.
        /// The use of this property must be very well justified.
        /// </summary>
        public bool CreateTransactionScope { get; set; }

        /// <summary>
        /// Gets or sets the optimistic concurrency strategy.
        /// </summary>
        public OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to dispose the repository in the clean-up phase.
        /// If the lifetime of the repository is controlled outside of this handler (e.g. with <see cref="HierarchicalLifetimeManager"/>),
        /// do not dispose the repository here.
        /// </summary>
        public bool DisposeRepository { get; set; }

        /// <summary>
        /// Gets or sets the maximum optimistic concurrency retries.
        /// </summary>
        public int MaxOptimisticConcurrencyRetries { get; set; } = 10;

        /// <summary>
        /// Prepares per-call data specific to the handler.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override UnitOfWorkData Prepare(
            IMethodInvocation input)
        {
            if (!(input.Target is IHasRepository))
                throw new InvalidOperationException($"{nameof(UnitOfWorkCallHandler)} can be used only with services that implement {nameof(IHasRepository)}. Either implement it in {input.Target.GetType().Name} or remove this handler from the pipeline.");

            var data = new UnitOfWorkData();

            if (!CreateTransactionScope)
                return data;

            if (Transaction.Current != null)
            {
                Debug.WriteLine(
                    "WARNING: Did not create transaction scope. The method {0} is called in the context of an existing transaction {1}/{2}. Is this intended!",
                    input.MethodBase.Name,
                    Transaction.Current.TransactionInformation.LocalIdentifier,
                    Transaction.Current.TransactionInformation.DistributedIdentifier);

                return data;
            }

            data.TransactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            return data;
        }

        /// <summary>
        /// Actions that take place after invoking the next handler or the target in the chain.
        /// Here it saves all changes in the IRepository instance which lifetime is managed per call context;
        /// commits the transaction scope if there are no exceptions;
        /// and disposes the IRepository instance.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The result.</param>
        /// <param name="callData">The call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            UnitOfWorkData callData)
        {
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            if (methodReturn.IsAsyncCall())
                return methodReturn;        // return the task, do not clean-up yet

            try
            {
                if (methodReturn.Exception != null)
                    return methodReturn;    // return the exception (and cleanup)

                var hasRepository = (IHasRepository)input.Target;

                // get the repository
                callData.Repository = hasRepository.Repository;

                if (callData.Repository == null)
                    throw new InvalidOperationException(nameof(IHasRepository)+" must return a non-null repository.");

                // commit
                CommitChanges(callData);
                return methodReturn;
            }
            catch (Exception x)
            {
                // wrap the exception in a new IMethodReturn
                return input.CreateExceptionMethodReturn(
                                ProcessException(input, x));
            }
            finally
            {
                // and clean-up
                CleanUp(callData);
            }
        }

        /// <summary>
        /// Gives the aspect a chance to do some final work after the main task is truly complete.
        /// The overriding implementations should begin by calling the base class' implementation first.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The call data.</param>
        /// <returns>Task{TResult}.</returns>
        protected override async Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            UnitOfWorkData callData)
        {
            try
            {
                if (methodReturn.Exception != null)
                    throw methodReturn.Exception;

                var result = await base.ContinueWith<TResult>(input, methodReturn, callData);
                var hasRepository = (IHasRepository)input.Target;

                callData.Repository = hasRepository.Repository;

                if (callData.Repository == null)
                    throw new InvalidOperationException(nameof(IHasRepository)+" must return a non-null repository.");

                await CommitChangesAsync(callData);
                return result;
            }
            catch (Exception x)
            {
                throw ProcessException(input, x);
            }
            finally
            {
                CleanUp(callData);
            }
        }

        void CommitChanges(
            UnitOfWorkData callData)
        {
            var success = false;
            var retries = 0;

            while (!success)
                try
                {
                    callData.Repository.CommitChanges();
                    callData.TransactionScope?.Complete();
                    success = true;
                }
                catch (DbUpdateConcurrencyException x)
                {
                    // see https://msdn.microsoft.com/en-us/data/jj592904.aspx
                    if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.StoreWins  ||
                        retries >= MaxOptimisticConcurrencyRetries)
                        throw;

                    Facility.LogWriter.ExceptionWarning(x);

                    var entry = x.Entries.Single();

                    WaitBeforeRetry();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
        }

        async Task CommitChangesAsync(
            UnitOfWorkData callData)
        {
            var success = false;
            var retries = 0;

            while (!success)
                try
                {
                    await callData.Repository.CommitChangesAsync();
                    callData.TransactionScope?.Complete();
                    success = true;
                }
                catch (DbUpdateConcurrencyException x)
                {
                    // see https://msdn.microsoft.com/en-us/data/jj592904.aspx
                    if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.StoreWins  ||
                        retries >= MaxOptimisticConcurrencyRetries)
                        throw;

                    Facility.LogWriter.ExceptionWarning(x);

                    var entry = x.Entries.Single();

                    await WaitBeforeRetryAsync();
                    entry.OriginalValues.SetValues(await entry.GetDatabaseValuesAsync());
                }
        }

        void WaitBeforeRetry()
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);

            Task.Delay(rand.Next(150)).Wait();
        }

        async Task WaitBeforeRetryAsync()
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);

            await Task.Delay(rand.Next(150));
        }

        void CleanUp(
            UnitOfWorkData callData)
        {
            if (DisposeRepository)
                callData.Repository?.Dispose();

            callData.TransactionScope?.Dispose();
        }

        Exception ProcessException(
            IMethodInvocation input,
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(input     != null, nameof(input));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            var aggregateException = exception as AggregateException;

            if (aggregateException != null  &&  aggregateException.InnerExceptions.Count == 1)
                exception = aggregateException.InnerExceptions[0];

            if (exception.IsTransient()  ||  exception.IsOptimisticConcurrency())
                exception = new RepeatableOperationException(exception);

            // add more info to the exception
            exception.Data.Add("ServiceMethod", input.Target.GetType().ToString()+'.'+input.MethodBase.Name);

            return exception;
        }
    }
}
