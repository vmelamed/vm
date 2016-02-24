using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The <c>ProtectedKeyCipher</c> is a symmetric cipher. The symmetric key is encrypted using DPAPI and so stored in a file.
    /// This class also defines a set of crypto-operations (virtual protected methods) 
    /// for the descending classes which implement the various steps of the (GoF template) methods that compose and decompose the crypto-packages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The cipher can be used to protect data at rest, e.g. data stored in a database, file, etc.
    /// </para><para>
    /// By default the cipher uses the <see cref="T:System.Security.Cryptography.AesCryptoServiceProvider"/> with default parameters. 
    /// </para><para>
    /// The encrypted symmetric key is stored in a file. The class determines the path and name of the file from an <see cref="IKeyLocationStrategy"/> object.
    /// If the key file does not exist a new key is generated, encrypted and saved in a storage (<see cref="IKeyStorage"/>) in the location determined by the 
    /// <see cref="IKeyLocationStrategy"/> object.
    /// </para><para>
    /// DPAPI uses different keys for different machines and users. Therefore to enable your application to exchange and persist encrypted data with 
    /// other machines and applications (data in motion), you would need to export the clear text of the key and then import it to the target machines.
    /// The command line utility <c>ProtectedKey</c> can be used for this purpose.  More elaborate management can be implemented by using the 
    /// <see cref="IKeyManagement"/>'s methods <see cref="M:ProtectedKeyCipher.ExportSymmetricKey"/> and 
    /// <see cref="M:ProtectedKeyCipher.ImportSymmetricKey"/>. Please, follow the best security practices while handling the clear text of the key.
    /// </para><para>
    /// The symmetric key is generated and encrypted into a file once and read and decrypted from a file once per the life time of the cipher object.
    /// If the symmetric key is compromised, all documents encrypted with it will be compromised. If higher level of security is required, consider using the
    /// <see cref="EncryptedKeyCipher"/> which generates the symmetric key and encrypts it with asymmetric key, e.g. from a certificate, 
    /// instead of using DPAPI. If all participating machines share the same certificate, the file can be shared as is - without exporting and importing of the key in clear text.
    /// A third option is to use <see cref="EncryptedNewKeyCipher"/> which generates a new key for each document. 
    /// The latter encrypts the symmetric key with an asymmetric key, e.g. from a certificate, and stores it in the crypto package together with the 
    /// initialization vector and the crypto text. No need for key management whatsoever, however the performance is lowered.
    /// </para><para>
    /// <b>Note that the DPAPI encryption of the key is done in the scope of the local machine. This means that anyone logged-on locally to the machine with read-access to the file could 
    /// obtain the clear text of the symmetric key. Best practice is to protect the key file from access by other users. Therefore the key file is created with allowed access 
    /// (full control) only to the current user, the SYSTEM account and the Administrators group.</b>
    /// While it is possible to change the protection scope to the current user, usually this is highly impractical: the key management must be done under the same security
    /// context as the application's. Especially for web applications, services, etc. which usually run under accounts with very limited rights 
    /// this is almost impossible scenario which might pose other security risks.
    /// </para><para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item>Length of the encrypted symmetric cipher initialization vector (serialized Int32) - 4 bytes.</item>
    ///         <item>The bytes of the encrypted initialization vector.</item>
    ///         <item>The bytes of the encrypted text.</item>
    ///     </list>
    /// </para><para>
    /// The cipher can also be used to encrypt elements of or entire XML documents.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is implemented correctly.")]
    public class ProtectedKeyCipher : SymmetricKeyCipherBase, ICipherAsync
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedKeyCipher"/> class.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// Also a string instance with name &quot;DefaultSymmetricEncryption&quot; can be defined in a Common Service Locator compatible dependency injection container.
        /// </param>
        /// <param name="symmetricKeyLocation">
        /// Seeding name of store location name of the encrypted symmetric key (e.g. relative or absolute path).
        /// Can be <see langword="null"/>, empty or whitespace characters only.
        /// The parameter will be passed to the <paramref name="symmetricKeyLocationStrategy"/> to determine the final store location name path (e.g. relative or absolute path).
        /// </param>
        /// <param name="symmetricKeyLocationStrategy">
        /// Object which implements the strategy for determining the store location name (e.g. path and filename) of the encrypted symmetric key.
        /// If <see langword="null"/> it defaults to a new instance of the class <see cref="KeyLocationStrategy"/>.
        /// Alternatively an implementation type can be registered in a common service locator compatible DI container.
        /// </param>
        /// <param name="keyStorage">
        /// Object which implements the storing and retrieving of the the encrypted symmetric key to and from the store with the determined location name.
        /// If <see langword="null"/> it defaults to a new instance of the class <see cref="KeyFile"/>.
        /// Alternatively an implementation type can be registered in a common service locator compatible DI container.
        /// </param>
        public ProtectedKeyCipher(
            string symmetricAlgorithmName = null,
            string symmetricKeyLocation = null,
            IKeyLocationStrategy symmetricKeyLocationStrategy = null,
            IKeyStorageAsync keyStorage = null)
            : this(symmetricAlgorithmName)
        {
            ResolveKeyStorage(symmetricKeyLocation, symmetricKeyLocationStrategy, keyStorage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedKeyCipher"/> class for initialization by the constructors of the inheriting classes.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// Also a string instance with name &quot;DefaultSymmetricEncryption&quot; can be defined in a Common Service Locator compatible dependency injection container.
        /// </param>
        protected ProtectedKeyCipher(
            string symmetricAlgorithmName)
            : base(symmetricAlgorithmName)
        {
        }
        #endregion

        #region ICipher Members

        /// <summary>
        /// Gets or sets a value indicating whether the encrypted texts are or should be Base64 encoded.
        /// </summary>
        public virtual bool Base64Encoded { get; set; }

        /// <summary>
        /// Reads the clear text from the <paramref name="dataStream"/> encrypts it and writes the result into the <paramref name="encryptedStream"/> 
        /// stream. This is the reverse method of <see cref="M:Decrypt(System.Stream, System.Stream)"/>.
        /// </summary>
        /// <param name="dataStream">
        /// The unencrypted input stream.
        /// </param>
        /// <param name="encryptedStream">
        /// The output stream where to write the crypto package which will contain the encrypted data 
        /// as well as some other crypto artifacts, e.g. initialization vector.
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The CryptoStream will do it.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual void Encrypt(
            Stream dataStream,
            Stream encryptedStream)
        {
            // generate, read from store, etc.
            InitializeSymmetricKey();

            // if the output needs to be Base-64 encoded, wrap the encoded stream in a crypto stream applying Base64 transformation
            Stream outputStream = Base64Encoded
                                    ? new CryptoStream(encryptedStream, new ToBase64Transform(), CryptoStreamMode.Write)
                                    : encryptedStream;

            try
            {
                // store IV, etc.
                BeforeWriteEncrypted(outputStream);

                // wrap the output stream into a crypto-stream;
                // and copy the input stream into the crypto-stream;
                using (var cryptoStream = CreateEncryptingStream(outputStream))
                {
                    DoEncrypt(dataStream, cryptoStream);
                    // output hash, signature, etc.
                    AfterWriteEncrypted(encryptedStream, cryptoStream);
                }
            }
            finally
            {
                if (Base64Encoded)
                    outputStream.Dispose();
            }
        }

        /// <summary>
        /// Reads and decrypts the <paramref name="encryptedStream"/> stream and writes the clear text into the <paramref name="dataStream"/> stream.
        /// This is the reverse method of <see cref="M:Encrypt(System.Stream, System.Stream)"/>.
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The CryptoStream will do it.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual void Decrypt(
            Stream encryptedStream,
            Stream dataStream)
        {
            // generate, read from store, etc.
            InitializeSymmetricKey();

            // if the output needs to be Base-64 encoded, wrap the encoded stream in a crypto stream applying Base64 transformation
            Stream inputStream = Base64Encoded
                                    ? new CryptoStream(encryptedStream, new FromBase64Transform(), CryptoStreamMode.Read)
                                    : encryptedStream;

            try
            {
                // restore IV, etc.
                BeforeReadDecrypted(inputStream);

                // wrap the input stream with a crypto stream; 
                // and copy the crypto stream into the output stream
                using (var cryptoStream = CreateDecryptingStream(inputStream))
                {
                    DoDecrypt(cryptoStream, dataStream);
                    // output hash, signature, etc.
                    AfterReadDecrypted(cryptoStream, cryptoStream);
                }
            }
            catch (FormatException x)
            {
                if (!Base64Encoded)
                    throw;

                throw new ArgumentException("The input data does not represent a valid, Base64 encoded, crypto package.", x);
            }
            finally
            {
                if (Base64Encoded)
                    inputStream.Dispose();
            }
        }

        /// <summary>
        /// Encrypts the specified <paramref name="data"/>. This is the reverse method of <see cref="M:Decrypt(byte[])"/>.
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
        public virtual byte[] Encrypt(
            byte[] data)
        {
            if (data == null)
                return null;

            using (var dataStream = new MemoryStream(data))
            using (var encryptedStream = new MemoryStream())
            {
                Encrypt(dataStream, encryptedStream);
                encryptedStream.Close();
                return encryptedStream.ToArray();
            }
        }

        /// <summary>
        /// Decrypts the specified <paramref name="encryptedData"/>.
        /// This is the reverse method of <see cref="M:Encrypt(byte[])"/>.
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
        public virtual byte[] Decrypt(
            byte[] encryptedData)
        {
            if (encryptedData == null)
                return null;

            using (var encryptedStream = new MemoryStream(encryptedData))
            using (var dataStream = new MemoryStream())
            {
                Decrypt(encryptedStream, dataStream);
                dataStream.Close();
                return dataStream.ToArray();
            }
        }
        #endregion

        #region ICipherAsync Members
        /// <summary>
        /// Asynchronously reads the clear text from the <paramref name="dataStream"/>, encrypts it and writes the result into the 
        /// <paramref name="encryptedStream"/> stream. This is the reverse method of <see cref="M:DecryptAsync(System.Stream, System.Stream)"/>.
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual async Task EncryptAsync(
            Stream dataStream,
            Stream encryptedStream)
        {
            // generate, read from store, etc.
            await InitializeSymmetricKeyAsync();

            // store IV, etc.
            await BeforeWriteEncryptedAsync(encryptedStream);

            // wrap the output stream into a crypto-stream;
            // and copy the input stream into the crypto-stream;
            using (var cryptoStream = CreateEncryptingStream(encryptedStream))
            {
                await DoEncryptAsync(dataStream, cryptoStream);

                // output hash, signature, etc.
                AfterWriteEncrypted(encryptedStream, cryptoStream);
            }
        }

        /// <summary>
        /// Asynchronously reads and decrypts the <paramref name="encryptedStream"/> stream and writes the clear text into the 
        /// <paramref name="dataStream"/> stream. This is the reverse method of <see cref="M:EncryptAsync(System.Stream, System.Stream)"/>.
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual async Task DecryptAsync(
            Stream encryptedStream,
            Stream dataStream)
        {
            // generate, read from store, etc.
            await InitializeSymmetricKeyAsync();

            // store IV, etc.
            await BeforeReadDecryptedAsync(encryptedStream);

            // wrap the input stream with a crypto stream; 
            // and copy the crypto stream into the output stream
            using (var cryptoStream = CreateDecryptingStream(encryptedStream))
            {
                await DoDecryptAsync(cryptoStream, dataStream);

                // output hash, signature, etc.
                AfterReadDecrypted(cryptoStream, cryptoStream);
            }
        }
        #endregion

        #region Initialization of the symmetric key overrides
        /// <summary>
        /// Encrypts the symmetric key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <returns>System.Byte[].</returns>
        protected override byte[] EncryptSymmetricKey() => ProtectedData.Protect(Symmetric.Key, null, DataProtectionScope.LocalMachine);

        /// <summary>
        /// Decrypts the symmetric key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedKey">The encrypted key.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void DecryptSymmetricKey(
            byte[] encryptedKey)
        {
            Symmetric.Key = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.LocalMachine);
        }
        #endregion

        #region Encrypting primitives
        /// <summary>
        /// Allows the inheritors to write some unencrypted information to the <paramref name="encryptedStream"/>
        /// before the encrypted text, e.g. here the cipher writes the initialization vector.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream"/> cannot be written to.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void BeforeWriteEncrypted(
            Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, nameof(encryptedStream));
            Contract.Requires<ArgumentException>(encryptedStream.CanWrite, "The argument "+nameof(encryptedStream)+" cannot be written to.");
            Contract.Requires<InvalidOperationException>(IsSymmetricKeyInitialized, "The symmetric key must be initialized first.");

            Symmetric.GenerateIV();

            var encryptedIV = EncryptIV();

            // write the length and the contents of the IV in the encrypted stream
            encryptedStream.Write(BitConverter.GetBytes(encryptedIV.Length), 0, sizeof(int));
            encryptedStream.Write(encryptedIV, 0, encryptedIV.Length);
        }

        /// <summary>
        /// Encrypts the symmetric cipher's initialization vector.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <returns>System.Byte[].</returns>
        protected virtual byte[] EncryptIV()
        {
            Contract.Requires<InvalidOperationException>(IsSymmetricKeyInitialized, "The symmetric key must be initialized first.");

            return ShouldEncryptIV
                        ? ProtectedData.Protect(Symmetric.IV, null, DataProtectionScope.LocalMachine)
                        : Symmetric.IV;
        }

        /// <summary>
        /// Creates the encrypting stream and gives an opportunity to the inheritors to modify the creation of the crypto-stream, 
        /// e.g. wrap or chain the crypto-stream created here with another crypto-stream that computes a hash of the unencrypted document.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <returns>The created CryptoStream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream"/> cannot be written.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual CryptoStream CreateEncryptingStream(
            Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, nameof(encryptedStream));
            Contract.Requires<ArgumentException>(encryptedStream.CanWrite, "The argument "+nameof(encryptedStream)+" cannot be written to.");

            return new CryptoStream(
                            encryptedStream,
                            Symmetric.CreateEncryptor(),
                            CryptoStreamMode.Write);
        }

        /// <summary>
        /// Performs the actual encryption of the <paramref name="dataStream"/> and writing into the <paramref name="cryptoStream"/>.
        /// Also gives opportunity to the inheritors to modify the actual encryption process.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when either <paramref name="cryptoStream"/> or <paramref name="dataStream"/> are <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when either <paramref name="dataStream"/> cannot be read or the <paramref name="cryptoStream"/> cannot be written.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void DoEncrypt(
            Stream dataStream,
            Stream cryptoStream)
        {
            Contract.Requires<ArgumentNullException>(dataStream != null, nameof(dataStream));
            Contract.Requires<ArgumentException>(dataStream.CanRead, "The argument "+nameof(dataStream)+" cannot be read from.");

            Contract.Requires<ArgumentNullException>(cryptoStream != null, nameof(cryptoStream));
            Contract.Requires<ArgumentException>(cryptoStream.CanWrite, "The argument "+nameof(cryptoStream)+" cannot be written to.");

            dataStream.CopyTo(cryptoStream);
        }

        /// <summary>
        /// Gives an opportunity to the inheritors to write more unencrypted information to the <paramref name="encryptedStream"/>, e.g.
        /// add the signature or the hash to the <paramref name="encryptedStream"/>.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void AfterWriteEncrypted(
            Stream encryptedStream,
            CryptoStream cryptoStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, nameof(encryptedStream));
            Contract.Requires<ArgumentException>(encryptedStream.CanWrite, "The argument "+nameof(encryptedStream)+" cannot be written to.");

            Contract.Requires<ArgumentNullException>(cryptoStream != null, nameof(cryptoStream));
            Contract.Requires<ArgumentException>(cryptoStream.CanWrite, "The argument "+nameof(cryptoStream)+" cannot be written to.");
        }
        #endregion

        #region Decrypting primitives
        /// <summary>
        /// Allows the inheritors to read some unencrypted information from the <paramref name="encryptedStream"/>, 
        /// e.g. here the cipher reads and sets the initialization vector in the symmetric cipher.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream"/> cannot be read.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void BeforeReadDecrypted(
            Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, "encryptedStream");
            Contract.Requires<ArgumentException>(encryptedStream.CanRead, "The argument "+nameof(encryptedStream)+" cannot be read from.");
            Contract.Requires<InvalidOperationException>(IsSymmetricKeyInitialized, "The symmetric key must be initialized first.");

            // read the length of the IV and allocate an array for it
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            // read the length of the IV and allocate an array for it
            if (encryptedStream.Read(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the length of the IV.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            var encryptedIV = new byte[length];

            // read the encrypted IV from the memory stream
            if (encryptedStream.Read(encryptedIV, 0, encryptedIV.Length) != encryptedIV.Length)
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the IV.", nameof(encryptedStream));

            // decrypt the IV and set it in Symmetric
            DecryptIV(encryptedIV);
        }

        /// <summary>
        /// Decrypts the symmetric cipher's initialization vector.
        /// </summary>
        /// <param name="encryptedIV">The encrypted initialization vector.</param>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected virtual void DecryptIV(
            byte[] encryptedIV)
        {
            Contract.Requires<InvalidOperationException>(IsSymmetricKeyInitialized, "The symmetric key must be initialized first.");

            Symmetric.IV = ShouldEncryptIV
                                ? ProtectedData.Unprotect(encryptedIV, null, DataProtectionScope.LocalMachine)
                                : encryptedIV;
        }

        /// <summary>
        /// Creates the decrypting stream and gives an opportunity to the inheritors to modify the creation of the crypto-stream, 
        /// e.g. wrap or chain the crypto-stream created here with another crypto-stream that computes a hash of the unencrypted text.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <returns>The created CryptoStream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream"/> cannot be read.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual CryptoStream CreateDecryptingStream(
            Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, nameof(encryptedStream));
            Contract.Requires<ArgumentException>(encryptedStream.CanRead, "The argument "+nameof(encryptedStream)+" cannot be read from.");
            Contract.Requires<InvalidOperationException>(IsSymmetricKeyInitialized, "The symmetric key must be initialized first.");
            Contract.Ensures(Contract.Result<CryptoStream>() != null);

            return new CryptoStream(
                            encryptedStream,
                            Symmetric.CreateDecryptor(),
                            CryptoStreamMode.Read);
        }

        /// <summary>
        /// Performs the actual decryption of the crypto-text from the package and writes it into the <paramref name="dataStream"/>.
        /// Also gives opportunity to the inheritors to modify the actual decryption process.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <param name="dataStream">The data stream.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when either <paramref name="cryptoStream"/> or <paramref name="dataStream"/> are <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when either <paramref name="dataStream"/> cannot be written or the <paramref name="cryptoStream"/> cannot be read.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void DoDecrypt(
            Stream cryptoStream,
            Stream dataStream)
        {
            Contract.Requires<ArgumentNullException>(cryptoStream != null, nameof(cryptoStream));
            Contract.Requires<ArgumentException>(cryptoStream.CanRead, "The argument "+nameof(cryptoStream)+" cannot be read from.");

            Contract.Requires<ArgumentNullException>(dataStream != null, nameof(dataStream));
            Contract.Requires<ArgumentException>(dataStream.CanWrite, "The argument "+nameof(dataStream)+" cannot be written to.");

            cryptoStream.CopyTo(dataStream);
        }

        /// <summary>
        /// Gives an opportunity to the inheritors to read more unencrypted information from the <paramref name="encryptedStream"/> or perform other activities, e.g.
        /// verify the signature or the hash of the <paramref name="encryptedStream"/>.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void AfterReadDecrypted(
            Stream encryptedStream,
            CryptoStream cryptoStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, nameof(encryptedStream));
            Contract.Requires<ArgumentException>(encryptedStream.CanRead, "The argument "+nameof(encryptedStream)+" cannot be read from.");

            Contract.Requires<ArgumentNullException>(cryptoStream != null, "cryptoStream");
            Contract.Requires<ArgumentException>(cryptoStream.CanRead, "The argument "+nameof(cryptoStream)+" cannot be read from.");
        }
        #endregion

        #region Async encrypting primitives
        /// <summary>
        /// Allows the inheritors to write asynchronously some unencrypted information to the <paramref name="encryptedStream"/>
        /// before the encrypted text, e.g. here the cipher writes the initialization vector.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <returns>
        /// A <see cref="T:Task"/> object representing the process.
        /// </returns>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream"/> cannot be written to.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual async Task BeforeWriteEncryptedAsync(
            Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, "encryptedStream");
            Contract.Requires<ArgumentException>(encryptedStream.CanWrite, "The argument \"encryptedStream\" cannot be written to.");
            Contract.Requires<InvalidOperationException>(IsSymmetricKeyInitialized, "The symmetric key must be initialized first.");

            Symmetric.GenerateIV();

            var encryptedIV = EncryptIV();

            // write the length and the contents of the IV in the encrypted stream
            await encryptedStream.WriteAsync(BitConverter.GetBytes(encryptedIV.Length), 0, sizeof(int));
            await encryptedStream.WriteAsync(encryptedIV, 0, encryptedIV.Length);
        }

        /// <summary>
        /// Performs asynchronously the actual encryption of the <paramref name="dataStream"/> and writing into the <paramref name="cryptoStream"/>.
        /// Also gives opportunity to the inheritors to modify the actual encryption process.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when either <paramref name="cryptoStream"/> or <paramref name="dataStream"/> are <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when either <paramref name="dataStream"/> cannot be read or the <paramref name="cryptoStream"/> cannot be written.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual async Task DoEncryptAsync(
            Stream dataStream,
            Stream cryptoStream)
        {
            Contract.Requires<ArgumentNullException>(dataStream != null, "dataStream");
            Contract.Requires<ArgumentException>(dataStream.CanRead, "The argument \"dataStream\" cannot be read from.");

            Contract.Requires<ArgumentNullException>(cryptoStream != null, "cryptoStream");
            Contract.Requires<ArgumentException>(cryptoStream.CanWrite, "The argument \"cryptoStream\" cannot be written to.");

            await dataStream.CopyToAsync(cryptoStream);
        }
        #endregion

        #region Async decrypting primitives
        /// <summary>
        /// Allows the inheritors to read asynchronously some unencrypted information from the <paramref name="encryptedStream"/>, 
        /// e.g. here the cipher reads and sets the initialization vector in the symmetric cipher.
        /// </summary>
        /// <returns>A <see cref="Task"/> object.</returns>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream"/> cannot be read.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual async Task BeforeReadDecryptedAsync(
            Stream encryptedStream)
        {
            Contract.Requires<ArgumentNullException>(encryptedStream != null, nameof(encryptedStream));
            Contract.Requires<ArgumentException>(encryptedStream.CanRead, "The argument "+nameof(encryptedStream)+" cannot be read from.");
            Contract.Requires<InvalidOperationException>(IsSymmetricKeyInitialized, "The symmetric key must be initialized first.");

            // read the length of the key and allocate an array for it
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            // read the length of the IV and allocate an array for it
            if (await encryptedStream.ReadAsync(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the length of the IV.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            var encryptedIV = new byte[length];

            // read the encrypted IV from the memory stream
            if (await encryptedStream.ReadAsync(encryptedIV, 0, encryptedIV.Length) != encryptedIV.Length)
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the IV.", nameof(encryptedStream));

            // decrypt the IV and set it in Symmetric
            DecryptIV(encryptedIV);
        }

        /// <summary>
        /// Performs asynchronously the actual decryption of the crypto-text from the package and writes it into the <paramref name="dataStream"/>.
        /// Also gives opportunity to the inheritors to modify the actual decryption process.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <param name="dataStream">The data stream.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when either <paramref name="cryptoStream"/> or <paramref name="dataStream"/> are <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when either <paramref name="dataStream"/> cannot be written or the <paramref name="cryptoStream"/> cannot be read.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual async Task DoDecryptAsync(
            Stream cryptoStream,
            Stream dataStream)
        {
            Contract.Requires<ArgumentNullException>(cryptoStream != null, nameof(cryptoStream));
            Contract.Requires<ArgumentException>(cryptoStream.CanRead, "The argument "+nameof(cryptoStream)+" cannot be read from.");

            Contract.Requires<ArgumentNullException>(dataStream != null, nameof(dataStream));
            Contract.Requires<ArgumentException>(dataStream.CanWrite, "The argument "+nameof(dataStream)+" cannot be written to.");

            await cryptoStream.CopyToAsync(dataStream);
        }
        #endregion

        /// <summary>
        /// Copies certain characteristics of this instance to the <paramref name="cipher"/> parameter.
        /// The goal is to produce a cipher with the same encryption/decryption behavior but saving the key encryption and decryption ceremony and overhead if possible.
        /// </summary>
        /// <param name="cipher">The cipher that gets the identical symmetric algorithm object.</param>
        protected override void CopyTo(
            SymmetricKeyCipherBase cipher)
        {
            base.CopyTo(cipher);

            var c = cipher as ProtectedKeyCipher;

            if (c == null)
                return;

            c.Base64Encoded = Base64Encoded;
        }
    }
}
