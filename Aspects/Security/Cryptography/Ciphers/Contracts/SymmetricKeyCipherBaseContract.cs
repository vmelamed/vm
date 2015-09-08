using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
    [ContractClassFor(typeof(SymmetricKeyCipherBase))]
    abstract class SymmetricKeyCipherBaseContract : SymmetricKeyCipherBase
    {
        protected SymmetricKeyCipherBaseContract()
            : base(null)
        {
        }

        protected override byte[] EncryptSymmetricKey()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null && Contract.Result<byte[]>().Length > 0, "The method returned null or empty encrypted key.");

            throw new NotImplementedException();
        }

        protected override void DecryptSymmetricKey(
            byte[] encryptedKey)
        {
            Contract.Requires<ArgumentNullException>(encryptedKey != null, "encryptedKey");
            Contract.Requires<ArgumentNullException>(encryptedKey.Length > 0, "The argument \"encryptedKey\" cannot be empty.");

            throw new NotImplementedException();
        }
    }
}
