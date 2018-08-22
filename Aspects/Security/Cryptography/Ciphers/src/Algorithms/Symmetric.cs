
using System;

namespace vm.Aspects.Security.Cryptography.Ciphers.Algorithms
{
    /// <summary>
    /// Contains constant strings identifying the various symmetric encryption algorithms used by the ciphers.
    /// </summary>
    public static class Symmetric
    {
        /// <summary>
        /// The default and preferred symmetric algorithm implementation is AES.
        /// </summary>
        public const string Default = AesManaged;

        /// <summary>
        /// AES implemented by AesCryptoServiceProvider maybe faster than AES Managed. Available on older platforms and not developed anymore. Recommended.
        /// </summary>
        public const string Aes = "AESCryptoServiceProvider";

        /// <summary>
        /// AES implemented by AesManaged may be faster than AesCryptoServiceProvider. Available on all platforms. Preferred.
        /// </summary>
        public const string AesManaged = "AESManaged";

        /// <summary>
        /// The Rijndael implemented by RijndaelManaged.
        /// </summary>
        public const string Rijndael = "Rijndael";

        /// <summary>
        /// The DES implemented by DESCryptoServiceProvider. Not recommended, use for backwards compatibility only.
        /// </summary>
        [Obsolete("DES is not a recommended hash algorithm. It should be used only for backwards compatibility.")]
        public const string Des = "DES";

        /// <summary>
        /// The Triple DES implemented by TripleDESCryptoServiceProvider. Not recommended, use for backwards compatibility only.
        /// </summary>
        public const string TripleDes = "TripleDES";

        /// <summary>
        /// The RC2 implemented by RC2CryptoServiceProvider. Not recommended, use for backwards compatibility only.
        /// </summary>
        [Obsolete("RC2 is not a recommended hash algorithm. It should be used only for backwards compatibility.")]
        public const string RC2 = "RC2";
    }
}
