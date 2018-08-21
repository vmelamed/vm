using System;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers.DefaultServices
{
    /// <summary>
    /// Class <c>HashAlgorithmFactory</c> encapsulates the strategy for determining and realizing the hash algorithm.
    /// </summary>
    public sealed class HashAlgorithmFactory : IHashAlgorithmFactory
    {
        #region IHashAlgorithmFactory members
        /// <summary>
        /// Gets the name of the hash algorithm.
        /// </summary>
        /// <value>The hash algorithm.</value>
        public string HashAlgorithmName { get; private set; } = Algorithms.Hash.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashAlgorithmFactory"/> class with optional hash algorithm name.
        /// </summary>
        /// <param name="hashAlgorithmName">Name of the hash algorithm.</param>
        /// <remarks>
        /// Implements the strategy for resolving the hash algorithm:
        /// <list type="number">
        ///     <item>
        ///         If the user passed hash algorithm name that is not <see langword="null"/>, empty or whitespace characters only, 
        ///         it is used in creating the <see cref="HashAlgorithm"/> object; otherwise
        ///     </item>
        ///     <item>
        ///         Assume the default hash algorithm - <see cref="Algorithms.Hash.Default"/> - SHA256.
        ///     </item>
        /// </list>
        /// </remarks>
        public IHashAlgorithmFactory Initialize(
            string hashAlgorithmName = null)
        {
            // 1. If the user passed hash algorithm name that is not null, empty or whitespace characters only, 
            //    it will be used in creating the <see cref="HashAlgorithm"/> object.
            if (!hashAlgorithmName.IsNullOrWhiteSpace())
                HashAlgorithmName = hashAlgorithmName;

            // try it
            using (var hashAlgorithm = Create())
                if (hashAlgorithm == null)
                    // if unsuccessful - throw an exception.
                    throw new ArgumentException(
                                $"\"{HashAlgorithmName}\" was not recognized as a valid hash algorithm.", nameof(hashAlgorithmName));

            return this;
        }

        /// <summary>
        /// Creates a <see cref="HashAlgorithm"/> instance.
        /// </summary>
        /// <returns><see cref="HashAlgorithm"/> instance.</returns>
        public HashAlgorithm Create()
            => HashAlgorithm.Create(HashAlgorithmName);
        #endregion
    }
}
