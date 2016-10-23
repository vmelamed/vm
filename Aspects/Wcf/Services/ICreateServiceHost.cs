using System;
using System.Diagnostics.Contracts;
using System.ServiceModel;
using System.Threading.Tasks;

namespace vm.Aspects.Wcf.Services
{
    /// <summary>
    /// Represents the service creating behavior of the messaging pattern service host factories.
    /// </summary>
    [ContractClass(typeof(ICreateServiceHostContract))]
    public interface ICreateServiceHost
    {
        /// <summary>
        /// Creates a service host outside of WAS where the created service is specified by <see cref="Type"/>.
        /// Can be used when the service is created in a self-hosting environment or for testing purposes.
        /// </summary>
        /// <param name="serviceType">Specifies the type of service to be hosted.
        /// Can be <see langword="null"/>, in which case the service type must be registered in the DI container against the contract of the service.
        /// </param>
        /// <param name="baseAddresses">
        /// The <see cref="Array"/> of type <see cref="Uri"/> that contains the base addresses for the service hosted.
        /// </param>
        /// <returns>
        /// A <see cref="ServiceHost"/> for the type of service specified with a specific base address.
        /// </returns>
        ServiceHost CreateHost(
            Type serviceType,
            params Uri[] baseAddresses);

        /// <summary>
        /// Generic method which creates a service host outside of WAS where the created service is specified by the type parameter of the generic.
        /// Can be used when the service is created in a self-hosting environment or for testing purposes.
        /// </summary>
        /// <typeparam name="TService">Specifies the type of service to be hosted.</typeparam>
        /// <param name="baseAddresses">The <see cref="Array" /> of type <see cref="Uri" /> that contains the base addresses for the service hosted.</param>
        /// <returns>A <see cref="ServiceHost" /> for the type of service specified with a specific base address.</returns>
        ServiceHost CreateHost<TService>(
            params Uri[] baseAddresses);

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

    #region ICreateServiceHost contract binding
    [ContractClassFor(typeof(ICreateServiceHost))]
    abstract class ICreateServiceHostContract : ICreateServiceHost
    {
        public ServiceHost CreateHost(params Uri[] baseAddresses)
        {
            Contract.Requires<ArgumentNullException>(baseAddresses != null, nameof(baseAddresses));
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

            throw new NotImplementedException();
        }

        public ServiceHost CreateHost(Type serviceType, params Uri[] baseAddresses)
        {
            Contract.Requires<ArgumentNullException>(serviceType != null, nameof(serviceType));
            Contract.Requires<ArgumentNullException>(baseAddresses != null, nameof(baseAddresses));
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

            throw new NotImplementedException();
        }

        public ServiceHost CreateHost<TService>(params Uri[] baseAddresses)
        {
            Contract.Requires<ArgumentNullException>(baseAddresses != null, nameof(baseAddresses));
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

            throw new NotImplementedException();
        }

        public Task<bool> InitializeHostTask
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
    #endregion

}
