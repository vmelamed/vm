using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities;
using vm.Aspects.Threading;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class DIInstanceProvider. This class cannot be inherited. Instantiates the service by resolving it from the DI container represented by the service locator.
    /// </summary>
    public sealed class DIInstanceProvider : IInstanceProvider
    {
        readonly Type _serviceContractType;
        readonly string _serviceResolveName;
        readonly bool _useRootContainer;

        readonly static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly static IDictionary<InstanceContext, IUnityContainer> _containers = new Dictionary<InstanceContext, IUnityContainer>();

        /// <summary>
        /// Initializes the provider with an interface type.
        /// </summary>
        /// <param name="serviceContractType">The type of the interface that the objects created by this provider must implement. Can be <see langword="null" />.</param>
        /// <param name="serviceResolveName">The resolution name to use when resolving the instance.</param>
        /// <param name="useRootContainer">
        /// If set to <see langword="false"/> (the default) the instance provider will create a child container off of the root container and 
        /// the service and all of its dependencies will be resolved from that child container. Upon service instance release, the container, 
        /// the service and all dependencies with <see cref="HierarchicalLifetimeManager"/> will be disposed.
        /// If set to <see langword="true"/> the instance provider will resolve the service instance from the root container, 
        /// upon release the service will be disposed and it is responsible for the disposal of its dependencies.
        /// </param>
        public DIInstanceProvider(
            Type serviceContractType,
            string serviceResolveName,
            bool useRootContainer = false)
        {
            // TODO: Complete member initialization
            _serviceContractType = serviceContractType;
            _serviceResolveName  = serviceResolveName;
            _useRootContainer    = useRootContainer;
        }

        #region IInstanceProvider Members
        /// <summary>
        /// Creates an instance of an object suitable for injecting dependencies and policies/aspects.
        /// </summary>
        /// <param name="instanceContext">The context of the service. We'll get the type of the object from it.</param>
        /// <param name="message">Ignored.</param>
        /// <returns>A service instance which is suitable for injecting dependencies and policies/aspects.</returns>
        public object GetInstance(
            InstanceContext instanceContext,
            Message message)
        {
            if (instanceContext == null)
                throw new ArgumentNullException(nameof(instanceContext));
            if (instanceContext.Host == null)
                throw new ArgumentException("The instance context's property Host cannot be null.", nameof(instanceContext));

            IUnityContainer container;

            if (_useRootContainer)
                container = DIContainer.Root;
            else
            {
                container = DIContainer.Root.CreateChildContainer();

                using (_sync.WriterLock())
                {
                    Debug.Assert(!_containers.ContainsKey(instanceContext), "THERE IS A CONTAINER ASSOCIATED WITH THIS INSTANCE CONTEXT ALREADY.");
                    _containers[instanceContext] = container;
                }
            }

            var serviceType = instanceContext.Host.Description.ServiceType;

            try
            {
                // the object must either implement the interface cached in _serviceContractType or
                // must inherit from MarshalByRefObject. Otherwise we cannot get a transparent proxy suitable for injecting policies/aspects!
                if (_serviceContractType != null && serviceType.GetInterface(_serviceContractType.Name) == null  ||
                    _serviceContractType == null && !typeof(MarshalByRefObject).IsAssignableFrom(serviceType))
                    throw new ArgumentException("If no service interface is specified, the service type must be derived from System.MarshalByRefObject.");

                return _serviceContractType!=null
                            ? container.Resolve(_serviceContractType, _serviceResolveName)
                            : container.Resolve(serviceType, _serviceResolveName);
            }
            catch (Exception x)
            {
                Facility.LogWriter.ExceptionCritical(x);
                throw;
            }
        }

        /// <summary>
        /// Creates an instance of an object suitable for injecting dependencies and policies/aspects.
        /// </summary>
        /// <param name="instanceContext">The context of the service. We will get the type of the object from it.</param>
        /// <returns>A service instance which is suitable for injecting dependencies and policies/aspects.</returns>
        public object GetInstance(
            InstanceContext instanceContext) => GetInstance(instanceContext, null);

        /// <summary>
        /// Performs any clean-up on the created instances above.
        /// </summary>
        /// <param name="instanceContext">Ignored.</param>
        /// <param name="instance">The object that we have to clean-up about.</param>
        /// <remarks>
        /// If the object supports <see cref="IDisposable"/> the method calls <c>Dispose</c> on it.
        /// </remarks>
        public void ReleaseInstance(
            InstanceContext instanceContext,
            object instance)
        {
            IUnityContainer container = null;

            instance.Dispose();
            if (!_useRootContainer)
            {
                using (_sync.WriterLock())
                {
                    _containers.TryGetValue(instanceContext, out container);
                    _containers.Remove(instanceContext);
                }

                Debug.Assert(container != null, "THERE IS NO CONTAINER ASSOCIATED WITH THIS INSTANCE CONTEXT.");
                container.Dispose();
            }
        }
        #endregion

        /// <summary>
        /// Gets the current container if present, otherwise the root container
        /// </summary>
        public static IUnityContainer CurrentContainer
        {
            get
            {
                IUnityContainer current;
                var instanceContext = OperationContext.Current?.InstanceContext;

                if (instanceContext != null)
                    using (_sync.ReaderLock())
                        if (_containers.TryGetValue(instanceContext, out current))
                            return current;

                return DIContainer.Root;
            }
        }
    }
}
