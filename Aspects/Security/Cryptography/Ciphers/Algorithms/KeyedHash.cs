using System;

namespace vm.Aspects.Security.Cryptography.Ciphers.Algorithms
{
    /// <summary>
    /// Contains constant strings identifying various keyed hashing algorithms used by the ciphers.
    /// </summary>
    public static class KeyedHash
    {
        /// <summary>
        /// The user can register a certificate instance in the Common Service Locator for encrypting the symmetric key with a resolve name - &quot;EncryptingHashKeyCertificate&quot;.
        /// </summary>
        public const string CertificateResolveName = "EncryptingHashKeyCertificate";

        /// <summary>
        /// The user can register hash algorithm name string instance in the Common Service Locator with a resolve name - &quot;DefaultHash&quot;.
        /// </summary>
        public const string ResolveName = "DefaultKeyedHash";

        /// <summary>
        /// The default and preferred hash algorithm implementation - &quot;HMACSHA256&quot;.
        /// </summary>
        public const string Default = HmacSha256;

        /// <summary>
        /// The MAC-TripleDES. Recommended.
        /// </summary>
        public const string MacTripleDes = "MACTripleDES";

        /// <summary>
        /// The HMAC-SHA256. Recommended.
        /// </summary>
        public const string HmacSha256 = "HMACSHA256";

        /// <summary>
        /// The HMAC-SHA384. Recommended.
        /// </summary>
        public const string HmacSha384 = "HMACSHA384";

        /// <summary>
        /// The HMAC-SHA512. Recommended.
        /// </summary>
        public const string HmacSha512 = "HMACSHA512";

        /// <summary>
        /// The HMAC-SHA1. Not recommended, use for backwards compatibility only.
        /// </summary>
        [Obsolete("HMAC-SHA-1 is not a recommended hash algorithm. It should be used only for backwards compatibility.")]
        public const string HmacSha1 = "HMACSHA1";
        /// <summary>
        /// The HMAC-RIPEMD160. Not recommended, use for backwards compatibility only.
        /// </summary>
        [Obsolete("HMAC-RIPEMD160 is not a recommended hash algorithm. It should be used only for backwards compatibility.")]
        public const string HmacRipemd160 = "HMACRIPEMD160";
    }
}
