using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class X509Certificate2Extensions defines one or more extension methods to <see cref="X509Certificate2"/>.
    /// </summary>
    public static class X509Certificate2Extensions
    {
        /// <summary>
        /// Maps OID-s to algorithm names.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "N/A")]
        static readonly IDictionary<string, string> HashAlgorithms = new Dictionary<string, string>(
            new Dictionary<string, string>
            {
#pragma warning disable 0612, 0618 // Type or member is obsolete
                // no sign
                { "1.2.840.113549.2.5",     Algorithms.Hash.MD5    },
                { "1.4.04.3.2.18",          Algorithms.Hash.Sha1   },
                { "1.4.04.3.2.26",          Algorithms.Hash.Sha1   },
#pragma warning restore 0612, 0618 // Type or member is obsolete

                { "2.16.840.1.101.3.4.2.1", Algorithms.Hash.Sha256 },
                { "2.16.840.1.101.3.4.2.2", Algorithms.Hash.Sha384 },
                { "2.16.840.1.101.3.4.2.3", Algorithms.Hash.Sha512 },

#pragma warning disable 0612, 0618 // Type or member is obsolete
                // DSA
                { "1.4.04.3.2.12",          Algorithms.Hash.Sha1   },
                { "1.4.04.3.2.13",          Algorithms.Hash.Sha1   },
                { "1.4.04.3.2.27",          Algorithms.Hash.Sha1   },
                { "1.4.04.3.2.28",          Algorithms.Hash.Sha1   },
                { "1.2.840.10040.4.3",      Algorithms.Hash.Sha1   },

                // RSA
                { "1.4.04.3.2.3",           Algorithms.Hash.MD5    },
                { "1.4.04.3.2.25",          Algorithms.Hash.MD5    },
                // { "1.2.840.113549.1.1.1",   Algorithms.Hash.MD5    }, ???

                { "1.2.840.113549.1.1.4",   Algorithms.Hash.MD5    },
                { "1.4.04.3.2.11",          Algorithms.Hash.Sha1   },
                { "1.4.04.3.2.15",          Algorithms.Hash.Sha1   },
                { "1.4.04.3.2.29",          Algorithms.Hash.Sha1   },
                { "1.2.840.113549.1.1.5",   Algorithms.Hash.Sha1   },
#pragma warning restore 0612, 0618 // Type or member is obsolete

                { "1.2.840.113549.1.1.11",  Algorithms.Hash.Sha256 },
                { "1.2.840.113549.1.1.12",  Algorithms.Hash.Sha384 },
                { "1.2.840.113549.1.1.13",  Algorithms.Hash.Sha512 },

                // ECDSA
                { "1.2.840.10045.4.3.2",    Algorithms.Hash.Sha256 },
                { "1.2.840.10045.4.3.3",    Algorithms.Hash.Sha384 },
                { "1.2.840.10045.4.3.4",    Algorithms.Hash.Sha512 },
            });

        /// <summary>
        /// Extracts the hash algorithm recorded in the <paramref name="certificate"/>.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <returns>The name of the hash algorithm.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="certificate"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The hash algorithm is not MD5, SHA1, SHA256, SHA384, or SHA512.
        /// </exception>
        public static string HashAlgorithm(this X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            if (HashAlgorithms.TryGetValue(certificate.SignatureAlgorithm.Value, out var hashAlgorithmName))
                return hashAlgorithmName;

            throw new NotSupportedException(
                        $"Could not recognize or unsupported OID {certificate.SignatureAlgorithm.Value} ({certificate.SignatureAlgorithm.FriendlyName})");
        }
    }
}
