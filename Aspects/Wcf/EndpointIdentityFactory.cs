using System;
using System.Diagnostics.Contracts;
using System.Globalization;
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
        /// Creates an endpoint identity either from string identifier or from a certificate.
        /// </summary>
        /// <param name="identityType">Type of the identity.</param>
        /// <param name="identity">The identity: the DNS name, SPN, UPN or the RSA key. If <see langword="null"/>, <paramref name="identifyingCertificate"/> must not be <see langword="null"/>.</param>
        /// <param name="identifyingCertificate">The identifying certificate. If <see langword="null"/>, <paramref name="identity"/> must not be <see langword="null"/>.</param>
        /// <returns>EndpointIdentity instance.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the identity type is not supported.
        /// </exception>
        public static EndpointIdentity CreateEndpointIdentity(
            ServiceIdentity identityType,
            string identity,
            X509Certificate2 identifyingCertificate)
        {
            Contract.Requires<ArgumentException>(
                !string.IsNullOrWhiteSpace(identity) || 
                identifyingCertificate!=null,
                "Both parameters - identity and identifyingCertificate - cannot be null at the same time.");

            if (string.IsNullOrWhiteSpace(identity))
                return CreateEndpointIdentity(identityType, identifyingCertificate);
            else
                return CreateEndpointIdentity(identityType, identity);
        }

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
            string identity)
        {
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                           ||
                identityType == ServiceIdentity.Dns  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Rsa  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Upn  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Spn  &&  !string.IsNullOrWhiteSpace(identity),
                "Invalid combination of identity parameters.");

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

            case ServiceIdentity.None:
                return null;

            default:
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, "Identity type {0} is not supported.", identityType));
            }
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
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                          ||
                identityType == ServiceIdentity.Certificate &&  identifyingCertificate!=null  ||
                identityType == ServiceIdentity.Dns         &&  identifyingCertificate!=null  ||
                identityType == ServiceIdentity.Rsa         &&  identifyingCertificate!=null,
                "Invalid combination of identity parameters.");

            switch (identityType)
            {
            case ServiceIdentity.Certificate:
                return EndpointIdentity.CreateX509CertificateIdentity(identifyingCertificate);

            case ServiceIdentity.Dns:
                return EndpointIdentity.CreateDnsIdentity(identifyingCertificate.SubjectName.Name.Split(',')[0]);

            case ServiceIdentity.Rsa:
                return EndpointIdentity.CreateRsaIdentity(identifyingCertificate);

            case ServiceIdentity.None:
                return null;

            default:
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, "Identity type {0} is not supported.", identityType));
            }
        }
    }
}
