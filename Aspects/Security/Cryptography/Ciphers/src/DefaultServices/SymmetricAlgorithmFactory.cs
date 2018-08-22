using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers.DefaultServices
{
    /// <summary>
    /// DefaultSymmetricAlgorithmFactory encapsulates the default strategy for creating of a symmetric encryption algorithm object.
    /// </summary>
    public sealed class SymmetricAlgorithmFactory : ISymmetricAlgorithmFactory
    {
        #region ISymmetricAlgorithmFactory Members
        /// <summary>
        /// Initializes the factory with an optional symmetric algorithm name.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// If you do not call this method the factory will create instances implementing the <see cref="Algorithms.Symmetric.Default"/> (AESManaged) algorithm.
        /// Name of the symmetric algorithm. Hint: use the constants in the <see cref="Algorithms.Symmetric"/> static class.
        /// </param>
        public ISymmetricAlgorithmFactory Initialize(
            string symmetricAlgorithmName = Algorithms.Symmetric.Default)
        {
            SymmetricAlgorithmName = !symmetricAlgorithmName.IsNullOrWhiteSpace()
                                        ? symmetricAlgorithmName
                                        : Algorithms.Symmetric.Default;

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="SymmetricAlgorithm" /> instance.
        /// </summary>
        /// <returns><see cref="SymmetricAlgorithm" /> instance.</returns>
        public SymmetricAlgorithm Create()
            => SymmetricAlgorithm.Create(SymmetricAlgorithmName);

        /// <summary>
        /// Gets the name of the symmetric algorithm.
        /// </summary>
        /// <value>The name of the symmetric algorithm.</value>
        /// <remarks>Initial value <see cref="Algorithms.Symmetric.Default"/> (AESManaged).</remarks>
        public string SymmetricAlgorithmName { get; private set; } = Algorithms.Symmetric.Default;

        #endregion
    }
}
