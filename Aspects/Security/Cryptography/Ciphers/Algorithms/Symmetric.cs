
namespace vm.Aspects.Security.Cryptography.Ciphers.Algorithms
{
    /// <summary>
    /// Contains constant strings identifying the various symmetric encryption algorithms used by the ciphers.
    /// </summary>
    public static class Symmetric
    {
        /// <summary>
        /// The user can register a certificate instance in the Common Service Locator for encrypting the symmetric key with a resolve name - &quot;EncryptingCertificate&quot;.
        /// </summary>
        public const string CertificateResolveName = "EncryptingCertificate";

        /// <summary>
        /// The user can register symmetric algorithm name string instance in the Common Service Locator with a resolve name - &quot;DefaultSymmetricAlgorithm&quot;.
        /// </summary>
        public const string ResolveName = "DefaultSymmetricAlgorithm";

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
        public const string Des = "DES";

        /// <summary>
        /// The Triple DES implemented by TripleDESCryptoServiceProvider. Not recommended, use for backwards compatibility only.
        /// </summary>
        public const string TripleDes = "TripleDES";

        /// <summary>
        /// The RC2 implemented by RC2CryptoServiceProvider. Not recommended, use for backwards compatibility only.
        /// </summary>
        public const string RC2 = "RC2";
    }
}
