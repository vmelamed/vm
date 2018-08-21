using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Interface <c>IHashAlgorithmFactory</c> defines the behavior of an object factory which creates 
    /// the underlying <see cref="HashAlgorithm"/> objects. The factory must implement a strategy for picking the
    /// hash algorithm given choices like, parameters, default values, etc.
    /// </summary>
    public interface IHashAlgorithmFactory
    {
        /// <summary>
        /// Initializes the factory with an optional hash algorithm name.
        /// Possibly implements the resolution strategy and initializes the factory with the appropriate values.
        /// </summary>
        /// <param name="hashAlgorithmName">Name of the hash algorithm.</param>
        /// <returns>IHashAlgorithmFactory - usually the current instance. Appropriate for method chaining.</returns>
        IHashAlgorithmFactory Initialize(string hashAlgorithmName);

        /// <summary>
        /// Creates a <see cref="HashAlgorithm"/> instance.
        /// </summary>
        /// <returns><see cref="HashAlgorithm"/> instance.</returns>
        HashAlgorithm Create();

        /// <summary>
        /// Gets the name of the hash algorithm.
        /// </summary>
        /// <value>The name of the hash algorithm.</value>
        string HashAlgorithmName { get; }
    }
}
