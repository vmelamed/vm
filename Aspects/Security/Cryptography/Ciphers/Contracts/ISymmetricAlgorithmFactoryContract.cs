using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
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

        #region IDisposable Members
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
