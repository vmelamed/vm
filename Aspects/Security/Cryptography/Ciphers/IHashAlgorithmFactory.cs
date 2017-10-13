using System.Security.Cryptography;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Interface <c>IHashAlgorithmFactory</c> defines the behavior of an object factory which creates 
    /// the underlying <see cref="HashAlgorithm"/> objects. The factory must implement a strategy for picking the
    /// hash algorithm given choices like, parameters, Common Service Locator registrations, default values, etc.
    /// </summary>
    public interface IHashAlgorithmFactory
    {
        /// <summary>
        /// Initializes the factory with an optional hash algorithm name.
        /// Possibly implements the resolution strategy and initializes the factory with the appropriate values.
        /// </summary>
        void Initialize(string hashAlgorithmName);

        /// <summary>
        /// Creates a <see cref="HashAlgorithm"/> instance.
        /// </summary>
        /// <returns><see cref="HashAlgorithm"/> instance.</returns>
        /// <exception cref="ActivationException">
        /// If the factory could not resolve the hash algorithm.
        /// </exception>
        HashAlgorithm Create();

        /// <summary>
        /// Gets the name of the hash algorithm.
        /// </summary>
        /// <value>The name of the hash algorithm.</value>
        string HashAlgorithmName { get; }
    }
}
