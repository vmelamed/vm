using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class DIInstanceProvider. This class cannot be inherited. Instantiates the service by resolving it from the DI container represented by the service locator.
    /// </summary>
    public sealed class DIInstanceProvider : IInstanceProvider
    {
        readonly Type _serviceContractType;
        readonly string _serviceResolveName;

        readonly static object _sync = new object();
        readonly static IDictionary<InstanceContext, IUnityContainer> _containers = new Dictionary<InstanceContext, IUnityContainer>();

        /// <summary>
        /// Initializes the provider with an interface type.
        /// </summary>
        /// <param name="type">
        /// The type of the interface that the objects created by this provider must implement. Can be <see langword="null"/>.
        /// </param>
        /// <param name="name">
        /// The resolution name to use when resolving the instance.
        /// </param>
        public DIInstanceProvider(
            Type type,
            string name)
        {
            // TODO: Complete member initialization
            _serviceContractType = type;
            _serviceResolveName  = name;
        }

        #region IInstanceProvider Members
        /// <summary>
        /// Creates an instance of an object suitable for injecting policies.
        /// </summary>
        /// <param name="instanceContext">The context of the service. We'll get the type of the object from it.</param>
        /// <param name="message">Ignored.</param>
        /// <returns>A service instance which is suitable for injecting policies.</returns>
        public object GetInstance(
            InstanceContext instanceContext,
            Message message)
        {
            if (instanceContext == null)
                throw new ArgumentNullException(nameof(instanceContext));
            if (instanceContext.Host == null)
                throw new ArgumentException("The instance context's property Host cannot be null.", nameof(instanceContext));

            var childContainer = DIContainer.Root.CreateChildContainer();

            lock (_sync)
            {
                Debug.Assert(!_containers.ContainsKey(instanceContext), "THERE IS A CONTAINER ASSOCIATED WITH THIS INSTANCE CONTEXT ALREADY.");
                _containers[instanceContext] = childContainer;
            }

            return CreateInstance(
                        childContainer,
                        instanceContext.Host.Description.ServiceType,
                        _serviceContractType,
                        _serviceResolveName);
        }

        /// <summary>
        /// Creates an instance of an object suitable for injecting policies.
        /// </summary>
        /// <param name="instanceContext">The context of the service. We will get the type of the object from it.</param>
        /// <returns>A service instance which is suitable for injecting policies.</returns>
        public object GetInstance(
            InstanceContext instanceContext) => GetInstance(instanceContext, null);

        /// <summary>
        /// Performs any clean-up on the created above instances.
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
            IUnityContainer childContainer = null;

            lock (_sync)
            {
                _containers.TryGetValue(instanceContext, out childContainer);
                _containers.Remove(instanceContext);
            }

            Debug.Assert(childContainer != null, "THERE IS NO CONTAINER ASSOCIATED WITH THIS INSTANCE CONTEXT.");
            childContainer.Dispose();
        }
        #endregion

        /// <summary>
        /// Creates a DI resolved instance (of a service) with dynamic binding to the type and the interface of the object.
        /// </summary>
        /// <param name="container">The DI container.</param>
        /// <param name="serviceType">The type of the created object (service).</param>
        /// <param name="serviceContract">The type of the interface (service contract).</param>
        /// <param name="serviceResolveName">The DI resolve name of the service.</param>
        /// <returns>A reference to the newly created instance (service).</returns>
        /// <exception cref="System.ArgumentException">If no service interface is specified, the service type must be derived from the class MarshalByRefObject.;serviceType</exception>
        /// <exception cref="System.ArgumentNullException">serviceType</exception>
        /// <exception cref="ArgumentNullException">If no service interface is specified, the service type must be derived from the class MarshalByRefObject.;serviceType</exception>
        /// <exception cref="ArgumentException">The <paramref name="serviceType" /> is <see langword="null" />.</exception>
        object CreateInstance(
            IUnityContainer container,
            Type serviceType,
            Type serviceContract,
            string serviceResolveName)
        {
            Contract.Requires<ArgumentNullException>(serviceType != null, nameof(serviceType));
            Contract.Requires<ArgumentException>((serviceContract == null || serviceType.GetInterface(serviceContract.Name) != null)  &&
                                                 (serviceContract != null || typeof(MarshalByRefObject).IsAssignableFrom(serviceType)), "If no service interface is specified, the service type must be derived from System.MarshalByRefObject.");

            try
            {
                // the object must either implement the interface cached in serviceContract or
                // must inherit from MarshalByRefObject. Otherwise we cannot get a transparent proxy!
                if (serviceContract != null && serviceType.GetInterface(serviceContract.Name) == null  ||
                    serviceContract == null && !typeof(MarshalByRefObject).IsAssignableFrom(serviceType))
                    throw new ArgumentException("If no service interface is specified, the service type must be derived from System.MarshalByRefObject.", nameof(serviceType));

                return serviceContract!=null
                            ? container.Resolve(serviceContract, serviceResolveName)
                            : container.Resolve(serviceType, serviceResolveName);
            }
            catch (Exception x)
            {
                Facility.LogWriter.ExceptionCritical(x);
                throw;
            }
        }
    }
}
