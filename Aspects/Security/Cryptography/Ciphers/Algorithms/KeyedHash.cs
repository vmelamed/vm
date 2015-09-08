using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Security.Cryptography.Ciphers.Algorithms
{
    /// <summary>
    /// Contains constant strings identifying various keyed hashing algorithms used by the ciphers.
    /// </summary>
    public static class KeyedHash
    {
        /// <summary>
        /// The default and preferred hash algorithm implementation is HMACSHA256.
        /// </summary>
        public const string Default = HmacSha256;

        /// <summary>
        /// HMACSHA1. Prefer HMACSHA256, and higher.
        /// </summary>
        public const string HmacSha1 = "HMACSHA1";

        /// <summary>
        /// HMACSHA256. Recommended.
        /// </summary>
        public const string HmacSha256 = "HMACSHA256";

        /// <summary>
        /// HMACSHA384.
        /// </summary>
        public const string HmacSha384 = "HMACSHA384";

        /// <summary>
        /// HMACSHA512. Recommended.
        /// </summary>
        public const string HmacSha512 = "HMACSHA512";

        /// <summary>
        /// HMACMD5. Not recommended, use for backwards compatibility only.
        /// </summary>
        public const string HmacMD5 = "HMACMD5";

        /// <summary>
        /// HMACRIPEMD160 implemented by RIPEMD160Managed. Not recommended, use for backwards compatibility only.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Ripemd")]
        public const string HmacRipemd160 = "HMACRIPEMD160";

        /// <summary>
        /// MACTripleDES. Not recommended, use for backwards compatibility only.
        /// </summary>
        public const string MacTripleDes = "MACTripleDES";
    }
}
