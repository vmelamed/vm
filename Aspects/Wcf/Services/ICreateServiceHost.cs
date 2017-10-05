using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace vm.Aspects.Wcf.Services
{
    /// <summary>
    /// Represents the service creating behavior of the messaging pattern service host factories.
    /// </summary>
    public interface ICreateServiceHost
    {
        /// <summary>
        /// Sets an endpoint provider delegate.
        /// </summary>
        /// <param name="provideEndpoints">The provide endpoints.</param>
        /// <returns>ICreateServiceHost.</returns>
        ICreateServiceHost SetEndpointProvider(
            Func<IEnumerable<ServiceEndpoint>> provideEndpoints);

        /// <summary>
        /// Sets the service registrar which registers injection types specific to the service associated with this factory.
        /// </summary>
        /// <param name="registrar">The registration method.</param>
        ICreateServiceHost SetServiceRegistrar(
            Action<IUnityContainer, Type, IDictionary<RegistrationLookup, ContainerRegistration>> registrar);

        /// <summary>
        /// Registers the service's contract and implementation types in the DI container
        /// as well as all the facilities needed for the normal work of the services from this framework.
        /// </summary>
        /// <returns>IUnityContainer.</returns>
        IUnityContainer RegisterDefaults();

        /// <summary>
        /// Gets a value indicating whether all types and instances needed for this  this instance are registered.
        /// </summary>
        bool AreRegistered { get; }

        /// <summary>
        /// Creates a service host outside of WAS for a service type resolved internally, i.e. from DI container or hard-coded.
        /// Can be used when the service is created in a self-hosting environment or for testing purposes.
        /// </summary>
        /// <param name="baseAddresses">The <see cref="Array" /> of type <see cref="Uri" /> that contains the base addresses for the service hosted.</param>
        /// <returns>A <see cref="ServiceHost" /> for the type of service specified with a specific base address.</returns>
        ServiceHost CreateHost(
            params Uri[] baseAddresses);

        /// <summary>
        /// Represents the task of initializing the created host.
        /// </summary>
        Task<bool> InitializeHostTask { get; }
    }
}
