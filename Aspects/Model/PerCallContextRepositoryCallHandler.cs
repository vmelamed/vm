using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using vm.Aspects.Exceptions;
using vm.Aspects.Model.Repository;
using vm.Aspects.Policies;

namespace vm.Aspects.Model
{
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
    /// The class PerCallContextRepositoryCallHandler is meant to be used as a policy (AOP aspect) in the call context of a WCF call.
    /// It is assumed that the repository is resolved from the DI container and has <see cref="T:vm.Aspects.Wcf.PerCallContextLifetimeManager"/>, i.e. all
    /// resolutions for <see cref="IRepository"/> with the same resolve name in the same WCF call context will return one and the same repository object.
    /// This handler implements two post-call actions: if there are no exceptions, it calls <see cref="IRepository.CommitChanges"/> to commit the unit 
    /// of work, otherwise rolls back the current transaction and then removes the repository's lifetime manager from the container. In other words,
    /// the application developer does not need to worry about saving changes in the repository, committing and rolling-back transactions, 
    /// error handling, repository disposal, etc.
    /// </summary>
    public class PerCallContextRepositoryCallHandler : BaseCallHandler<RepositoryData>
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
        /// <param name="result">The result.</param>
        /// <param name="callData">The call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn result,
            RepositoryData callData)
        {
            Contract.Requires<ArgumentNullException>(input  != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));

            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            if (((MethodInfo)input.MethodBase).ReturnType.Is<Task>())
                return PostInvokeAsync(input, result, callData);
            else
                return PostInvokeSync(input, result, callData);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn PostInvokeSync(
            IMethodInvocation input,
            IMethodReturn result,
            RepositoryData callData)
        {
            Contract.Requires<ArgumentNullException>(input  != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            try
            {
                if (result.Exception != null)
                {
                    CleanUp(callData);
                    return result;
                }

                var hasRepository = input.Target as IHasRepository;

                if (hasRepository == null)
                {
                    CleanUp(callData);
                    return result;
                }

                callData.Repository = hasRepository.Repository ?? hasRepository.AsyncRepository;
                CommitChanges(callData);
                return result;
            }
            catch (Exception x)
            {
                return ProcessException(input, x);
            }
            finally
            {
                CleanUp(callData);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn PostInvokeAsync(
            IMethodInvocation input,
            IMethodReturn result,
            RepositoryData callData)
        {
            Contract.Requires<ArgumentNullException>(input  != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            try
            {
                if (result.Exception != null)
                {
                    CleanUp(callData);
                    return result;
                }

                var hasRepository = input.Target as IHasRepository;

                if (hasRepository == null)
                {
                    CleanUp(callData);
                    return result;
                }

                callData.AsyncRepository = hasRepository.AsyncRepository;

                if (callData.AsyncRepository == null)
                {
                    callData.Repository = hasRepository.Repository;
                    CommitChanges(callData);
                }

                // at this point we have async repository, we have a transaction scope, and we do not have exceptions.
                var returnedTask = result.ReturnValue as Task;
                var returnedTaskResultType = returnedTask.GetType().IsGenericType
                                                    ? returnedTask.GetType().GetGenericArguments()[0]
                                                    : typeof(bool);
                var gmi = _miDoInvokeAsyncGeneric.MakeGenericMethod(returnedTaskResultType);

                return input.CreateMethodReturn(
                                    gmi.Invoke(this, new object[] { returnedTask, input, callData }));
            }
            catch (Exception x)
            {
                CleanUp(callData);
                return ProcessException(input, x);
            }
        }

        static readonly MethodInfo _miDoInvokeAsyncGeneric = typeof(PerCallContextRepositoryCallHandler)
                                                                    .GetMethod(nameof(DoInvokeAsyncGeneric), BindingFlags.NonPublic|BindingFlags.Instance);

        async Task<T> DoInvokeAsyncGeneric<T>(
            Task<T> returnedTask,
            RepositoryData callData)
        {
            Contract.Requires<ArgumentNullException>(returnedTask != null, nameof(returnedTask));

            Contract.Ensures(Contract.Result<Task<T>>() != null);

            // await the actions in the handlers in the pipeline after this to finish their tasks
            var result = await returnedTask;

            await CommitChangesAsync(callData);
            CleanUp(callData);

            return result;
        }

        void CommitChanges(
            RepositoryData callData)
        {
            callData.Repository?.CommitChanges();
            callData.TransactionScope?.Complete();
        }

        async Task CommitChangesAsync(
            RepositoryData callData)
        {
            await callData.AsyncRepository?.CommitChangesAsync();
            callData.TransactionScope?.Complete();
        }

        void CleanUp(
            RepositoryData callData)
        {
            callData.Repository?.Dispose();
            callData.AsyncRepository?.Dispose();
            callData.TransactionScope?.Dispose();
        }

        IMethodReturn ProcessException(
            IMethodInvocation input,
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(input     != null, nameof(input));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            var aggregateException = exception as AggregateException;

            if (aggregateException != null  &&  aggregateException.InnerExceptions.Count == 1)
                exception = aggregateException.InnerExceptions[0];

            exception = exception.IsTransient()  ||  exception.IsOptimisticConcurrency()
                            ? exception = new RepeatableOperationException(exception)  // wrap and rethrow
                            : exception;

            // add more info to the exception
            exception.Data.Add("ServiceMethod", input.Target.GetType().ToString()+'.'+input.MethodBase.Name);

            return input.CreateExceptionMethodReturn(exception);
        }
    }
}
