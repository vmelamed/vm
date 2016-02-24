using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface <c>ICipherAsync</c> extends the <see cref="T:ICipherAsync"/> interface with asynchronous versions of
    /// its <see cref="T:Stream"/> related methods.
    /// </summary>
    [ContractClass(typeof(ICipherAsyncContract))]
    public interface ICipherAsync : ICipher
    {
        /// <summary>
        /// Asynchronously reads the clear text from the <paramref name="dataStream"/>, encrypts it and writes the result into the 
        /// <paramref name="encryptedStream"/> stream. This is the reverse method of <see cref="M:ICipherAsync.DecryptAsync(System.Stream, System.Stream)"/>.
        /// </summary>
        /// <param name="dataStream">
        /// The unencrypted input stream.
        /// </param>
        /// <param name="encryptedStream">
        /// The output stream where to write the crypto package which will contain the encrypted data 
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc.
        /// </param>
        /// <returns>
        /// A <see cref="T:Task"/> object which represents the process of asynchronous encryption.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when either <paramref name="dataStream"/> or <paramref name="encryptedStream"/> are <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// Thrown when either <paramref name="dataStream"/> cannot be read or 
        /// <paramref name="encryptedStream"/> cannot be written to.
        /// </exception>
        /// <exception cref="T:System.Security.CryptographicException">
        /// The encryption failed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurred.
        /// </exception>
        Task EncryptAsync(
            Stream dataStream,
            Stream encryptedStream);

        /// <summary>
        /// Asynchronously reads and decrypts the <paramref name="encryptedStream"/> stream and writes the clear text into the 
        /// <paramref name="dataStream"/> stream. This is the reverse method of <see cref="M:ICipherAsync.EncryptAsync(System.Stream, System.Stream)"/>.
        /// </summary>
        /// <param name="encryptedStream">
        /// The input crypto package stream which contains the encrypted data 
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc.
        /// </param>
        /// <param name="dataStream">
        /// The output stream where to put the unencrypted data.
        /// </param>
        /// <returns>
        /// A <see cref="T:Task"/> object which represents the process of asynchronous decryption.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when either <paramref name="encryptedStream"/> or <paramref name="dataStream"/> are <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// Thrown when either <paramref name="dataStream"/> cannot be written to or 
        /// <paramref name="encryptedStream"/> cannot be read from.
        /// </exception>
        /// <exception cref="T:System.Security.CryptographicException">
        /// The decryption failed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurred.
        /// </exception>
        Task DecryptAsync(
            Stream encryptedStream,
            Stream dataStream);
    }

    [ContractClassFor(typeof(ICipherAsync))]
    abstract class ICipherAsyncContract : ICipherAsync
    {
        #region ICipherAsync Members
        public Task EncryptAsync(Stream dataStream, Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(dataStream != null, nameof(dataStream));
            Contract.Requires<ArgumentNullException>(encryptedStream != null, nameof(encryptedStream));
            Contract.Requires<ArgumentException>(dataStream.CanRead, "The argument "+nameof(dataStream)+" cannot be read from.");
            Contract.Requires<ArgumentException>(encryptedStream.CanWrite, "The argument "+nameof(encryptedStream)+" cannot be written to.");

            throw new NotImplementedException();
        }

        public Task DecryptAsync(Stream encryptedStream, Stream dataStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, nameof(encryptedStream));
            Contract.Requires<ArgumentNullException>(dataStream != null, nameof(dataStream));
            Contract.Requires<ArgumentException>(encryptedStream.CanRead, "The argument "+nameof(dataStream)+" cannot be read from.");
            Contract.Requires<ArgumentException>(dataStream.CanWrite, "The argument "+nameof(encryptedStream)+" cannot be written to.");

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
