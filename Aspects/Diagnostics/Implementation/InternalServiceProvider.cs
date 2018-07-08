using System;

namespace vm.Aspects.Diagnostics.Implementation
{
    /// <summary>
    /// Provides an internal and pretty trivial implementation of <see cref="IServiceProvider"/>
    /// </summary>
    /// <seealso cref="IServiceProvider" />
    class InternalServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType" />.-or-
        /// <see langword="null" /> if there is no service object of type <paramref name="serviceType" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return serviceType == typeof(IMemberInfoComparer) ? new MemberDumpOrder() : null;
        }
    }
}
