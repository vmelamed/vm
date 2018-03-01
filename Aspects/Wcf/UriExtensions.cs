using System;
using System.Net;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Adds extension method(s) to <see cref="Uri"/>.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// The loopback address 127.0.0.1.
        /// </summary>
        public const string Loopback  = "127.0.0.1";

        /// <summary>
        /// The localhost
        /// </summary>
        public const string Localhost = "localhost";

        /// <summary>
        /// The localhost
        /// </summary>
        public const string LoopbackV6 = "::1";

        /// <summary>
        /// The DNS name of this host
        /// </summary>
        public static readonly string ThisHost = Dns.GetHostEntry(Localhost).HostName;

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
                   resource.Host.Equals(Loopback, StringComparison.OrdinalIgnoreCase)   ||
                   resource.Host.Equals(Localhost, StringComparison.OrdinalIgnoreCase)  ||
                   resource.Host.Equals(LoopbackV6, StringComparison.OrdinalIgnoreCase);
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
