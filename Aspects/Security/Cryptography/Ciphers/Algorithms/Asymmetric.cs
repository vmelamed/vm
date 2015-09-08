
namespace vm.Aspects.Security.Cryptography.Ciphers.Algorithms
{
    /// <summary>
    /// Contains constant strings identifying the various asymmetric encryption algorithms used by the ciphers.
    /// </summary>
    public static class Asymmetric
    {
        /// <summary>
        /// The default and preferred asymmetric algorithm implementation is RSA.
        /// </summary>
        public const string Default = Rsa;

        /// <summary>
        /// RSA implemented by RSACryptoServiceProvider. Recommended.
        /// </summary>
        public const string Rsa = "RSA";
    }
}
