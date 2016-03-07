using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace vm.Aspects.Wcf.EnableCorsService
{
    /// <summary>
    /// Class EnableCorsServiceHostFactory.
    /// </summary>
    /// <seealso cref="System.ServiceModel.Activation.ServiceHostFactory" />
    /// <remarks>
    /// Based on: https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
    /// </remarks>
    public class EnableCorsServiceHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// Creates the service host.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        /// <returns>ServiceHost.</returns>
        protected override ServiceHost CreateServiceHost(
            Type serviceType,
            Uri[] baseAddresses) => new EnableCorsServiceHost(serviceType, baseAddresses);
    }
}
