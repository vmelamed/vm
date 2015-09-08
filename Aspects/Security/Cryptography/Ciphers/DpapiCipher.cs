using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
#if NET45
using System.Threading.Tasks;
#endif

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class DpapiCipher uses DPAPI to encrypt and decrypt the data. The cipher is appropriate for Protecting small amounts of data for 
    /// applications that run on a single machine and do not use this cipher for exchange of information with other machines.
    /// </summary>
    /// <remarks>
    /// Crypto package contents:
    /// <list type="number">
    /// <item><description>The bytes of the encrypted text.</description></item>
    /// </list>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification="Nothing to dispose")]
    public sealed class DpapiCipher : ICipherAsync
    {
        const int BlockLength = 4096;

        /// <summary>
        /// Initializes a new instance of the <see cref="DpapiCipher"/> class with scope <see cref="System.Security.Cryptography.DataProtectionScope.LocalMachine"/> and no entropy.
        /// </summary>
        public DpapiCipher()
            : this(DataProtectionScope.LocalMachine, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DpapiCipher"/> class with user specified scope and
        /// user defined entropy which makes the encryption stronger.
        /// </summary>
        /// <param name="scope">The scope (we recommend <see cref="System.Security.Cryptography.DataProtectionScope.LocalMachine"/>).</param>
        /// <param name="entropy">The entropy makes the encryption stronger and should be kept secret. Appropriate value may be a password entered by the user.</param>
        public DpapiCipher(
            DataProtectionScope scope,
            byte[] entropy)
        {
            Scope   = scope;
            Entropy = entropy;
        }

        #region Properties
        /// <summary>
        /// Gets or sets the entropy which makes the encryption stronger.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="It is an opaque value.")]
        public byte[] Entropy { get; set; }

        /// <summary>
        /// Gets or sets the DPAPI scope: current user or local machine (the recommended).
        /// </summary>
        public DataProtectionScope Scope { get; set; }
        #endregion

        #region ICipher Members
        /// <summary>
        /// Gets or sets a value indicating whether the encrypted texts are or should be Base64 encoded.
        /// </summary>
        public bool Base64Encoded { get; set; }

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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="The CryptoStream will do it.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public void Encrypt(
            Stream dataStream,
            Stream encryptedStream)
        {
            var buffer = new byte[BlockLength];
            var read = 0;
            var first = true;

            // if the output needs to be Base-64 encoded, wrap the encoded stream in a crypto stream applying Base64 transformation
            Stream outputStream = Base64Encoded
                                    ? new CryptoStream(encryptedStream, new ToBase64Transform(), CryptoStreamMode.Write)
                                    : encryptedStream;

            try
            {
                do
                {
                    read = dataStream.Read(buffer, 0, buffer.Length);

                    if (!first  &&  read == 0)
                        return;
                    first = false;
                    if (read < buffer.Length)
                        Array.Resize(ref buffer, read);

                    var encrypted = Encrypt(buffer);

                    outputStream.Write(BitConverter.GetBytes(encrypted.Length), 0, sizeof(int));
                    outputStream.Write(encrypted, 0, encrypted.Length);
                }
                while (read == BlockLength);
            }
            finally
            {
                if (Base64Encoded)
                    outputStream.Dispose();
            }

            outputStream.Flush();
        }

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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="The CryptoStream will do it.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public void Decrypt(
            Stream encryptedStream,
            Stream dataStream)
        {
            var bufferLenBytes = new byte[sizeof(int)];
            int bufferLen;
            byte[] buffer = null;
            byte[] decrypted = null;
            var read = 0;
            var first = true;

            // if the output needs to be Base-64 encoded, wrap the encoded stream in a crypto stream applying Base64 transformation
            Stream inputStream = Base64Encoded
                                    ? new CryptoStream(encryptedStream, new FromBase64Transform(), CryptoStreamMode.Read)
                                    : encryptedStream;

            try
            {
                do
                {
                    read = inputStream.Read(bufferLenBytes, 0, bufferLenBytes.Length);

                    if (!first && read == 0)
                        return;
                    first = false;
                    if (read != bufferLenBytes.Length)
                        throw new ArgumentException("The input stream does not seem to be produced with compatible cipher.", "encryptedStream");

                    bufferLen = BitConverter.ToInt32(bufferLenBytes, 0);

                    if (buffer == null)
                        buffer = new byte[bufferLen];
                    else
                        if (buffer.Length != bufferLen)
                            Array.Resize(ref buffer, bufferLen);

                    read = inputStream.Read(buffer, 0, bufferLen);

                    if (read != bufferLen)
                        throw new ArgumentException("The input stream does not seem to be produced with compatible cipher.", "encryptedStream");

                    decrypted = Decrypt(buffer);

                    dataStream.Write(decrypted, 0, decrypted.Length);
                }
                while (decrypted.Length == BlockLength);
            }
            finally
            {
                if (Base64Encoded)
                    inputStream.Dispose();
            }
        }

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
        public byte[] Encrypt(
            byte[] data)
        {
            if (data == null)
                return null;

            return ProtectedData.Protect(data, Entropy, Scope);
        }

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
        public byte[] Decrypt(
            byte[] encryptedData)
        {
            if (encryptedData == null)
                return null;

            return ProtectedData.Unprotect(encryptedData, Entropy, Scope);
        }
        #endregion

#if NET45
        #region ICipherAsync Members
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public async Task EncryptAsync(
            Stream dataStream,
            Stream encryptedStream)
        {
            var buffer = new byte[BlockLength];
            var read = 0;
            var first = true;

            do
            {
                read = await dataStream.ReadAsync(buffer, 0, buffer.Length);

                if (first)
                    first = false;
                else
                    if (read == 0)
                        return;

                if (read < buffer.Length)
                    Array.Resize(ref buffer, read);

                var encrypted = await Task.Run(() => Encrypt(buffer));

                await encryptedStream.WriteAsync(BitConverter.GetBytes(encrypted.Length), 0, sizeof(int));
                await encryptedStream.WriteAsync(encrypted, 0, encrypted.Length);
            }
            while (read == BlockLength);
        }

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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public async Task DecryptAsync(
            Stream encryptedStream,
            Stream dataStream)
        {
            var bufferLenBytes = new byte[sizeof(int)];
            int bufferLen;
            byte[] buffer = null;
            byte[] decrypted = null;
            var read = 0;
            var first = true;

            do
            {
                read = await encryptedStream.ReadAsync(bufferLenBytes, 0, bufferLenBytes.Length);

                if (first)
                    first = false;
                else
                    if (read == 0)
                        return;

                if (read != bufferLenBytes.Length)
                    throw new ArgumentException("The input stream does not seem to be produced with compatible cipher.", "encryptedStream");

                bufferLen = BitConverter.ToInt32(bufferLenBytes, 0);

                if (buffer == null)
                    buffer = new byte[bufferLen];
                else
                    if (buffer.Length != bufferLen)
                        Array.Resize(ref buffer, bufferLen);

                read = await encryptedStream.ReadAsync(buffer, 0, bufferLen);

                if (read != bufferLen)
                    throw new ArgumentException("The input stream does not seem to be produced with compatible cipher.", "encryptedStream");

                decrypted = await Task.Run(() => Decrypt(buffer));
                await dataStream.WriteAsync(decrypted, 0, decrypted.Length);
            }
            while (decrypted.Length == BlockLength);
        }
        #endregion
#endif

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources, here it does nothing.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification="Nothing to dispose")]
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification="Nothing to dispose")]
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
