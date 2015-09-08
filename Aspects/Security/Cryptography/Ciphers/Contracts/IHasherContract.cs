using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
    [ContractClassFor(typeof(IHasher))]
    abstract class IHasherContract : IHasher
    {
        #region IHasher Members

        public int SaltLength
        {
            get
            {
                Contract.Ensures(Contract.Result<int>()==0 || Contract.Result<int>()>=Hasher.DefaultSaltLength, "The salt length should be either 0 or not less than 8 bytes.");

                throw new NotImplementedException();
            }
            set
            {
                Contract.Requires<ArgumentException>(value==0 || value>=8, "The salt length should be either 0 or not less than 8 bytes.");

                throw new NotImplementedException();
            }
        }

        public byte[] Hash(
            Stream dataStream)
        {
            Contract.Requires<ArgumentException>(dataStream==null || dataStream.CanRead, "The \"dataStream\" cannot be read from.");
            Contract.Ensures(!(dataStream==null ^ Contract.Result<byte[]>()==null), "The returned value is invalid.");

            throw new NotImplementedException();
        }

        public bool TryVerifyHash(
            Stream dataStream,
            byte[] hash)
        {
            Contract.Requires<ArgumentException>(dataStream==null || dataStream.CanRead, "The \"dataStream\" cannot be read from.");
            Contract.Requires<ArgumentNullException>(dataStream==null || hash!=null, "hash");
            Contract.Ensures(dataStream!=null || Contract.Result<bool>()==(hash==null), "Invalid return value.");

            throw new NotImplementedException();
        }

        public byte[] Hash(
            byte[] data)
        {
            Contract.Ensures(!(data==null ^ Contract.Result<byte[]>()==null), "Invalid return value.");

            throw new NotImplementedException();
        }

        public bool TryVerifyHash(byte[] data, byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(data==null || hash!=null, "hash");
            Contract.Ensures(data!=null || Contract.Result<bool>()==(hash==null), "Invalid return value.");

            throw new NotImplementedException();
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
