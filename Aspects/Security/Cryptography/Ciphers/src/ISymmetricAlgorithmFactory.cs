using System;
using System.Security.Cryptography;

using vm.Aspects.Security.Cryptography.Ciphers.Algorithms;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Interface <c>ISymmetricAlgorithmFactory</c> defines the behavior of an object factory which creates 
    /// the underlying <see cref="Symmetric"/> objects. The factory must implement a strategy for picking the
    /// symmetric algorithm given choices like, parameters, default values, etc.
    /// </summary>
    public interface ISymmetricAlgorithmFactory
    {
        /// <summary>
        /// Initializes the factory with an optional symmetric algorithm name.
        /// Possibly implements the resolution strategy and initializes the factory with the appropriate values.
        /// </summary>
        /// <param name="symmetricAlgorithmName">Name of the symmetric algorithm.</param>
        /// <returns>The initialized instance implementing <see cref="ISymmetricAlgorithmFactory"/>.</returns>
        ISymmetricAlgorithmFactory Initialize(string symmetricAlgorithmName = null);

        /// <summary>
        /// Creates a <see cref="Symmetric"/> instance.
        /// </summary>
        /// <returns><see cref="Symmetric"/> instance.</returns>
        /// <exception cref="Exception">
        /// If the factory could not resolve the symmetric algorithm.
        /// </exception>
        SymmetricAlgorithm Create();

        /// <summary>
        /// Gets the name of the symmetric algorithm.
        /// </summary>
        /// <value>The name of the symmetric algorithm.</value>
        string SymmetricAlgorithmName { get; }
    }
}
