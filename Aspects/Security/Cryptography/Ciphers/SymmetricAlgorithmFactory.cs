﻿using System;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class <c>SymmetricAlgorithmFactory</c> encapsulates the strategy for determining and realizing the symmetric algorithm.
    /// </summary>
    public sealed class SymmetricAlgorithmFactory : ISymmetricAlgorithmFactory
    {
        /// <summary>
        /// The resolved symmetric algorithm name
        /// </summary>
        string _symmetricAlgorithmName;
        /// <summary>
        /// The generated symmetric factory
        /// </summary>
        Func<SymmetricAlgorithm> _symmetricAlgorithmFactory;

        #region ISymmetricAlgorithmFactory Members

        /// <summary>
        /// Initializes the factory with an optional symmetric algorithm name.
        /// Possibly implements the resolution strategy and initializes the factory with the appropriate values.
        /// </summary>
        /// <param name="symmetricAlgorithmName">Name of the symmetric algorithm.</param>
        /// <exception cref="Microsoft.Practices.ServiceLocation.ActivationException"></exception>
        public void Initialize(
            string symmetricAlgorithmName = null)
        {
            // 1. If the user passed symmetric algorithm name that is not null, empty or whitespace characters only, 
            //    it will be used in creating the <see cref="T:Symmetric"/> object.
            if (!string.IsNullOrWhiteSpace(symmetricAlgorithmName))
                _symmetricAlgorithmName = symmetricAlgorithmName;
            else
            {
                try
                {
                    // 2. try to resolve the symmetric algorithm object directly from the CSL
                    _symmetricAlgorithmFactory = () => ServiceLocatorWrapper.Default.GetInstance<SymmetricAlgorithm>();

                    using (var symmetricAlgorithm = _symmetricAlgorithmFactory())
                        // if we are here, we've got our factory.
                        return;
                }
                catch (ActivationException)
                {
                    // the symmetric algorithm is not registered in the CSL
                    _symmetricAlgorithmFactory = null;
                }

                try
                {
                    // 3. Try to resolve the name of the symmetric algorithm from the CSL with resolve name "DefaultSymmetricAlgorithm" 
                    _symmetricAlgorithmName = ServiceLocatorWrapper.Default.GetInstance<string>(Algorithms.Symmetric.ResolveName);
                }
                catch (ActivationException)
                {
                }

                if (string.IsNullOrWhiteSpace(_symmetricAlgorithmName))
                    // 4. if the symmetric algorithm name has not been resolved so far, assume the default algorithm:
                    _symmetricAlgorithmName = Algorithms.Symmetric.Default;
            }

            // set the factory
            _symmetricAlgorithmFactory = () => SymmetricAlgorithm.Create(_symmetricAlgorithmName);

            // try it, if successful we'll store it and return it in the first call to Create()
            using (var symmetricAlgorithm = _symmetricAlgorithmFactory())
                if (symmetricAlgorithm == null)
                    // if unsuccessful - throw an exception.
                    throw new ActivationException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "The name \"{0}\" was not recognized as a valid symmetric algorithm.",
                                    _symmetricAlgorithmName));
        }

        /// <summary>
        /// Creates a <see cref="SymmetricAlgorithm" /> instance.
        /// </summary>
        /// <returns><see cref="SymmetricAlgorithm" /> instance.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public SymmetricAlgorithm Create()
        {
            if (_symmetricAlgorithmFactory == null)
                throw new InvalidOperationException("The factory was not initialized properly. Call Initialize first.");

            return _symmetricAlgorithmFactory();
        }

        /// <summary>
        /// Gets the name of the symmetric algorithm.
        /// </summary>
        /// <value>The name of the symmetric algorithm.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public string SymmetricAlgorithmName => _symmetricAlgorithmName;

        #endregion
    }
}
