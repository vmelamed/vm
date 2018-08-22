using System;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers.DefaultServices
{
    /// <summary>
    /// Class <c>KeyedHashAlgorithmFactory</c> encapsulates the strategy for determining and realizing the keyed hash algorithm.
    /// </summary>
    public sealed class KeyedHashAlgorithmFactory : IHashAlgorithmFactory
    {
        /// <summary>
        /// The generated keyed hash factory
        /// </summary>
        readonly Func<KeyedHashAlgorithm> _hashFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedHashAlgorithmFactory"/> class.
        /// </summary>
        public KeyedHashAlgorithmFactory()
            => _hashFactory = () => KeyedHashAlgorithm.Create(HashAlgorithmName);

        #region IHashAlgorithmFactory members
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedHashAlgorithmFactory"/> class with optional keyed hash algorithm name.
        /// </summary>
        /// <param name="hashAlgorithmName">Name of the keyed hash algorithm.</param>
        /// <exception cref="Exception">
        /// If the supplied keyed hash algorithm name is not valid.
        /// </exception>
        /// <remarks>
        /// Implements the strategy for resolving the keyed hash algorithm:
        /// <list type="number">
        ///     <item>
        ///         If the user passed keyed hash algorithm name that is not <see langword="null"/>, empty or whitespace characters only, 
        ///         it is used in creating the <see cref="KeyedHashAlgorithm"/> object; otherwise
        ///     </item>
        ///     <item>
        ///         Assumes the default keyed hash algorithm - <see cref="Algorithms.KeyedHash.Default"/> - HMACSHA256.
        ///     </item>
        /// </list>
        /// </remarks>
        public IHashAlgorithmFactory Initialize(
            string hashAlgorithmName = null)
        {
            // 1. If the user passed keyed hash algorithm name that is not null, empty or whitespace characters only, 
            //    it will be used in creating the <see cref="KeyedHashAlgorithm"/> object.
            if (!hashAlgorithmName.IsNullOrWhiteSpace())
                HashAlgorithmName = hashAlgorithmName;

            // try it
            using (var hashAlgorithm = _hashFactory())
                if (hashAlgorithm == null)
                    // if unsuccessful - throw an exception.
                    throw new ArgumentException(
                                $"\"{HashAlgorithmName}\" was not recognized as a valid keyed hash algorithm.", nameof(hashAlgorithmName));

            return this;
        }

        /// <summary>
        /// Creates a <see cref="KeyedHashAlgorithm"/> instance.
        /// </summary>
        /// <returns><see cref="HashAlgorithm"/> instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the factory could not resolve the keyed hash algorithm.
        /// </exception>
        public HashAlgorithm Create()
        {
            if (_hashFactory == null)
                throw new InvalidOperationException("The factory was not initialized properly. Call Initialize first.");

            return _hashFactory();
        }

        /// <summary>
        /// Gets the name of the keyed hash algorithm.
        /// </summary>
        /// <value>The name of the keyed hash algorithm.</value>
        public string HashAlgorithmName { get; private set; } = Algorithms.KeyedHash.Default;
        #endregion
    }
}
