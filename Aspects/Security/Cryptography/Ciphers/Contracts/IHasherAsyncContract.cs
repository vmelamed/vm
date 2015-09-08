using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
    [ContractClassFor(typeof(IHasherAsync))]
    abstract class IHasherAsyncContract : IHasherAsync
    {
        #region IHasherAsync Members

        public Task<byte[]> HashAsync(Stream dataStream)
        {
            Contract.Requires<ArgumentException>(dataStream==null || dataStream.CanRead, "The \"dataStream\" cannot be read from.");
            Contract.Ensures(!(dataStream==null ^ Contract.Result<byte[]>()==null), "The returned value is invalid.");

            throw new NotImplementedException();
        }

        public Task<bool> TryVerifyHashAsync(Stream dataStream, byte[] hash)
        {
            Contract.Requires<ArgumentException>(dataStream==null || dataStream.CanRead, "The \"dataStream\" cannot be read from.");
            Contract.Requires<ArgumentNullException>(dataStream==null || hash!=null, "hash");
            Contract.Ensures(dataStream!=null || Contract.Result<bool>()==(hash==null), "Invalid return value.");

            throw new NotImplementedException();
        }

        #endregion

        #region IHasher Members

        public int SaltLength
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public byte[] Hash(System.IO.Stream dataStream)
        {
            throw new NotImplementedException();
        }

        public bool TryVerifyHash(System.IO.Stream dataStream, byte[] hash)
        {
            throw new NotImplementedException();
        }

        public byte[] Hash(byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool TryVerifyHash(byte[] data, byte[] hash)
        {
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
