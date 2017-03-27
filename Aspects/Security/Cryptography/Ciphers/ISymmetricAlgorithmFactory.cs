using Microsoft.Practices.ServiceLocation;
using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using vm.Aspects.Security.Cryptography.Ciphers.Algorithms;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Interface <c>ISymmetricAlgorithmFactory</c> defines the behavior of an object factory which creates 
    /// the underlying <see cref="Symmetric"/> objects. The factory must implement a strategy for picking the
    /// symmetric algorithm given choices like, parameters, Common Service Locator registrations, default values, etc.
    /// </summary>
    [ContractClass(typeof(ISymmetricAlgorithmFactoryContract))]
    public interface ISymmetricAlgorithmFactory
    {
        /// <summary>
        /// Initializes the factory with an optional symmetric algorithm name.
        /// Possibly implements the resolution strategy and initializes the factory with the appropriate values.
        /// </summary>
        void Initialize(string symmetricAlgorithmName = null);

        /// <summary>
        /// Creates a <see cref="Symmetric"/> instance.
        /// </summary>
        /// <returns><see cref="Symmetric"/> instance.</returns>
        /// <exception cref="ActivationException">
        /// If the factory could not resolve the symmetric algorithm.
        /// </exception>
        SymmetricAlgorithm Create();

        /// <summary>
        /// Gets the name of the symmetric algorithm.
        /// </summary>
        /// <value>The name of the symmetric algorithm.</value>
        string SymmetricAlgorithmName { get; }
    }

    [ContractClassFor(typeof(ISymmetricAlgorithmFactory))]
    abstract class ISymmetricAlgorithmFactoryContract : ISymmetricAlgorithmFactory
    {
        #region ISymmetricAlgorithmFactory Members
        public void Initialize(
            string symmetricAlgorithmName)
        {
        }

        public SymmetricAlgorithm Create()
        {
            Contract.Ensures(Contract.Result<SymmetricAlgorithm>() != null, "Could not create a symmetric algorithm.");

            throw new NotImplementedException();
        }

        public string SymmetricAlgorithmName
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
