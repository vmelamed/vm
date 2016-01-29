using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class DIInstanceProvider. This class cannot be inherited. Instantiates the service by resolving it from the DI container represented by the service locator.
    /// </summary>
    public sealed class DIInstanceProvider : IInstanceProvider
    {
        readonly Type _serviceContractType;
        readonly string _serviceResolveName;

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
            if (instanceContext==null)
                throw new ArgumentNullException("instanceContext");
            if (instanceContext.Host==null)
                throw new ArgumentException("The instance context's property Host cannot be null.");

            return CreateInstance(
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
            InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

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
            IDisposable disposable = instance as IDisposable;

            if (disposable != null)
                disposable.Dispose();
        }
        #endregion

        /// <summary>
        /// Creates a DI resolved instance (of a service) with dynamic binding to the type and the interface of the object.
        /// </summary>
        /// <param name="serviceType">The type of the created object (service).</param>
        /// <param name="serviceContract">The type of the interface (service contract).</param>
        /// <param name="serviceResolveName">The DI resolve name of the service.</param>
        /// <returns>A reference to the newly created instance (service).</returns>
        /// <exception cref="System.ArgumentNullException">serviceType</exception>
        /// <exception cref="System.ArgumentException">If no service interface is specified, the service type must be derived from the class MarshalByRefObject.;serviceType</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceType" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><list type="bullet">
        ///   <item>
        /// The <paramref name="serviceContract" /> is <see langword="null" /> and the class <paramref name="serviceType" />
        /// does not inherit from <see cref="MarshalByRefObject" /> or
        /// </item>
        ///   <item>
        ///     <paramref name="serviceContract" /> is not <see langword="null" /> but is not implemented by <paramref name="serviceType" />.
        /// </item>
        /// </list></exception>
        static object CreateInstance(
            Type serviceType,
            Type serviceContract,
            string serviceResolveName)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            // the object must either implement the interface cached in serviceContract or
            // must inherit from MarshalByRefObject. Otherwise we cannot get a transparent proxy!
            if (serviceContract != null && serviceType.GetInterface(serviceContract.Name) == null  ||
                serviceContract == null && !typeof(MarshalByRefObject).IsAssignableFrom(serviceType))
                throw new ArgumentException("If no service interface is specified, the service type must be derived from the class MarshalByRefObject.", "serviceType");

            if (serviceResolveName == null)
                serviceResolveName = string.Empty;

            return serviceContract!=null
                                    ? ServiceLocator.Current.GetInstance(serviceContract, serviceResolveName)
                                    : ServiceLocator.Current.GetInstance(serviceType, serviceResolveName);
        }
    }
}
