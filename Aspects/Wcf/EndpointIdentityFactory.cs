using System;
using System.Diagnostics.Contracts;
using System.IdentityModel.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Class EndpointIdentityFactory creates an endpoint identity depending on the ServiceIdentity enum.
    /// </summary>
    public static class EndpointIdentityFactory
    {
        /// <summary>
        /// Creates an endpoint identity.
        /// </summary>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or 
        /// <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identity">The identity: the DNS name, SPN, UPN or the RSA key.</param>
        /// <returns>EndpointIdentity instance.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the identity type is not supported.
        /// </exception>
        public static EndpointIdentity CreateEndpointIdentity(
            ServiceIdentity identityType,
            string identity = null)
        {
            Contract.Requires<ArgumentException>(identityType != ServiceIdentity.Certificate, "Invalid combination of identity parameters.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None || !identity.IsNullOrWhiteSpace(), "Invalid combination of identity parameters.");

            switch (identityType)
            {
            case ServiceIdentity.Dns:
                return EndpointIdentity.CreateDnsIdentity(identity);

            case ServiceIdentity.Upn:
                return EndpointIdentity.CreateUpnIdentity(identity);

            case ServiceIdentity.Spn:
                return EndpointIdentity.CreateSpnIdentity(identity);

            case ServiceIdentity.Rsa:
                return EndpointIdentity.CreateRsaIdentity(identity);

            default:
                return null;
            }

            throw new NotSupportedException($"Identity type {identityType} is not supported.");
        }

        /// <summary>
        /// Creates an endpoint identity.
        /// </summary>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Certificate" /> or <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identifyingCertificate">The identifying certificate.</param>
        /// <returns>EndpointIdentity instance.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the identity type is not supported.
        /// </exception>
        public static EndpointIdentity CreateEndpointIdentity(
            ServiceIdentity identityType,
            X509Certificate2 identifyingCertificate)
        {
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None  ||  (identityType == ServiceIdentity.Dns  ||
                                                                                            identityType == ServiceIdentity.Rsa  ||
                                                                                            identityType == ServiceIdentity.Certificate) && identifyingCertificate!=null, "Invalid combination of identity parameters.");

            switch (identityType)
            {
            case ServiceIdentity.Certificate:
                return EndpointIdentity.CreateX509CertificateIdentity(identifyingCertificate);

            case ServiceIdentity.Rsa:
                return EndpointIdentity.CreateRsaIdentity(identifyingCertificate);

            case ServiceIdentity.Dns:
                return EndpointIdentity.CreateDnsIdentity(identifyingCertificate.SubjectName.Name.Split(',')[0].Replace("CN=", ""));

            default:
                return null;
            }

            throw new NotSupportedException($"Identity type {identityType} is not supported.");
        }

        /// <summary>
        /// Creates an endpoint identity based on claim.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns>EndpointIdentity.</returns>
        public static EndpointIdentity CreateEndpointIdentity(
            Claim identity) => identity!=null
                                    ? EndpointIdentity.CreateIdentity(identity)
                                    : null;
    }
}
