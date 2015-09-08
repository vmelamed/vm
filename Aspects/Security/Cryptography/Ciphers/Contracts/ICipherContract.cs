using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
    [ContractClassFor(typeof(ICipher))]
    abstract class ICipherContract : ICipher
    {
        #region ICipher Members
        public bool Base64Encoded { get; set; }

        public void Encrypt(Stream dataStream, Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(dataStream != null, "dataStream");
            Contract.Requires<ArgumentNullException>(encryptedStream != null, "encryptedStream");
            Contract.Requires<ArgumentException>(dataStream.CanRead, "The argument \"dataStream\" cannot be read from.");
            Contract.Requires<ArgumentException>(encryptedStream.CanWrite, "The argument \"encryptedStream\" cannot be written to.");

            throw new NotImplementedException();
        }

        public void Decrypt(Stream encryptedStream, Stream dataStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, "encryptedStream");
            Contract.Requires<ArgumentNullException>(dataStream != null, "dataStream");
            Contract.Requires<ArgumentException>(encryptedStream.CanRead, "The argument \"dataStream\" cannot be read from.");
            Contract.Requires<ArgumentException>(dataStream.CanWrite, "The argument \"encryptedStream\" cannot be written to.");

            throw new NotImplementedException();
        }

        public byte[] Encrypt(byte[] data)
        {
            Contract.Ensures(data==null && Contract.Result<byte[]>()==null ||
                             data!=null && Contract.Result<byte[]>()!=null);

            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            Contract.Ensures(encryptedData==null && Contract.Result<byte[]>()==null ||
                             encryptedData!=null && Contract.Result<byte[]>()!=null);

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
