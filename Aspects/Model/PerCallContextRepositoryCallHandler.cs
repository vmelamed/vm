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
    /// It is assumed that the repository is resolved from the DI container and has <see cref="T:PerCallContextLifetimeManager"/>, i.e. all
    /// resolutions for <see cref="IRepository"/> with the same resolve name in the same WCF call context will return one and the same repository object.
    /// This handler implements two post-call actions: if there are no exceptions, it commits the unit of work, otherwise rolls back the current transaction
    /// and then removes the repository's lifetime manager from the container. In other words the application developer does not need to worry about 
    /// saving changes in the repository, committing and rolling-back transactions, error handling, repository disposal, etc.
    /// </summary>
    public class PerCallContextRepositoryCallHandler : ICallHandler
    {
        /// <summary>
        /// Gets or sets the resolve name of the repository registered in the current call context.
        /// </summary>
        public string RepositoryResolveName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create transaction scope for task asynchronous method.
        /// </summary>
        public bool CreateTransactionScopeForTasks { get; set; } = true;

        #region ICallHandler Members

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

        /// <summary>
        /// Order in which the handler will be executed
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }

        #endregion

        /// <summary>
        /// Actions that take place before invoking the next handler or the target in the chain.
        /// Here it creates a transaction scope.
        /// </summary>
        /// <param name="input">The input.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The scope will be disposed in PostInvoke.")]
        protected virtual void PreInvoke(IMethodInvocation input)
        {
            if (!CreateTransactionScopeForTasks)
                return;

            if (Transaction.Current != null)
            {
                Debug.WriteLine(
                    "WARNING: Did not create transaction scope. The method {0} is called from within an existing transaction {1}/{2}. Is this intended!",
                    input.MethodBase.Name,
                    Transaction.Current.TransactionInformation.LocalIdentifier,
                    Transaction.Current.TransactionInformation.DistributedIdentifier);
                return;
            }

#if DOTNET45
            var scope = new TransactionScope();
#else
            var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
#endif

            input.InvocationContext["transactionScope"] = scope;
        }

        /// <summary>
        /// Invokes the next handler or the target in the chain.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="getNext">The get next.</param>
        /// <returns>IMethodReturn.</returns>
        protected virtual IMethodReturn DoInvoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext) => getNext().Invoke(input, getNext);

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
            if (!CreateTransactionScopeForTasks)
                return result;

            if (((MethodInfo)input.MethodBase).ReturnType.Is(typeof(Task)))
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

            var scope        = GetTransactionScope(input);
            var registration = GetRepositoryRegistration();

            try
            {
                if (result.Exception != null)
                    return result;

                var repository = registration?.LifetimeManager?.GetValue() as IRepository;

                if (repository != null)
                    repository.CommitChanges();

                scope.Complete();
            }
            catch (Exception x)
            {
                Exception ex = x.IsTransient()
                                    ? ex = new RepeatableOperationException(x)  // wrap and rethrow
                                    : x;

                ex.Data.Add("ServiceMethod", input.Target.GetType().ToString()+'.'+input.MethodBase.Name);
                result = input.CreateExceptionMethodReturn(ex);
            }
            finally
            {
                registration?.LifetimeManager?.RemoveValue();
                scope.Dispose();

                Debug.WriteLine("### PerCallContextRepositoryCallHandler PostInvokeSync disposed the repository and the transaction scope");
            }

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn PostInvokeAsync(
            IMethodInvocation input,
            IMethodReturn result)
        {
            Contract.Requires<ArgumentNullException>(input  != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            var scope        = GetTransactionScope(input);
            var registration = GetRepositoryRegistration();
            var repository   = registration?.LifetimeManager?.GetValue() as IRepository;

            try
            {
                if (result.Exception != null)
                    return result;

                if (repository == null)
                {
                    scope.Complete();
                    return result;
                }

                // at this point we have a repository, we have a transaction scope, and we do not have exceptions.

                var returnedTask = result.ReturnValue as Task;
                var returnedTaskResultType = returnedTask.GetType().IsGenericType
                                                    ? returnedTask.GetType().GetGenericArguments()[0]
                                                    : typeof(bool);
                var gmi = _miDoInvokeAsyncGeneric.MakeGenericMethod(returnedTaskResultType);

                return input.CreateMethodReturn(
                                gmi.Invoke(null, new object[] { returnedTask, input, registration, scope }));
            }
            catch (Exception x)
            {
                return input.CreateExceptionMethodReturn(x);
            }
        }

        static readonly MethodInfo _miDoInvokeAsyncGeneric = typeof(PerCallContextRepositoryCallHandler)
                                                                .GetMethod(nameof(DoInvokeAsyncGeneric), BindingFlags.NonPublic|BindingFlags.Static);

        static async Task<T> DoInvokeAsyncGeneric<T>(
            Task<T> returnedTask,
            IMethodInvocation input,
            ContainerRegistration registration,
            TransactionScope scope)
        {
            Contract.Requires<ArgumentNullException>(returnedTask != null, nameof(returnedTask));
            Contract.Requires<ArgumentNullException>(input        != null, nameof(input));
            Contract.Requires<ArgumentNullException>(registration != null, nameof(registration));
            Contract.Requires<ArgumentNullException>(scope        != null, nameof(scope));
            Contract.Ensures(Contract.Result<Task<T>>() != null);

            // find the repository and commit the changes
            try
            {
                // await the actions in the handlers in the handlers pipeline after this to finish their tasks
                var result = await returnedTask;

                if (await CommitChangesAsync(registration, scope) == null)
                    CommitChanges(registration, scope);

                return result;
            }
            catch (Exception x)
            {
                Exception ex = x;

                // flatten the aggregate exceptions if we can
                var aggregateException = ex as AggregateException;

                if (aggregateException != null  &&  aggregateException.InnerExceptions.Count == 1)
                    ex = aggregateException.InnerExceptions[0];

                if (ex.IsTransient())
                    ex = new RepeatableOperationException(ex);

                ex.Data.Add("ServiceMethod", input.Target.GetType().ToString()+'.'+input.MethodBase.Name);
                throw ex;
            }
            finally
            {
                registration?.LifetimeManager?.RemoveValue();
                scope.Dispose();

                Debug.WriteLine("### PerCallContextRepositoryCallHandler PostInvokeAsync disposed the repository and the transaction scope");
            }
        }

        static IRepository CommitChanges(
            ContainerRegistration registration,
            TransactionScope scope)
        {
            Contract.Requires<ArgumentNullException>(registration != null, nameof(registration));

            var value      = registration.LifetimeManager.GetValue();
            var repository = value as IRepository;

            if (repository != null  &&
                !(repository?.IsDisposed).GetValueOrDefault())
            {
                repository.CommitChanges();
                scope.Complete();
            }

            return repository;
        }

        static async Task<IRepositoryAsync> CommitChangesAsync(
            ContainerRegistration registration,
            TransactionScope scope)
        {
            Contract.Requires<ArgumentNullException>(registration != null, nameof(registration));

            var value      = registration.LifetimeManager.GetValue();
            var repository = value as IRepositoryAsync;

            if (repository != null  &&
                !(repository?.IsDisposed).GetValueOrDefault())
            {
                await repository.CommitChangesAsync();
                scope.Complete();
            }

            return repository;
        }

        ContainerRegistration GetRepositoryRegistration()
        {
            IDictionary<RegistrationLookup, ContainerRegistration> registrations;

            lock (DIContainer.Root)
                registrations = DIContainer.Root.GetRegistrationsSnapshot();

            // the repository registration
            ContainerRegistration registration = null;

            if (!(registrations.TryGetValue(new RegistrationLookup(typeof(IRepositoryAsync), RepositoryResolveName), out registration) ||
                  registrations.TryGetValue(new RegistrationLookup(typeof(IRepository), RepositoryResolveName), out registration)))
                return null;

            return registration;
        }

        static TransactionScope GetTransactionScope(IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Ensures(Contract.Result<TransactionScope>() != null);

            object obj;

            input.InvocationContext.TryGetValue("transactionScope", out obj);

            Contract.Assume((obj as TransactionScope) != null, "Could not find the transaction scope?");

            return obj as TransactionScope;
        }
    }
}
