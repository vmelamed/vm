using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
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

            var result = getNext().Invoke(input, getNext);

            if (result.Exception != null)
                return result;

            if (result.ReturnValue is Task)
                return CommitWorkAsync(input, result);
            else
                return CommitWork(input, result);
        }

        /// <summary>
        /// Order in which the handler will be executed
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }

        #endregion

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "It is re-thrown.")]
        IMethodReturn CommitWork(
            IMethodInvocation input,
            IMethodReturn result)
        {
            IDictionary<RegistrationLookup, ContainerRegistration> registrations;

            lock (DIContainer.Root)
                registrations = DIContainer.Root.GetRegistrationsSnapshot();

            // find the corresponding registration
            ContainerRegistration registration;

            if (!registrations.TryGetValue(new RegistrationLookup(typeof(IRepository), RepositoryResolveName), out registration) ||
                !(registration.LifetimeManager is PerCallContextLifetimeManager))
                return result;

            try
            {
                var repository = registration.LifetimeManager.GetValue() as IRepository;

                if (repository == null)
                    return result;

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

        class DoCommitWorkAsyncParameters
        {
            public IMethodInvocation Input { get; set; }
            public Task NextTask { get; set; }
            public Type ReturnType { get; set; }
            public bool IsVoid { get { return ReturnType == null; } }
        }

        IMethodReturn CommitWorkAsync(
            IMethodInvocation input,
            IMethodReturn result)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));

            var parameters = new DoCommitWorkAsyncParameters
            {
                Input      = input,
                NextTask   = result.ReturnValue as Task,
                ReturnType = result.ReturnValue.GetType().IsGenericType ? result.ReturnValue.GetType().GetGenericArguments()[0] : null,
            };

            // the return type of the asynchronous method that is wrapped in the task, i.e. the T in Task<T>
            var returnType = parameters.IsVoid
                                ? typeof(bool)
                                : parameters.ReturnType;

            // the type of the method wrapped in the applied aspect is Func<DoCommitWorkAsyncParameters, T>
            var commitWorkAsyncGeneric = typeof(Func<,>).MakeGenericType(typeof(DoCommitWorkAsyncParameters), returnType);

            // create a delegate out of an instantiated DoCommitWorkAsync<T>
            var doCommitWorkAsyncDelegate = GetType().GetMethod(nameof(DoCommitWorkAsync))
                                                     .MakeGenericMethod(returnType)
                                                     .CreateDelegate(commitWorkAsyncGeneric);
            // pass the delegate to a Task<T> c-tor
            var wrappedTask = typeof(Task).MakeGenericType(returnType)
                                          .GetConstructor(new Type[] { commitWorkAsyncGeneric })
                                          .Invoke(new object[]
                                                  {
                                                      doCommitWorkAsyncDelegate,
                                                      parameters,
                                                  }) as Task;
            wrappedTask.Start();
            return input.CreateMethodReturn(wrappedTask);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called via reflection.")]
        async Task<T> DoCommitWorkAsync<T>(
            DoCommitWorkAsyncParameters parameters)
        {
            T returnValue = default(T);
            // find the corresponding registration
            ContainerRegistration registration = null;

            try
            {
                if (parameters.IsVoid)
                    await parameters.NextTask;
                else
                    returnValue = await ((Task<T>)parameters.NextTask);

                // find the repository type registration
                IDictionary<RegistrationLookup, ContainerRegistration> registrations;

                lock (DIContainer.Root)
                    registrations = DIContainer.Root.GetRegistrationsSnapshot();

                if (!(registrations.TryGetValue(new RegistrationLookup(typeof(IRepositoryAsync), RepositoryResolveName), out registration) &&
                      registrations.TryGetValue(new RegistrationLookup(typeof(IRepository), RepositoryResolveName), out registration)) ||
                    !(registration.LifetimeManager is PerCallContextLifetimeManager))
                    return returnValue;

                var asyncRepository = registration.LifetimeManager.GetValue() as IRepositoryAsync;
                var repository = asyncRepository ?? (registration.LifetimeManager.GetValue() as IRepository);

                if (repository == null)
                    return returnValue;

                if (asyncRepository != null)
                    await asyncRepository.CommitChangesAsync();
                else
                    repository.CommitChanges();
            }
            catch (Exception x)
            {
                var ex = x;

                if (x.IsTransient())
                    // wrap and rethrow
                    ex = new RepeatableOperationException(x);
                else
                    ex = x;

                ex.Data.Add("ServiceMethod", parameters.Input.Target.GetType().ToString()+'.'+parameters.Input.MethodBase.Name);
            }
            finally
            {
                if (registration != null && (registration.LifetimeManager is PerCallContextLifetimeManager))
                    registration.LifetimeManager.RemoveValue();
            }

            return returnValue;
        }
    }
}
