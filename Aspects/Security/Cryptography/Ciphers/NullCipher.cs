using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The class <c>NullCipher</c> is a development- and test-friendly convenience class which implements trivially the <see cref="ICipherAsync"/> interface:
    /// copies the source data blindly into the output target.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "N/A")]
    public sealed class NullCipher : ICipherAsync
    {
        #region ICipher Members
        /// <summary>
        /// Gets or sets a value indicating whether the encrypted texts are or should be Base64 encoded.
        /// </summary>
        public bool Base64Encoded { get; set; }

        /// <summary>
        /// Encrypts (here it just copies) the <paramref name="dataStream" /> stream into the <paramref name="encryptedStream" /> stream.
        /// This is the reverse method of <see cref="M:ICipherAsync.Decrypt(System.Stream, System.Stream)" />.
        /// </summary>
        /// <param name="dataStream">The unencrypted input stream.</param>
        /// <param name="encryptedStream">
        /// The output stream where to put the crypto package which will contain the encrypted data as well as some other crypto artifacts, 
        /// e.g. initialization vector, hash, etc.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// dataStream
        /// or
        /// encryptedStream
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Encrypt(
            Stream dataStream,
            Stream encryptedStream)
        {
            dataStream.CopyTo(encryptedStream);
        }

        /// <summary>
        /// Decrypts (here it just copies) the <paramref name="encryptedStream" /> stream into the <paramref name="dataStream" /> stream.
        /// This is the reverse method of <see cref="M:ICipherAsync.Encrypt(System.Stream, System.Stream)" />.
        /// </summary>
        /// <param name="encryptedStream">
        /// The input crypto package stream which contains the encrypted data as well as some other crypto artifacts, 
        /// e.g. initialization vector, hash, etc.
        /// </param>
        /// <param name="dataStream">The output stream where to put the unencrypted data.</param>
        /// <exception cref="System.ArgumentNullException">
        /// dataStream
        /// or
        /// encryptedStream
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Decrypt(
            Stream encryptedStream,
            Stream dataStream)
        {
            encryptedStream.CopyTo(dataStream);
        }

        /// <summary>
        /// Encrypts (here it just copies) the specified <paramref name="data" />.
        /// This is the reverse method of <see cref="M:ICipherAsync.Decrypt(byte[])" />.
        /// </summary>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The bytes of the crypto package which contains the encrypted data as well as some other crypto artifacts, e.g. initialization vector, hash, etc.
        /// Or returns <see langword="null" /> if <paramref name="data" /> is <see langword="null" />.</returns>
        public byte[] Encrypt(
            byte[] data)
        {
            if (data == null)
                return null;

            return (byte[])data.Clone();
        }

        /// <summary>
        /// Decrypts (here it just copies) the specified <paramref name="encryptedData" />.
        /// This is the reverse method of <see cref="M:ICipherAsync.Encrypt(byte[])" />.
        /// </summary>
        /// <param name="encryptedData">The bytes of the crypto package which contains the encrypted data as well as some other crypto artifacts, e.g. initialization vector, hash, etc.
        /// If <paramref name="encryptedData" /> is <see langword="null" /> the method returns <see langword="null" />.</param>
        /// <returns>The decrypted <paramref name="encryptedData" /> or <see langword="null" /> if <paramref name="encryptedData" /> is <see langword="null" />.</returns>
        public byte[] Decrypt(
            byte[] encryptedData)
        {
            if (encryptedData == null)
                return null;

            return (byte[])encryptedData.Clone();
        }
        #endregion

        #region ICipherAsync Members
        /// <summary>
        /// encrypt as an asynchronous operation.
        /// </summary>
        /// <param name="dataStream">The unencrypted input stream.</param>
        /// <param name="encryptedStream">The output stream where to write the crypto package which will contain the encrypted data
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc.</param>
        /// <returns>A <see cref="Task" /> object which represents the process of asynchronous encryption.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// dataStream
        /// or
        /// encryptedStream
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public async Task EncryptAsync(
            Stream dataStream,
            Stream encryptedStream)
        {
            await dataStream.CopyToAsync(encryptedStream);
        }

        /// <summary>
        /// decrypt as an asynchronous operation.
        /// </summary>
        /// <param name="encryptedStream">The input crypto package stream which contains the encrypted data
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc.</param>
        /// <param name="dataStream">The output stream where to put the unencrypted data.</param>
        /// <returns>A <see cref="Task" /> object which represents the process of asynchronous decryption.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// dataStream
        /// or
        /// encryptedStream
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public async Task DecryptAsync(
            Stream encryptedStream,
            Stream dataStream)
        {
            await encryptedStream.CopyToAsync(dataStream);
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "N/A")]
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
