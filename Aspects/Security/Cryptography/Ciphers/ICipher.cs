using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface <c>ICipher</c> defines the behavior of objects which are configured according to some cryptographic scheme and
    /// have the responsibility to protect data for confidentiality; and optionally integrity and authentication with encryption, 
    /// crypto-hashing and signing.
    /// </summary>
    [ContractClass(typeof(ICipherContract))]
    public interface ICipher : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the encrypted texts are or should be Base64 encoded.
        /// The encrypted outputs or expected inputs are series of bytes - 1 byte ASCII characters.
        /// Note that the ciphers, which include a hash or a signature, do not support Base64 encoding and will
        /// throw <see cref="InvalidOperationException"/> if you try to set the property.
        /// </summary>
        bool Base64Encoded { get; set; }

        /// <summary>
        /// Reads the clear text from the <paramref name="dataStream"/> encrypts it and writes the result into the <paramref name="encryptedStream"/> 
        /// stream. This is the reverse method of <see cref="M:ICipherAsync.Decrypt(System.Stream, System.Stream)"/>.
        /// </summary>
        /// <param name="dataStream">
        /// The unencrypted input stream.
        /// </param>
        /// <param name="encryptedStream">
        /// The output stream where to write the crypto package which will contain the encrypted data 
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc.
        /// </param>
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
        void Encrypt(
            Stream dataStream,
            Stream encryptedStream);

        /// <summary>
        /// Reads and decrypts the <paramref name="encryptedStream"/> stream and writes the clear text into the <paramref name="dataStream"/> stream.
        /// This is the reverse method of <see cref="M:ICipherAsync.Encrypt(System.Stream, System.Stream)"/>.
        /// </summary>
        /// <param name="encryptedStream">
        /// The input crypto package stream which contains the encrypted data 
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc.
        /// </param>
        /// <param name="dataStream">
        /// The output stream where to put the unencrypted data.
        /// </param>
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
        void Decrypt(
            Stream encryptedStream,
            Stream dataStream);

        /// <summary>
        /// Encrypts the specified <paramref name="data"/>. This is the reverse method of <see cref="M:ICipherAsync.Decrypt(byte[])"/>.
        /// </summary>
        /// <param name="data">
        /// The data to be encrypted.
        /// </param>
        /// <returns>
        /// The bytes of the crypto package which contains the encrypted data 
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc.
        /// Or returns <see langword="null"/> if <paramref name="data"/> is <see langword="null"/>.
        /// </returns>
        /// <exception cref="T:System.Security.CryptographicException">
        /// The encryption failed.
        /// </exception>
        byte[] Encrypt(
            byte[] data);

        /// <summary>
        /// Decrypts the specified <paramref name="encryptedData"/>.
        /// This is the reverse method of <see cref="M:ICipherAsync.Encrypt(byte[])"/>.
        /// </summary>
        /// <param name="encryptedData">
        /// The bytes of the crypto package which contains the encrypted data 
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc.
        /// If <paramref name="encryptedData"/> is <see langword="null"/> the method returns <see langword="null"/>.
        /// </param>
        /// <returns>
        /// The decrypted <paramref name="encryptedData"/> or <see langword="null"/> if <paramref name="encryptedData"/> is <see langword="null"/>.
        /// </returns>
        /// <exception cref="T:System.Security.CryptographicException">
        /// The encryption failed.
        /// </exception>
        byte[] Decrypt(
            byte[] encryptedData);
    }

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
            Contract.Ensures(!(data==null ^ Contract.Result<byte[]>()==null));

            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            Contract.Ensures(!(encryptedData==null ^ Contract.Result<byte[]>()==null));

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
