using System;
using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Security.Cryptography.Ciphers.Algorithms
{
    /// <summary>
    /// Contains constant strings identifying various hashing algorithms used by the ciphers.
    /// </summary>
    public static class Hash
    {
        /// <summary>
        /// The user can register a certificate instance in the Common Service Locator for encrypting the hash (signature) with a resolve name - &quot;SigningCertificate&quot;.
        /// </summary>
        public const string CertificateResolveName = "SigningCertificate";

        /// <summary>
        /// The user can register hash algorithm name string instance in the Common Service Locator with a resolve name - &quot;DefaultHash&quot;.
        /// </summary>
        public const string ResolveName = "DefaultHash";

        /// <summary>
        /// The default and preferred hash algorithm implementation is SHA256.
        /// </summary>
        public const string Default = Sha256;

        /// <summary>
        /// SHA1. Not recommended. Prefer SHA256, and higher. Use for backwards compatibility only.
        /// </summary>
        [Obsolete("SHA-1 is not a recommended hash algorithm. It should be used only for backwards compatibility.")]
        public const string Sha1 = "SHA1";
        /// <summary>
        /// SHA256.
        /// </summary>
        public const string Sha256 = "SHA256";
        /// <summary>
        /// SHA384.
        /// </summary>
        public const string Sha384 = "SHA384";
        /// <summary>
        /// SHA512.
        /// </summary>
        public const string Sha512 = "SHA512";

        /// <summary>
        /// MD5. Not recommended, use for backwards compatibility only.
        /// </summary>
        [Obsolete("MD5 is not a recommended hash algorithm. It should be used only for backwards compatibility.")]
        public const string MD5 = "MD5";
        /// <summary>
        /// RIPEMD160 implemented by RIPEMD160Managed. Not recommended, use for backwards compatibility only.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Ripemd")]
        [Obsolete("RIPEMD5 is not a recommended hash algorithm. It should be used only for backwards compatibility.")]
        public const string Ripemd160 = "RIPEMD160";
    }
}
