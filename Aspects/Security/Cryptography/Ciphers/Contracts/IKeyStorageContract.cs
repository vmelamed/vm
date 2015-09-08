using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
    [ContractClassFor(typeof(IKeyStorage))]
    abstract class IKeyStorageContract : IKeyStorage
    {
        #region IKeyStorage Members

        public bool KeyLocationExists(string keyLocation)
        {
            Contract.Requires<ArgumentNullException>(keyLocation != null, "keyLocation");
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(keyLocation), "The argument \"keyLocation\" cannot be empty or consist of whitespace characters only.");

            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public void PutKey(byte[] key, string keyLocation)
        {
            Contract.Requires<ArgumentNullException>(key != null, "key");
            Contract.Requires<ArgumentException>(key.Length != 0, "The length of the array in the argument \"key\" cannot be 0.");
            Contract.Requires<ArgumentNullException>(keyLocation != null, "keyLocation");
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(keyLocation), "The argument \"keyLocation\" cannot be empty or consist of whitespace characters only.");

            throw new NotImplementedException();
        }

        public byte[] GetKey(string keyLocation)
        {
            Contract.Requires<ArgumentNullException>(keyLocation != null, "keyLocation");
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(keyLocation), "The argument \"keyLocation\" cannot be empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<byte[]>() != null, "The returned key is null.");
            Contract.Ensures(Contract.Result<byte[]>().Length != 0, "The returned key has 0 length.");

            throw new NotImplementedException();
        }

        public void DeleteKeyLocation(string keyLocation)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
