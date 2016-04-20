using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Exceptions;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model
{
    /// <summary>
    /// The class PerCallContextRepositoryCallHandler is meant to be used as a policy (AOP aspect) in the call context of a WCF call.
    /// It is assumed that the repository is resolved from the DI container and has <see cref="PerCallContextLifetimeManager"/>, i.e. all
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
        /// Gets or sets a value indicating whether to create transactionn scope for task asynchronous method.
        /// </summary>
        public bool CreateTransactionnScopeForTasks { get; set; } = true;

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

        void PreInvoke(IMethodInvocation input)
        {
            if (!CreateTransactionnScopeForTasks  ||
                !((MethodInfo)input.MethodBase).ReturnType.Is(typeof(Task)))
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

            var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            input.InvocationContext["transactionScope"] = scope;
        }

        IMethodReturn DoInvoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext) => getNext().Invoke(input, getNext);

        IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn result)
        {
            if (result.ReturnValue is Task)
                return PostInvokeAsync(input, result);
            else
                return PostInvokeSync(input, result);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn PostInvokeSync(
            IMethodInvocation input,
            IMethodReturn result)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            var scope = GetTransactionScope(input);
            var registration = GetRepositoryRegistration();

            try
            {
                var repository = registration?.LifetimeManager?.GetValue() as IRepository;

                if (repository == null)
                    return result;

                if (result.Exception == null)
                    repository.CommitChanges();

                scope?.Complete();
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
                scope?.Dispose();
            }

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn PostInvokeAsync(
            IMethodInvocation input,
            IMethodReturn result)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            var scope = GetTransactionScope(input);
            var registration = GetRepositoryRegistration();

            if (registration == null)
            {
                scope?.Dispose();
                return result;
            }

            if (result.Exception != null)
            {
                registration.LifetimeManager.RemoveValue();
                scope?.Dispose();
                return result;
            }

            try
            {
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
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(registration != null, nameof(registration));
            Contract.Ensures(Contract.Result<Task<T>>() != null);

            var repo = registration.LifetimeManager.GetValue();

            try
            {
                var result = await returnedTask;
                var asyncRepository = repo as IRepositoryAsync;

                if (asyncRepository != null)
                    await asyncRepository.CommitChangesAsync();
                else
                    (repo as IRepository)?.CommitChanges();

                scope?.Complete();
                return result;
            }
            catch (Exception x)
            {
                Exception ex;

                if (x.IsTransient())
                    // wrap and rethrow
                    ex = new RepeatableOperationException(x);
                else
                    ex = x;

                ex.Data.Add("ServiceMethod", input.Target.GetType().ToString()+'.'+input.MethodBase.Name);
                throw ex;
            }
            finally
            {
                registration.LifetimeManager.RemoveValue();
                scope?.Dispose();
            }
        }

        ContainerRegistration GetRepositoryRegistration()
        {
            IDictionary<RegistrationLookup, ContainerRegistration> registrations;

            lock (DIContainer.Root)
                registrations = DIContainer.Root.GetRegistrationsSnapshot();

            // the repository registration
            ContainerRegistration registration = null;

            if (!(registrations.TryGetValue(new RegistrationLookup(typeof(IRepositoryAsync), RepositoryResolveName), out registration) ||
                  registrations.TryGetValue(new RegistrationLookup(typeof(IRepository), RepositoryResolveName), out registration)) ||
                !(registration.LifetimeManager is PerCallContextLifetimeManager))
                return null;

            return registration;
        }

        static TransactionScope GetTransactionScope(IMethodInvocation input)
        {
            object obj;

            input.InvocationContext.TryGetValue("transactionScope", out obj);

            return obj as TransactionScope;
        }
    }
}
