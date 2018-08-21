using System;
using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Security.Cryptography.Ciphers.Algorithms
{
    /// <summary>
    /// Contains constant strings identifying the various signing algorithms used by the ciphers.
    /// </summary>
    public static class Signature
    {
        /// <summary>
        /// The default and preferred signature algorithm implementation is RSA.
        /// </summary>
        public const string Default = Rsa;

        /// <summary>
        /// RSA implemented by RSACryptoServiceProvider. Recommended.
        /// </summary>
        public const string Rsa = "RSA";

        /// <summary>
        /// DSA implemented by DSACryptoServiceProvider. Not recommended, use for backwards compatibility only.
        /// </summary>
        [Obsolete(("DSA is not a recommended hash algorithm."))]
        public const string Dsa = "DSA";

        /// <summary>
        /// ECDsa implemented by ECDsaCng. Recommended.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Ecd")]
        public const string ECDsa = "ECDsaCng";
    }
}
