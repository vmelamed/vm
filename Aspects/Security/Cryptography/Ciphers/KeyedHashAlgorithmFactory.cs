using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class <c>KeyedHashAlgorithmFactory</c> encapsulates the strategy for determining and realizing the keyed hash algorithm.
    /// </summary>
    public sealed class KeyedHashAlgorithmFactory : IHashAlgorithmFactory
    {
        /// <summary>
        /// The resolved keyed hash algorithm name
        /// </summary>
        string _hashAlgorithmName;
        /// <summary>
        /// The generated keyed hash factory
        /// </summary>
        Func<KeyedHashAlgorithm> _hashFactory;

        #region IHashAlgorithmFactory members
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedHashAlgorithmFactory"/> class with optional keyed hash algorithm name.
        /// </summary>
        /// <param name="hashAlgorithmName">Name of the keyed hash algorithm.</param>
        /// <exception cref="ActivationException">
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
        ///         Try to resolve the <see cref="KeyedHashAlgorithm"/> object directly from CSL; otherwise 
        ///     </item>
        ///     <item>
        ///         Try to resolve the name of the keyed hash algorithm from the CSL with resolve name <see cref="Algorithms.KeyedHash.ResolveName"/>; otherwise
        ///     </item>
        ///     <item>
        ///         Assume the default keyed hash algorithm - <see cref="Algorithms.KeyedHash.Default"/> - SHA256.
        ///     </item>
        /// </list>
        /// </remarks>
        public void Initialize(
            string hashAlgorithmName = null)
        {
            // 1. If the user passed keyed hash algorithm name that is not null, empty or whitespace characters only, 
            //    it will be used in creating the <see cref="KeyedHashAlgorithm"/> object.
            if (!string.IsNullOrWhiteSpace(hashAlgorithmName))
                _hashAlgorithmName = hashAlgorithmName;
            else
            {
                try
                {
                    // 2. try to resolve the keyed hash algorithm object directly from the CSL
                    _hashFactory = () => ServiceLocatorWrapper.Default.GetInstance<KeyedHashAlgorithm>();
                    using (var hashAlgorithm = _hashFactory())
                        // if we are here, we've got our factory.
                        return;
                }
                catch (ActivationException)
                {
                    // the keyed hash algorithm is not registered in the CSL
                    _hashFactory = null;
                }

                try
                {
                    // 3. Try to resolve the name of the keyed hash algorithm from the CSL with resolve name "DefaultHash" 
                    _hashAlgorithmName = ServiceLocatorWrapper.Default.GetInstance<string>(Algorithms.KeyedHash.ResolveName);
                }
                catch (ActivationException)
                {
                }

                if (string.IsNullOrWhiteSpace(_hashAlgorithmName))
                    // 4. if the keyed hash algorithm name has not been resolved so far, assume the default algorithm:
                    _hashAlgorithmName = Algorithms.Hash.Default;
            }

            // set the factory
            _hashFactory = () => KeyedHashAlgorithm.Create(_hashAlgorithmName);

            // try it
            using (var hashAlgorithm = _hashFactory())
                if (hashAlgorithm == null)
                    // if unsuccessful - throw an exception.
                    throw new ActivationException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "The name \"{0}\" was not recognized as a valid keyed hash algorithm.",
                                    _hashAlgorithmName));
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
        public string HashAlgorithmName
        {
            get { return _hashAlgorithmName; }
        }
        #endregion
    }
}
