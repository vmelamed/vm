using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Diagnostics.DumpImplementation;

namespace vm.Aspects.Diagnostics
{
    class ServiceResolver : ServiceLocatorImplBase
    {
        static Lazy<IServiceLocator> _serviceLocator = new Lazy<IServiceLocator>(() => new ServiceResolver(), true);

        internal static IServiceLocator Default => _serviceLocator.Value;

        /// <summary>
        /// Does the actual work of resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>Sequence of service instance objects.</returns>
        /// <exception cref="Microsoft.Practices.ServiceLocation.ActivationException">Thrown if the service type is not supported.</exception>
        protected override IEnumerable<object> DoGetAllInstances(
            Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            var list = new List<object>();

            if (serviceType == typeof(IMemberInfoComparer))
                list.Add(new MemberDumpOrder());

            return list;
        }

        /// <summary>
        /// Does the actual work of resolving the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <returns>The requested service instance.</returns>
        /// <exception cref="Microsoft.Practices.ServiceLocation.ActivationException">
        /// Thrown if the service type is not supported.
        /// </exception>
        protected override object DoGetInstance(
            Type serviceType,
            string key)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (serviceType != typeof(IMemberInfoComparer))
                throw new ActivationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Service type {0} is not registered in the internal service locator.",
                                serviceType.FullName));

            if (!string.IsNullOrWhiteSpace(key))
                throw new ActivationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Named instance '{0}' of the service {1} is not registered in the internal service locator.",
                                key,
                                serviceType.FullName));

            return new MemberDumpOrder();
        }
    }
}
