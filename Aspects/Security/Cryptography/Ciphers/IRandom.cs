namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Represents cryptographically strong random number generators behavior.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Fills an array of bytes with a cryptographically strong sequence of random values.
        /// </summary>
        /// <param name="data">The array to fills with a cryptographically strong sequence of random bytes.</param>
        void GetBytes(byte[] data);
    }
}
