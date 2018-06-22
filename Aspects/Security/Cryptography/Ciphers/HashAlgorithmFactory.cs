using CommonServiceLocator;
using System;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class <c>HashAlgorithmFactory</c> encapsulates the strategy for determining and realizing the hash algorithm.
    /// </summary>
    public sealed class HashAlgorithmFactory : IHashAlgorithmFactory
    {
        /// <summary>
        /// The resolved hash algorithm name
        /// </summary>
        string _hashAlgorithmName;
        /// <summary>
        /// The generated hash factory
        /// </summary>
        Func<HashAlgorithm> _hashFactory;

        #region IHashAlgorithmFactory members
        /// <summary>
        /// Initializes a new instance of the <see cref="HashAlgorithmFactory"/> class with optional hash algorithm name.
        /// </summary>
        /// <param name="hashAlgorithmName">Name of the hash algorithm.</param>
        /// <exception cref="ActivationException">
        /// If the supplied hash algorithm name is not valid.
        /// </exception>
        /// <remarks>
        /// Implements the strategy for resolving the hash algorithm:
        /// <list type="number">
        ///     <item>
        ///         If the user passed hash algorithm name that is not <see langword="null"/>, empty or whitespace characters only, 
        ///         it is used in creating the <see cref="HashAlgorithm"/> object; otherwise
        ///     </item>
        ///     <item>
        ///         Try to resolve the <see cref="HashAlgorithm"/> object directly from CSL; otherwise 
        ///     </item>
        ///     <item>
        ///         Try to resolve the name of the hash algorithm from the CSL with resolve name <see cref="HashAlgorithmName"/>; otherwise
        ///     </item>
        ///     <item>
        ///         Assume the default hash algorithm - <see cref="F:Algorithms.Hash.Default"/> - SHA256.
        ///     </item>
        /// </list>
        /// </remarks>
        public void Initialize(
            string hashAlgorithmName = null)
        {
            // 1. If the user passed hash algorithm name that is not null, empty or whitespace characters only, 
            //    it will be used in creating the <see cref="HashAlgorithm"/> object.
            if (!hashAlgorithmName.IsNullOrWhiteSpace())
                _hashAlgorithmName = hashAlgorithmName;
            else
            {
                try
                {
                    // 2. try to resolve the hash algorithm object directly from the CSL
                    _hashFactory = () => ServiceLocatorWrapper.Default.GetInstance<HashAlgorithm>();
                    using (var hashAlgorithm = _hashFactory())
                        // if we are here, we've got our factory.
                        return;
                }
                catch (ActivationException)
                {
                    // the hash algorithm is not registered in the CSL
                    _hashFactory = null;
                }

                try
                {
                    // 3. Try to resolve the name of the hash algorithm from the CSL with resolve name "DefaultHash" 
                    _hashAlgorithmName = ServiceLocatorWrapper.Default.GetInstance<string>(Algorithms.Hash.ResolveName);
                }
                catch (ActivationException)
                {
                }

                if (_hashAlgorithmName.IsNullOrWhiteSpace())
                    // 4. if the hash algorithm name has not been resolved so far, assume the default algorithm:
                    _hashAlgorithmName = Algorithms.Hash.Default;
            }

            // set the factory
            _hashFactory = () => HashAlgorithm.Create(_hashAlgorithmName);

            // try it
            using (var hashAlgorithm = _hashFactory())
                if (hashAlgorithm == null)
                    // if unsuccessful - throw an exception.
                    throw new ActivationException(
                                $"The name \"{_hashAlgorithmName}\" was not recognized as a valid hash algorithm.");
        }

        /// <summary>
        /// Creates a <see cref="HashAlgorithm"/> instance.
        /// </summary>
        /// <returns><see cref="HashAlgorithm"/> instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the factory could not resolve the hash algorithm.
        /// </exception>
        public HashAlgorithm Create()
        {
            if (_hashFactory == null)
                throw new InvalidOperationException("The factory was not initialized properly. Call Initialize first.");

            return _hashFactory();
        }

        /// <summary>
        /// Gets the name of the hash algorithm.
        /// </summary>
        /// <value>The name of the hash algorithm.</value>
        public string HashAlgorithmName => _hashAlgorithmName;
        #endregion
    }
}
