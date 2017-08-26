using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities.Diagnostics;
using vm.Aspects.Threading;

namespace vm.Aspects
{
    /// <summary>
    /// Class ContainerRegistrar. Base class for registrars which register types and instances in the Unity DI container with code.
    /// </summary>
    [ContractClass(typeof(ContainerRegistrarContract))]
    public abstract class ContainerRegistrar : IDisposable, IIsDisposed
    {
        readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly ISet<int> _containerHashes = new SortedSet<int>();

        /// <summary>
        /// Tests if the types and instances are registered.
        /// </summary>
        /// <param name="container">The container for which the test must be run.</param>
        /// <returns><see langword="true" /> if XXXX, <see langword="false" /> otherwise.</returns>
        [Pure]
        public bool AreRegistered(
            IUnityContainer container = null)
        {
            using (_sync.ReaderLock())
                return _containerHashes.Contains((container ?? DIContainer.Root).GetHashCode());
        }

        /// <summary>
        /// Resets the <see cref="AreRegistered"/> property. Use for testing only.
        /// </summary>
        [Conditional("TEST")]
        public virtual void Reset(
            IUnityContainer container = null)
        {
            Contract.Ensures(!AreRegistered(container));

            using (_sync.UpgradableReaderLock())
            {
                var code = (container ?? DIContainer.Root).GetHashCode();

                if (_containerHashes.Contains(code))
                    using (_sync.WriterLock())
                        _containerHashes.Remove(code);
            }
        }

        /// <summary>
        /// Registers with code a number of types and instances (perhaps if they are not already registered by other means) 
        /// in the specified Unity container. This method is thread safe but somewhat slower than <see cref="UnsafeRegister"/>.
        /// </summary>
        /// <param name="container">
        /// The container where to register the defaults. If <see langword="null"/>, defaults to <see cref="P:DIContainer.Root"/>.
        /// </param>
        /// <param name="isTest">
        /// Set to true for test DI configuration.
        /// </param>
        /// <returns>The container.</returns>
        public IUnityContainer Register(
            IUnityContainer container,
            bool isTest = false)
        {
            Contract.Ensures(AreRegistered(container));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (container == null)
                container = DIContainer.Initialize();

            if (AreRegistered(container))
                return container;

            lock (container)
                return UnsafeRegister(
                            container,
                            container.GetRegistrationsSnapshot(),
                            isTest);
        }

        /// <summary>
        /// Registers with code a number of types and instances (perhaps if they are not already registered by other means) 
        /// in the specified Unity container. 
        /// The method is <b>not</b> thread safe and should be called from a synchronized context.
        /// </summary>
        /// <param name="container">
        /// The container where to register the defaults. If <see langword="null"/>, defaults to <see cref="P:DIContainer.Root"/>.
        /// </param>
        /// <param name="registrations">
        /// The registrations dictionary used for faster lookup of the existing registrations.
        /// </param>
        /// <param name="isTest">
        /// Set to true for test DI configuration.
        /// </param>
        /// <returns>The container.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> is <see langword="null"/> or
        /// <paramref name="registrations"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// This method is <b>not</b> thread safe.
        /// By convention this method is meant to be called from the context of a <c>lock</c> statement with parameter the container itself, e.g.
        /// <code>
        /// <![CDATA[
        /// class MyRegistrar1 : ContainerRegistrar
        /// {
        ///     ...
        ///     protected override void DoRegister(IUnityContainer container) { ... }   
        /// }
        /// 
        /// class MyRegistrar2 : ContainerRegistrar
        /// {
        ///     ...
        ///     protected override void DoRegister(IUnityContainer container) { ... }   
        /// }
        /// 
        /// ...
        /// lock (DIContainer.Initialize())
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationsSnapshot();
        ///     
        ///     MyRegistrar1.UnsafeRegister(DIContainer.Root, registrations);
        ///     MyRegistrar2.UnsafeRegister(DIContainer.Root, registrations);
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </remarks>
        public IUnityContainer UnsafeRegister(
            IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            bool isTest = false)
        {
            Contract.Ensures(AreRegistered(container));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (container == null)
                container = DIContainer.Initialize();

            if (AreRegistered(container))
                return container;

            if (registrations == null)
                registrations = container.GetRegistrationsSnapshot();

            if (isTest)
                DoTestRegister(container, registrations);
            else
                DoRegister(container, registrations);

            using (_sync.WriterLock())
                _containerHashes.Add(container.GetHashCode());

            VmAspectsEventSource.Log.Registered(this);

            return container;
        }

        /// <summary>
        /// Does the actual work of the registration.
        /// The method is called from a synchronized context, i.e. does not need to be thread safe.
        /// </summary>
        /// <param name="container">The container where to register the defaults.</param>
        /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
        protected abstract void DoRegister(
            IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations);

        /// <summary>
        /// The inheriting types should override this method if they need to register different configuration for unit testing purposes.
        /// The default implementation calls <see cref="DoRegister"/>.
        /// The method is called from a synchronized context, i.e. does not need to be thread safe.
        /// </summary>
        /// <param name="container">The container where to register the defaults.</param>
        /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
        protected virtual void DoTestRegister(
            IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));

            DoRegister(container, registrations);
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <see langword="true"/> if the object has already been disposed, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ContainerRegistrar"/> class.
        /// </summary>
        ~ContainerRegistrar()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the finalizer.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <see langword="false"/> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing)
                _sync.Dispose();
        }
        #endregion
    }

    [ContractClassFor(typeof(ContainerRegistrar))]
    abstract class ContainerRegistrarContract : ContainerRegistrar
    {
        protected override void DoRegister(
            IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations)
        {
            Contract.Requires<ArgumentNullException>(container     != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));

            throw new NotImplementedException();
        }
    }
}
