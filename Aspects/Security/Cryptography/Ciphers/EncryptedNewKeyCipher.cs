using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class <c>EncryptedNewKeyCipher</c> is a symmetric cipher. A symmetric key is generated for each document and saved encrypted inside the crypto-package.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default the cipher uses the <see cref="T:System.Security.Cryptography.AesCryptoServiceProvider"/> with default parameters. 
    /// </para><para>
    /// The cipher can be used for protecting data in motion where the sender has access to the public key and
    /// the receiver owns the private key. It can be used also to encrypt data at rest, e.g. stored in a database, file, etc.
    /// </para><para>
    /// The asymmetric key can be stored in a:
    /// <list type="bullet">
    ///     <item>key container, in which case the class will locate the key container using an implementation of <see cref="IKeyLocationStrategy"/></item>
    ///     <item>or the sender of the crypto-package can use a public key stored in a public certificate which corresponds to 
    ///           a private certificate owned by the receiver. The private certificate stores the private key too.</item>
    /// </list>
    /// </para><para>
    /// The encrypted symmetric key is stored inside the crypto-package.
    /// </para><para>
    /// Note that this cipher generates a new symmetric key for each package and performs slower than the <see cref="ProtectedKeyCipher"/>.
    /// Which cipher to use is a matter of compromise. <see cref="ProtectedKeyCipher"/> 
    /// encrypts and stores or retrieves and decrypts a single symmetric key once per the life time of the cipher and uses it for each protected document.
    /// However the file containing the encrypted symmetric key needs to be managed separately and if compromised - all encrypted documents will be compromised too.
    /// </para><para>
    /// Since the key does not need management - the key is stored encrypted in the crypto package - it disables the inherited members of <see cref="IKeyManagement"/> by returning
    /// constant trivial values (<see langword="null"/> or <see langword="false"/>).
    /// </para><para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item><description>Length of the encrypted symmetric key (serialized Int32) - 4 bytes.</description></item>
    ///         <item><description>The bytes of the encrypted symmetric key.</description></item>
    ///         <item><description>Length of the encrypted symmetric initialization vector (serialized Int32) - 4 bytes.</description></item>
    ///         <item><description>The bytes of the encrypted initialization vector.</description></item>
    ///         <item><description>The bytes of the encrypted text.</description></item>
    ///     </list>
    /// </para><para>
    /// The cipher can also be used to encrypt elements of or entire XML documents.
    /// </para>
    /// </remarks>
    public class EncryptedNewKeyCipher : EncryptedKeyCipher
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedNewKeyCipher" /> class.
        /// </summary>
        /// <param name="certificate">
        /// The certificate containing the public and optionally the private encryption keys. Cannot be <see langword="null"/>.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;EncryptingCertificate&quot;.
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// Also a string instance with name &quot;DefaultSymmetricEncryption&quot; can be defined in a Common Service Locator compatible dependency injection container.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="certificate"/> is <see langword="null"/> and could not be resolved from the Common Service Locator.
        /// </exception>
        public EncryptedNewKeyCipher(
            X509Certificate2 certificate = null,
            string symmetricAlgorithmName = null)
            : base(symmetricAlgorithmName, certificate)
        {
            // we do not need symmetric key storage - the key is stored in the crypto-package.
            //InitializeKeyStorage(symmetricKeyLocation, symmetricKeyLocationStrategy, keyStorage);
        }
        #endregion

        #region Disable the inherited IKeyManagement - the keys are stored in the crypto-package.
        /// <summary>
        /// Gets the determined store location name (e.g. path and filename).
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override string KeyLocation
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Imports the clear text of a symmetric key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void ImportSymmetricKey(
            byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Exports the clear text of the symmetric key.
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public override byte[] ExportSymmetricKey()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously imports the symmetric key as a clear text.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override Task ImportSymmetricKeyAsync(
            byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously exports the symmetric key as a clear text.
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public override Task<byte[]> ExportSymmetricKeyAsync()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Initialization of the symmetric key overrides
        /// <summary>
        /// Initializes the symmetric key by either reading it from the storage with the specified key location name or by
        /// generating a new key and saving it in it.
        /// </summary>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected override void InitializeSymmetricKey()
        {
            Symmetric.GenerateKey();
            IsSymmetricKeyInitialized = true;
        }

        /// <summary>
        /// Initializes asynchronously the symmetric key by either reading it from the storage with the specified key location name or by
        /// generating a new key and saving it in it.
        /// </summary>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected override Task InitializeSymmetricKeyAsync()
        {
            InitializeSymmetricKey();
            return Task.FromResult<object>(null);
        }
        #endregion

        #region Encrypting primitives
        /// <summary>
        /// Allows the inheritors to write some unencrypted information to the
        /// <paramref name="encryptedStream" />
        /// before the encrypted text, e.g. here the cipher writes encrypted key's length and the encrypted key itself.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream" /> cannot be written to.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void BeforeWriteEncrypted(
            Stream encryptedStream)
        {
            var encryptedKey = EncryptSymmetricKey();

            encryptedStream.Write(BitConverter.GetBytes(encryptedKey.Length), 0, sizeof(int));
            encryptedStream.Write(encryptedKey, 0, encryptedKey.Length);
            base.BeforeWriteEncrypted(encryptedStream);
        }
        #endregion

        #region Decrypting primitives
        /// <summary>
        /// Allows the inheritors to read some unencrypted information from the
        /// <paramref name="encryptedStream" />,
        /// e.g. here the cipher reads, decrypts and sets the the symmetric key.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when
        /// <list type="bullet">
        ///     <item>The input stream <paramref name="encryptedStream"/> cannot be read; or</item>
        ///     <item>The input stream <paramref name="encryptedStream"/> does not represent valid encrypted data; or</item>
        /// </list>
        /// </exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void BeforeReadDecrypted(Stream encryptedStream)
        {
            // read the length of the key and allocate an array for it
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            if (encryptedStream.Read(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the length of the key.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            var encryptedKey = new byte[length];

            // read the encrypted key, decrypt it and set it in Symmetric
            if (encryptedStream.Read(encryptedKey, 0, encryptedKey.Length) != encryptedKey.Length)
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the key.", nameof(encryptedStream));

            DecryptSymmetricKey(encryptedKey);

            base.BeforeReadDecrypted(encryptedStream);
        }
        #endregion

        #region Async encrypting primitives
        /// <summary>
        /// Allows the inheritors to write asynchronously some unencrypted information to the
        /// <paramref name="encryptedStream" />
        /// before the encrypted text, e.g. here the cipher writes encrypted key's length and the encrypted key itself.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <returns>
        /// A <see cref="T:Task"/> object representing the process.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream" /> cannot be written to.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override async Task BeforeWriteEncryptedAsync(
            Stream encryptedStream)
        {
            var encryptedKey = EncryptSymmetricKey();

            await encryptedStream.WriteAsync(BitConverter.GetBytes(encryptedKey.Length), 0, sizeof(int));
            await encryptedStream.WriteAsync(encryptedKey, 0, encryptedKey.Length);
            await base.BeforeWriteEncryptedAsync(encryptedStream);
        }
        #endregion

        #region Async decrypting primitives
        /// <summary>
        /// Allows the inheritors to read asynchronously some unencrypted information from the
        /// <paramref name="encryptedStream" />,
        /// e.g. here the cipher reads, decrypts and sets the the symmetric key.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when
        /// <list type="bullet">
        ///     <item>The input stream <paramref name="encryptedStream"/> cannot be read; or</item>
        ///     <item>The input stream <paramref name="encryptedStream"/> does not represent valid encrypted data; or</item>
        /// </list>
        /// </exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override async Task BeforeReadDecryptedAsync(
            Stream encryptedStream)
        {
            // read the length of the key and allocate an array for it
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            if (await encryptedStream.ReadAsync(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the length of the key.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            var encryptedKey = new byte[length];

            // read the encrypted key, decrypt it and set it in Symmetric
            if (await encryptedStream.ReadAsync(encryptedKey, 0, encryptedKey.Length) != encryptedKey.Length)
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the key.", nameof(encryptedStream));

            DecryptSymmetricKey(encryptedKey);

            await base.BeforeReadDecryptedAsync(encryptedStream);
        }
        #endregion

        #region ILightCipher
        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override ICipherAsync CloneLightCipher()
        {
            throw new NotSupportedException("The method Duplicate() is not supported on this cipher.");
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override ICipherAsync ReleaseCertificate()
        {
            throw new NotSupportedException("The method ResetAsymmetricKeys() is not supported on this cipher.");
        }
        #endregion
    }
}
