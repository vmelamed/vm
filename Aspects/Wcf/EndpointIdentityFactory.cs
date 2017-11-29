using System;
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
        /// Validates that the parameters are valid for name-based (DNS, SPN, UPN or RSA) endpoint identities.
        /// </summary>
        /// <param name="identityType">Type of the identity.</param>
        /// <param name="identityName">Identity name (DNS, SPN or UPN) or the public RSA key.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The identity type is not <see cref="ServiceIdentity.None"/> and 
        /// <paramref name="identityName"/> is <see langword="null"/>, empty or consists of whatespace characters only.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The identity type is not <see cref="ServiceIdentity.None"/> and
        /// is not name based identity type.
        /// </exception>
        public static void ValidateIdentityParameters(
            ServiceIdentity identityType,
            string identityName)
        {
            if (identityType == ServiceIdentity.None && identityName.IsNullOrWhiteSpace())
                return;

            // the identity name should be not null, empty or blank.
            if (identityName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identityName));

            // the identity type must be name based
            if (identityType != ServiceIdentity.Dns &&
                identityType != ServiceIdentity.Spn &&
                identityType != ServiceIdentity.Upn &&
                identityType != ServiceIdentity.Rsa)
                throw new ArgumentException("Invalid identity type.", nameof(identityType));
        }

        /// <summary>
        /// Validates that the parameters are valid for certificate based endpoint identities.
        /// </summary>
        /// <param name="identityType">Type of the identity.</param>
        /// <param name="identifyingCertificate">The identifying certificate.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The identity type is not <see cref="ServiceIdentity.None"/> and the certificate is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The identity type is not <see cref="ServiceIdentity.None"/> and is not certificate based identity type.
        /// </exception>
        public static void ValidateIdentityParameters(
            ServiceIdentity identityType,
            X509Certificate2 identifyingCertificate)
        {
            if (identityType == ServiceIdentity.None && identifyingCertificate == null)
                return;

            if (identifyingCertificate == null)
                throw new ArgumentNullException(nameof(identifyingCertificate));

            // the identity type must be certificate based
            if (identityType != ServiceIdentity.Rsa &&
                identityType != ServiceIdentity.Dns &&
                identityType != ServiceIdentity.Certificate)
                throw new ArgumentException("Invalid identity type.", nameof(identityType));
        }

        /// <summary>
        /// Validates that the parameters are valid for claim identity.
        /// </summary>
        /// <param name="identityType">
        /// Type of the identity (must be <see cref="ServiceIdentity.Claim"/> or <see cref="ServiceIdentity.None"/>).
        /// </param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The identity claim is <see langword="null"/>
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The identity type is not <see cref="ServiceIdentity.None"/> and is not claim based identity type.
        /// </exception>
        public static void ValidateIdentityParameters(
            ServiceIdentity identityType,
            Claim identityClaim)
        {
            if (identityType == ServiceIdentity.None && identityClaim == null)
                return;

            if (identityClaim == null)
                throw new ArgumentNullException(nameof(identityClaim));

            if (identityType != ServiceIdentity.Claim)
                throw new ArgumentException("Invalid identity type.", nameof(identityType));
        }

        /// <summary>
        /// Creates an endpoint identity.
        /// </summary>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or 
        /// <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identityName">The identity: the DNS name, SPN, UPN or the RSA key.</param>
        /// <returns>EndpointIdentity instance.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the identity type is not supported.
        /// </exception>
        public static EndpointIdentity CreateEndpointIdentity(
            ServiceIdentity identityType,
            string identityName = null)
        {
            ValidateIdentityParameters(identityType, identityName);

            switch (identityType)
            {
                case ServiceIdentity.None:
                    return null;

                case ServiceIdentity.Dns:
                    return EndpointIdentity.CreateDnsIdentity(identityName);

                case ServiceIdentity.Upn:
                    return EndpointIdentity.CreateUpnIdentity(identityName);

                case ServiceIdentity.Spn:
                    return EndpointIdentity.CreateSpnIdentity(identityName);

                case ServiceIdentity.Rsa:
                    return EndpointIdentity.CreateRsaIdentity(identityName);

                default:
                    throw new NotSupportedException($"Identity type {identityType} is not supported.");
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
            ValidateIdentityParameters(identityType, identifyingCertificate);

            switch (identityType)
            {
                case ServiceIdentity.None:
                    return null;

                case ServiceIdentity.Certificate:
                    return EndpointIdentity.CreateX509CertificateIdentity(identifyingCertificate);

                case ServiceIdentity.Rsa:
                    return EndpointIdentity.CreateRsaIdentity(identifyingCertificate);

                case ServiceIdentity.Dns:
                    return EndpointIdentity.CreateDnsIdentity(
                                                RegularExpression.CommonName.Replace(
                                                                    identifyingCertificate.SubjectName.Name.Split(',')[0],
                                                                    string.Empty));

                default:
                    throw new NotSupportedException($"Identity type {identityType} is not supported.");
            }
        }

        /// <summary>
        /// Creates an endpoint identity based on claim.
        /// </summary>
        /// <param name="identityClaim">The identity.</param>
        /// <returns>EndpointIdentity.</returns>
        public static EndpointIdentity CreateEndpointIdentity(
            Claim identityClaim) => identityClaim != null ? EndpointIdentity.CreateIdentity(identityClaim) : null;

        /// <summary>
        /// Creates an endpoint identity based on claim.
        /// </summary>
        /// <param name="identityType">Type of the identity.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>EndpointIdentity.</returns>
        public static EndpointIdentity CreateEndpointIdentity(
            ServiceIdentity identityType,
            Claim identityClaim)
        {
            ValidateIdentityParameters(identityType, identityClaim);

            return identityType == ServiceIdentity.Claim ? EndpointIdentity.CreateIdentity(identityClaim) : null;
        }
    }
}
