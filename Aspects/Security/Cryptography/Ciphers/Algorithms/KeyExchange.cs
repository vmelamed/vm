
namespace vm.Aspects.Security.Cryptography.Ciphers.Algorithms
{
    /// <summary>
    /// Contains constant strings identifying various key exchange algorithms (currently not used by the ciphers).
    /// </summary>
    public static class KeyExchange
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
        /// ECDiffieHellman implemented by ECDiffieHellmanCng. Recommended.
        /// </summary>
        public const string ECDiffieHellman = "ECDiffieHellmanCng";
    }
}
