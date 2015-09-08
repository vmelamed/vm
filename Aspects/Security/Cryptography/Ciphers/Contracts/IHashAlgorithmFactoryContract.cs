using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
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
