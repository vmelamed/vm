using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using vm.Aspects.Exceptions;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model
{
    /// <summary>
    /// The class PerCallContextRepositoryCallHandler is meant to be used as a policy (AOP aspect) in the call context of a WCF call.
    /// It is assumed that the repository is resolved from the DI container and has <see cref="PerCallContextLifetimeManager"/>, i.e. all
    /// resolutions for <see cref="IRepository"/> with the same resolve name in the same WCF call context will return one and the same repository object.
    /// This handler implements two post-call actions: if there are no exceptions, it calls <see cref="IRepository.CommitChanges"/> to commit the unit 
    /// of work, otherwise rolls back the current transaction and then removes the repository's lifetime manager from the container. In other words,
    /// the application developer does not need to worry about saving changes in the repository, committing and rolling-back transactions, 
    /// error handling, repository disposal, etc.
    /// </summary>
    public class PerCallContextRepositoryCallHandler : ICallHandler
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
        /// Gets or sets the explicitly created transaction scope.
        /// </summary>
        TransactionScope TransactionScope { get; set; }
        /// <summary>
        /// Gets or sets the repository registration.
        /// </summary>
        ContainerRegistration RepositoryRegistration { get; set; }
        /// <summary>
        /// Gets or sets the synchronous repository.
        /// </summary>
        IRepository Repository { get; set; }
        /// <summary>
        /// Gets or sets the asynchronous repository.
        /// </summary>
        IRepositoryAsync AsyncRepository { get; set; }

        #region ICallHandler Members
        /// <summary>
        /// Order in which the handler will be executed
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }

        /// <summary>
        /// After the call the handler searches in the registrations for instantiated IRepository object with <see cref="T:PerCallContextLifetimeManager"/>; 
        /// commits the changes; and disposes the repository. The last step is needed because the call context saves the custom values in a TLS storage and 
        /// when the thread is reused a stale repository object will be returned on resolve.
        /// </summary>
        /// <param name="input">Inputs to the current call to the target.</param>
        /// <param name="getNext">Delegate to execute to get the next delegate in the handler
        /// chain.</param>
        /// <returns>Return value from the target.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when either <paramref name="input"/> or <paramref name="getNext"/> are <see langword="null"/>-s.</exception>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "It's OK here.")]
        public IMethodReturn Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (getNext == null)
                throw new ArgumentNullException(nameof(getNext));

            PreInvoke(input);

            var result = DoInvoke(input, getNext);

            return PostInvoke(input, result);
        }
        #endregion

        /// <summary>
        /// Actions that take place before invoking the next handler or the target in the chain.
        /// Here it creates a transaction scope.
        /// </summary>
        /// <param name="input">The input.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The scope will be disposed in PostInvoke.")]
        protected virtual void PreInvoke(
            IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            // find the repository registration
            IDictionary<RegistrationLookup, ContainerRegistration> registrations;

            lock (DIContainer.Root)
                registrations = DIContainer.Root.GetRegistrationsSnapshot();

            // get the repository registration
            ContainerRegistration registration = null;

            if (registrations.TryGetValue(new RegistrationLookup(typeof(IRepositoryAsync), RepositoryResolveName), out registration)  ||
                registrations.TryGetValue(new RegistrationLookup(typeof(IRepository), RepositoryResolveName), out registration))
                RepositoryRegistration = registration;

            // set the repository properties
            var value = RepositoryRegistration.LifetimeManager.GetValue();

            Contract.Assume(value != null);

            Repository = value as IRepository;
            if (Repository == null)
            {
                AsyncRepository = value as IRepositoryAsync;
                if (AsyncRepository != null)
                    Repository = AsyncRepository as IRepository;
            }

            Contract.Assume(Repository != null  ||  AsyncRepository != null);

            // open the transaction scope if necessary
            if (CreateTransactionScope)
            {
                if (Transaction.Current != null)
                {
                    Debug.WriteLine(
                        "WARNING: Did not create transaction scope. The method {0} is called in the context of an existing transaction {1}/{2}. Is this intended!",
                        input.MethodBase.Name,
                        Transaction.Current.TransactionInformation.LocalIdentifier,
                        Transaction.Current.TransactionInformation.DistributedIdentifier);
                    return;
                }

                TransactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                Repository.EnlistInAmbientTransaction();
            }
        }

        /// <summary>
        /// Invokes the next handler or the target in the chain.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="getNext">The get next.</param>
        /// <returns>IMethodReturn.</returns>
        protected virtual IMethodReturn DoInvoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(getNext != null, nameof(getNext));

            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            return getNext().Invoke(input, getNext);
        }

        /// <summary>
        /// Actions that take place after invoking the next handler or the target in the chain.
        /// Here it saves all changes in the IRepository instance which lifetime is managed per call context;
        /// commits the transaction scope if there are no exceptions;
        /// and disposes the IRepository instance.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="result">The result.</param>
        /// <returns>IMethodReturn.</returns>
        protected virtual IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn result)
        {
            Contract.Requires<ArgumentNullException>(input  != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));

            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            if (((MethodInfo)input.MethodBase).ReturnType.Is<Task>())
                return PostInvokeAsync(input, result);
            else
                return PostInvokeSync(input, result);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn PostInvokeSync(
            IMethodInvocation input,
            IMethodReturn result)
        {
            Contract.Requires<ArgumentNullException>(input  != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            try
            {
                if (result.Exception == null)
                    CommitChanges();

                return result;
            }
            catch (Exception x)
            {
                return ProcessException(input, x);
            }
            finally
            {
                CleanUp();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn PostInvokeAsync(
            IMethodInvocation input,
            IMethodReturn result)
        {
            Contract.Requires<ArgumentNullException>(input  != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            try
            {
                if (result.Exception != null)
                    return result;

                // at this point we have a repository, we have a transaction scope, and we do not have exceptions.
                var returnedTask = result.ReturnValue as Task;
                var returnedTaskResultType = returnedTask.GetType().IsGenericType
                                                    ? returnedTask.GetType().GetGenericArguments()[0]
                                                    : typeof(bool);
                var gmi = _miDoInvokeAsyncGeneric.MakeGenericMethod(returnedTaskResultType);

                return input.CreateMethodReturn(
                                    gmi.Invoke(this, new object[] { returnedTask, input }));
            }
            catch (Exception x)
            {
                return ProcessException(input, x);
            }
            finally
            {
                CleanUp();
            }
        }

        static readonly MethodInfo _miDoInvokeAsyncGeneric = typeof(PerCallContextRepositoryCallHandler)
                                                                .GetMethod(nameof(DoInvokeAsyncGeneric), BindingFlags.NonPublic|BindingFlags.Instance);

        async Task<T> DoInvokeAsyncGeneric<T>(
            Task<T> returnedTask,
            IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(returnedTask != null, nameof(returnedTask));
            Contract.Requires<ArgumentNullException>(input        != null, nameof(input));

            Contract.Ensures(Contract.Result<Task<T>>() != null);

            // await the actions in the handlers in the pipeline after this to finish their tasks
            var result = await returnedTask;

            await CommitChangesAsync();
            return result;
        }

        void CommitChanges()
        {
            Repository.CommitChanges();
            TransactionScope?.Complete();
        }

        async Task CommitChangesAsync()
        {
            if (AsyncRepository != null)
                await AsyncRepository.CommitChangesAsync();
            else
                Repository.CommitChanges();

            TransactionScope?.Complete();
        }

        void CleanUp()
        {
            // dispose the repository
            RepositoryRegistration?.LifetimeManager?.RemoveValue();

            // dispose the transaction scope
            TransactionScope?.Dispose();
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
