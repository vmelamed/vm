using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Data.Entity.Core;
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
    /// The local data to carry between the phases of the call handler.
    /// </summary>
    public class RepositoryData
    {
        /// <summary>
        /// Gets or sets the transaction scope.
        /// </summary>
        public TransactionScope TransactionScope { get; set; }

        /// <summary>
        /// Gets or sets the synchronous repository.
        /// </summary>
        public IRepository Repository { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous repository.
        /// </summary>
        public IRepositoryAsync AsyncRepository { get; set; }
    }

    /// <summary>
    /// OptimisticConcurrencyStrategy defines the strategies for handling optimistic concurrency exceptions (<see cref="OptimisticConcurrencyException"/>)
    /// </summary>
    public enum OptimisticConcurrencyStrategy
    {
        /// <summary>
        /// If the store contains newer values for the object - do not allow changes and extend the exception to the client.
        /// </summary>
        StoreWins,

        /// <summary>
        /// Even if the store contains newer values for the object - the client values are considered with higher priority and the changes are allowed nevertheless.
        /// </summary>
        ClientWins,
    }

    /// <summary>
    /// The class PerCallContextRepositoryCallHandler is meant to be used as a policy (AOP aspect) in the call context of a WCF call.
    /// It is assumed that the repository is resolved from the DI container and has <see cref="T:vm.Aspects.Wcf.PerCallContextLifetimeManager"/>, i.e. all
    /// resolutions for <see cref="IRepository"/> with the same resolve name in the same WCF call context will return one and the same repository object.
    /// This handler implements two post-call actions: if there are no exceptions, it calls <see cref="IRepository.CommitChanges"/> to commit the unit 
    /// of work, otherwise rolls back the current transaction and then removes the repository's lifetime manager from the container. In other words,
    /// the application developer does not need to worry about saving changes in the repository, committing and rolling-back transactions, 
    /// error handling, repository disposal, etc.
    /// </summary>
    public class UnitOfWorkCallHandler : BaseCallHandler<RepositoryData>
    {
        /// <summary>
        /// Gets or sets the resolve name of the repository registered in the current call context.
        /// </summary>
        public string RepositoryResolveName { get; set; }

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
        /// Gets or sets the maximum optimistic concurrency retries.
        /// </summary>
        public int MaxOptimisticConcurrencyRetries { get; set; } = 10;

        /// <summary>
        /// Guards the registrations below.
        /// </summary>
        static object _sync = new object();

        /// <summary>
        /// Prepares per-call data specific to the handler.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override RepositoryData Prepare(
            IMethodInvocation input)
        {
            var hasRepository = input.Target as IHasRepository;

            if (hasRepository == null)
                throw new InvalidOperationException($"Using this handler on services that do not implement {nameof(IHasRepository)} doesn't make sence. Either implement IHasRepository or remove this handler from the policies chain.");

            var data = new RepositoryData();

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
            RepositoryData callData)
        {
            Contract.Requires<ArgumentNullException>(input        != null, nameof(input));
            Contract.Requires<ArgumentNullException>(methodReturn != null, nameof(methodReturn));

            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            if (methodReturn.IsAsyncCall())
                return methodReturn;        // return the task, do not clean-up yet

            try
            {
                if (methodReturn.Exception != null)
                    return methodReturn;    // return the exception (and cleanup)

                var hasRepository = input.Target as IHasRepository;

                // get the repository
                callData.Repository = hasRepository.Repository ?? hasRepository.AsyncRepository;

                if (callData.Repository == null)
                    throw new InvalidOperationException($"{nameof(IHasRepository)} must return at least one non-null repository.");

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
            RepositoryData callData)
        {
            try
            {
                if (methodReturn.Exception != null)
                    throw methodReturn.Exception;

                var result = await base.ContinueWith<TResult>(input, methodReturn, callData);
                var hasRepository = input.Target as IHasRepository;

                callData.AsyncRepository = hasRepository.AsyncRepository;

                if (callData.AsyncRepository != null)
                {
                    await CommitChangesAsync(callData);
                    return result;
                }
                else
                {
                    callData.Repository = hasRepository.Repository;

                    if (callData.Repository != null)
                    {
                        CommitChanges(callData);
                        return result;
                    }
                    else
                        throw new InvalidOperationException($"{nameof(IHasRepository)} must return at least one non-null repository.");
                }
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
            RepositoryData callData)
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
            RepositoryData callData)
        {
            var success = false;
            var retries = 0;

            while (!success)
                try
                {
                    await callData.AsyncRepository.CommitChangesAsync();
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
            RepositoryData callData)
        {
            callData.Repository?.Dispose();
            callData.AsyncRepository?.Dispose();
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
