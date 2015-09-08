using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
    [ContractClassFor(typeof(IKeyManagement))]
    abstract class IKeyManagementContract : IKeyManagement
    {
        #region IKeyManagement Members
        public string KeyLocation
        {
            get
            {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()), "The key location cannot be null, empty or consist of whitespace characters only.");

                throw new NotImplementedException();
            }
        }

        public byte[] ExportSymmetricKey()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ExportSymmetricKeyAsync()
        {
            throw new NotImplementedException();
        }

        public void ImportSymmetricKey(byte[] key)
        {
            Contract.Requires<ArgumentNullException>(key != null, "key");
            Contract.Requires<ArgumentException>(key.Length > 0, "The length of the imported key is 0");

            throw new NotImplementedException();
        }

        public Task ImportSymmetricKeyAsync(byte[] key)
        {
            Contract.Requires<ArgumentNullException>(key != null, "key");
            Contract.Requires<ArgumentException>(key.Length > 0, "The length of the imported key is 0");

            throw new NotImplementedException();
        }
        #endregion
    }
}
