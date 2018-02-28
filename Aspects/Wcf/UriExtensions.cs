using System;
using System.Net;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Adds extension method(s) to <see cref="Uri"/>.
    /// </summary>
    public static class UriExtensions
    {
        const string _loopback  = "127.0.0.1";
        const string _localhost = "localhost";

        /// <summary>
        /// The DNS name of this host
        /// </summary>
        public static readonly string ThisHost = Dns.GetHostEntry(_localhost).HostName;

        /// <summary>
        /// Determines whether the specified URI is hosted on the local machine.
        /// </summary>
        /// <param name="resource">The address.</param>
        /// <returns><see langword="true"/> if the specified URI is on the local machine; otherwise, <see langword="false"/>.</returns>
        public static bool IsOnThisHost(
            this Uri resource)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));

            return resource.Host.Equals(ThisHost, StringComparison.OrdinalIgnoreCase)   ||
                   resource.Host.Equals(_loopback, StringComparison.OrdinalIgnoreCase)  ||
                   resource.Host.Equals(_localhost, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the <paramref name="resource"/> is on same host as <paramref name="other"/>.
        /// </summary>
        /// <param name="resource">The address.</param>
        /// <param name="other">The other.</param>
        /// <returns><see langword="true" /> if [is on same host as] [the specified other]; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// address
        /// or
        /// other
        /// </exception>
        public static bool IsOnSameHostAs(
            this Uri resource,
            Uri other)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return resource.Host.Equals(other.Host, StringComparison.OrdinalIgnoreCase) || (resource.IsOnThisHost() && other.IsOnThisHost());
        }
    }
}
