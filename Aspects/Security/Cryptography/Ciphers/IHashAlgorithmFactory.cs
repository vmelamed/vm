using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Interface <c>IHashAlgorithmFactory</c> defines the behavior of an object factory which creates 
    /// the underlying <see cref="T:HashAlgorithm"/> objects. The factory must implement a strategy for picking the
    /// hash algorithm given choices like, parameters, Common Service Locator registrations, default values, etc.
    /// </summary>
    [ContractClass(typeof(IHashAlgorithmFactoryContract))]
    public interface IHashAlgorithmFactory : IDisposable
    {
        /// <summary>
        /// Initializes the factory with an optional hash algorithm name.
        /// Possibly implements the resolution strategy and initializes the factory with the appropriate values.
        /// </summary>
        void Initialize(string hashAlgorithmName);

        /// <summary>
        /// Creates a <see cref="T:HashAlgorithm"/> instance.
        /// </summary>
        /// <returns><see cref="T:HashAlgorithm"/> instance.</returns>
        /// <exception cref="T:ActivationException">
        /// If the factory could not resolve the hash algorithm.
        /// </exception>
        HashAlgorithm Create();

        /// <summary>
        /// Gets the name of the hash algorithm.
        /// </summary>
        /// <value>The name of the hash algorithm.</value>
        string HashAlgorithmName { get; }
    }

    [ContractClassFor(typeof(IHashAlgorithmFactory))]
    abstract class IHashAlgorithmFactoryContract : IHashAlgorithmFactory
    {
        #region IHashAlgorithmFactory Members

        public void Initialize(
            string hashAlgorithmName)
        {
            throw new NotImplementedException();
        }

        public HashAlgorithm Create()
        {
            Contract.Ensures(Contract.Result<HashAlgorithm>() != null, "Could not create a hash algorithm.");

            throw new NotImplementedException();
        }

        public string HashAlgorithmName
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
