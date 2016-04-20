using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;
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

            if (((MethodInfo)input.MethodBase).ReturnType.Is(typeof(Task)))
                return InvokeAsync(input, getNext);
            else
                return InvokeSync(input, getNext);
        }

        /// <summary>
        /// Order in which the handler will be executed
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }

        #endregion

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn InvokeSync(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            Contract.Requires<ArgumentNullException>(getNext != null, nameof(getNext));

            var result = getNext().Invoke(input, getNext);

            IDictionary<RegistrationLookup, ContainerRegistration> registrations;

            lock (DIContainer.Root)
                registrations = DIContainer.Root.GetRegistrationsSnapshot();

            // find the corresponding registration
            ContainerRegistration registration;

            if (!registrations.TryGetValue(new RegistrationLookup(typeof(IRepository), RepositoryResolveName), out registration) ||
                !(registration.LifetimeManager is PerCallContextLifetimeManager))
                return result;

            var repository = registration.LifetimeManager.GetValue() as IRepository;

            if (repository == null)
                return result;

            try
            {
                if (result.Exception == null)
                    repository.CommitChanges();
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
                registration.LifetimeManager.RemoveValue();
            }

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        IMethodReturn InvokeAsync(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            Contract.Requires<ArgumentNullException>(getNext != null, nameof(getNext));

            var result = getNext().Invoke(input, getNext);

            IDictionary<RegistrationLookup, ContainerRegistration> registrations;

            lock (DIContainer.Root)
                registrations = DIContainer.Root.GetRegistrationsSnapshot();

            // the repository registration
            ContainerRegistration registration = null;

            if (!(registrations.TryGetValue(new RegistrationLookup(typeof(IRepositoryAsync), RepositoryResolveName), out registration) ||
                  registrations.TryGetValue(new RegistrationLookup(typeof(IRepository), RepositoryResolveName), out registration)) ||
                !(registration.LifetimeManager is PerCallContextLifetimeManager))
                return result;

            if (result.Exception != null)
            {
                registration.LifetimeManager.RemoveValue();
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
                                gmi.Invoke(null, new object[] { returnedTask, input, registration }));
            }
            catch (Exception x)
            {
                return input.CreateExceptionMethodReturn(x);
            }
        }

        static MethodInfo _miDoInvokeAsyncGeneric = typeof(PerCallContextRepositoryCallHandler)
                                                            .GetMethod(nameof(DoInvokeAsyncGeneric), BindingFlags.NonPublic|BindingFlags.Static);

        static async Task<T> DoInvokeAsyncGeneric<T>(
            Task<T> returnedTask,
            IMethodInvocation input,
            ContainerRegistration registration)
        {
            try
            {
                var result = await returnedTask;
                var asyncRepository = registration.LifetimeManager.GetValue() as IRepositoryAsync;

                if (asyncRepository != null)
                    await asyncRepository.CommitChangesAsync();
                else
                {
                    var repository = registration.LifetimeManager.GetValue() as IRepository;

                    if (repository != null)
                        repository.CommitChanges();
                }

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
            }
        }
    }
}
