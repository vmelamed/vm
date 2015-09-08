using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
    [ContractClassFor(typeof(ICipherAsync))]
    abstract class ICipherAsyncContract : ICipherAsync
    {
        #region ICipherAsync Members
        public Task EncryptAsync(Stream dataStream, Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(dataStream != null, "dataStream");
            Contract.Requires<ArgumentNullException>(encryptedStream != null, "encryptedStream");
            Contract.Requires<ArgumentException>(dataStream.CanRead, "The argument \"dataStream\" cannot be read from.");
            Contract.Requires<ArgumentException>(encryptedStream.CanWrite, "The argument \"encryptedStream\" cannot be written to.");

            throw new NotImplementedException();
        }

        public Task DecryptAsync(Stream encryptedStream, Stream dataStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, "encryptedStream");
            Contract.Requires<ArgumentNullException>(dataStream != null, "dataStream");
            Contract.Requires<ArgumentException>(encryptedStream.CanRead, "The argument \"dataStream\" cannot be read from.");
            Contract.Requires<ArgumentException>(dataStream.CanWrite, "The argument \"encryptedStream\" cannot be written to.");

            throw new NotImplementedException();
        }
        #endregion

        #region ICipher Members
        public bool Base64Encoded { get; set; }

        public void Encrypt(
            Stream dataStream,
            Stream encryptedStream)
        {
            throw new NotImplementedException();
        }

        public void Decrypt(
            Stream encryptedStream,
            Stream dataStream)
        {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] encryptedData)
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
