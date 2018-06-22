using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CommonServiceLocator;
using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The <c>EncryptedKeyCipher</c> is a symmetric cipher. The symmetric key is encrypted with an asymmetric algorithm and so saved in a file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default the cipher uses the <see cref="AesCryptoServiceProvider"/> with default parameters. 
    /// </para><para>
    /// The cipher can be used for protecting data in motion where the sender has access to the public key and
    /// the receiver owns the private key. It can be used also to encrypt data at rest, e.g. stored in a database, file, etc. 
    /// where the writer owns the private key and the reader has access to the public key.
    /// </para><para>
    /// The asymmetric key can be stored in a:
    /// <list type="bullet">
    ///     <item>key container, in which case the class will locate the key container using an implementation of <see cref="IKeyLocationStrategy"/></item>
    ///     <item>or the sender of the crypto-package can use a public key stored in a public certificate which corresponds to 
    ///           a private certificate owned by the receiver. The private certificate stores the private key too.</item>
    /// </list>
    /// </para><para>
    /// The encrypted symmetric key is stored in a file. The class determines the path and name of the file from an <see cref="IKeyLocationStrategy"/> object.
    /// If the key file does not exist a new key is generated, encrypted and saved in a file in the location determined by the <see cref="IKeyLocationStrategy"/> object.
    /// </para><para>
    /// Note that this cipher uses one symmetric key for all packages.
    /// It performs faster than the <see cref="ProtectedKeyCipher"/>.
    /// Which cipher to use is a matter of compromise. <see cref="ProtectedKeyCipher"/> 
    /// encrypts and stores or retrieves and decrypts a single symmetric key once per the life time of the cipher.
    /// However the file containing the encrypted symmetric key needs to be managed separately and if compromised all encrypted documents will be compromised too.
    /// </para><para>
    /// Crypto package contents:
    /// <list type="number">
    ///     <item><description>The length of the initialization vector of the encrypted symmetric cipher (serialized Int32) - 4 bytes.</description></item>
    ///     <item><description>The bytes of the encrypted initialization vector.</description></item>
    ///     <item><description>The bytes of the encrypted text.</description></item>
    /// </list>
    /// </para><para>
    /// The cipher can also be used to encrypt elements of or entire XML documents.
    /// </para>
    /// </remarks>
    public class EncryptedKeyCipher : ProtectedKeyCipher, ILightCipher
    {
        #region Protected properties
        /// <summary>
        /// Gets or sets the public key used for encrypting the symmetric key.
        /// </summary>
        /// <value>The public key.</value>
        protected RSACryptoServiceProvider PublicKey { get; set; }
        /// <summary>
        /// Gets or sets the private key used for decrypting the symmetric key.
        /// </summary>
        /// <value>The private key.</value>
        protected RSACryptoServiceProvider PrivateKey { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedNewKeyCipher" /> class.
        /// </summary>
        /// <param name="certificate">
        /// The certificate containing the public and optionally the private key for encryption and decryption of the symmetric key.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;EncryptingCertificate&quot;.
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or
        /// <see langword="null" />, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default" />.
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
        /// </param>
        /// <param name="keyStorage">
        /// Object which implements the storing and retrieving of the the encrypted symmetric key to and from the store with the determined location name.
        /// If <see langword="null"/> it defaults to a new instance of the class <see cref="KeyFile"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="certificate" /> is <see langword="null" /> and could not be resolved from the Common Service Locator.
        /// </exception>
        public EncryptedKeyCipher(
            X509Certificate2 certificate = null,
            string symmetricAlgorithmName = null,
            string symmetricKeyLocation = null,
            IKeyLocationStrategy symmetricKeyLocationStrategy = null,
            IKeyStorageAsync keyStorage = null)
            : this(symmetricAlgorithmName, certificate)
        {
            ResolveKeyStorage(symmetricKeyLocation, symmetricKeyLocationStrategy, keyStorage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedKeyCipher" /> class for initialization by the constructors of the inheriting classes.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or
        /// <see langword="null" />, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default" />.
        /// Also a string instance with name "DefaultSymmetricEncryption" can be defined in a Common Service Locator compatible dependency injection container.
        /// </param>
        /// <param name="certificate">
        /// The certificate containing the public and optionally the private key for encryption and decryption of the symmetric key.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;EncryptingCertificate&quot;.
        /// </param>
        protected EncryptedKeyCipher(
            string symmetricAlgorithmName,
            X509Certificate2 certificate)
            : base(symmetricAlgorithmName)
        {
            InitializeAsymmetricKeys(certificate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedKeyCipher"/> class.
        /// </summary>
        /// <param name="symmetricKeyAlgorithm">The symmetric key algorithm.</param>
        protected EncryptedKeyCipher(
            string symmetricKeyAlgorithm)
            : base(symmetricKeyAlgorithm)
        {
        }
        #endregion

        /// <summary>
        /// Initializes the asymmetric key.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="certificate"/> is <see langword="null"/>
        /// </exception>.
        protected void InitializeAsymmetricKeys(
            X509Certificate2 certificate)
        {
            if (certificate == null)
                try
                {
                    certificate = ServiceLocatorWrapper.Default.GetInstance<X509Certificate2>(Algorithms.Symmetric.CertificateResolveName);
                }
                catch (ActivationException x)
                {
                    throw new ArgumentNullException("The argument \"certificate\" was null and could not be resolved from the Common Service Locator.", x);
                }

            PublicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;

            if (certificate.HasPrivateKey)
                PrivateKey = (RSACryptoServiceProvider)certificate.PrivateKey;
        }

        /// <summary>
        /// Flag indicating whether to encrypt the initialization vector. Here the setter checks if the certificate has a private key.
        /// </summary>
        public override bool ShouldEncryptIV
        {
            get { return base.ShouldEncryptIV; }
            set
            {
                if (value == true  &&  PrivateKey == null)
                    throw new InvalidOperationException("The certificate does not contain a private key.");
                base.ShouldEncryptIV = value;
            }
        }

        /// <summary>
        /// Encrypts the symmetric key using the public key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <returns>System.Byte[].</returns>
        protected override byte[] EncryptSymmetricKey()
        {
            if (PublicKey == null)
                throw new InvalidOperationException("The method is not available on light clones.");

            return PublicKey.Encrypt(Symmetric.Key, true);
        }

        /// <summary>
        /// Decrypts the symmetric key using the private key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedKey">The encrypted key.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void DecryptSymmetricKey(
            byte[] encryptedKey)
        {
            if (encryptedKey == null)
                throw new ArgumentNullException(nameof(encryptedKey));
            if (encryptedKey.Length == 0)
                throw new ArgumentException(Resources.InvalidArgument, nameof(encryptedKey));
            if (PublicKey == null)
                throw new InvalidOperationException("The method is not available on light clones.");
            if (PrivateKey == null)
                throw new InvalidOperationException("The certificate does not contain a private key.");

            Symmetric.Key = PrivateKey.Decrypt(encryptedKey, true);
            IsSymmetricKeyInitialized = true;
        }

        #region Encrypting/decrypting primitives
        /// <summary>
        /// Encrypts the symmetric cipher's initialization vector.
        /// </summary>
        /// <returns>The encrypted initialization vector.</returns>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected override byte[] EncryptIV()
        {
            if (!IsSymmetricKeyInitialized)
                throw new InvalidOperationException(Resources.UninitializedSymmetricKey);
            if (ShouldEncryptIV  &&  PublicKey == null)
                throw new InvalidOperationException("The method is not available on light clones.");

            return ShouldEncryptIV
                        ? PublicKey.Encrypt(Symmetric.IV, true)
                        : Symmetric.IV;
        }

        /// <summary>
        /// Decrypts the symmetric cipher's initialization vector.
        /// </summary>
        /// <param name="encryptedIV">The encrypted initialization vector.</param>
        /// <exception cref="System.InvalidOperationException">The certificate did not contain a private key.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        /// <exception cref="InvalidOperationException"></exception>
        /// See also <seealso cref="CloneLightCipher"/>.
        protected override void DecryptIV(
            byte[] encryptedIV)
        {
            if (!IsSymmetricKeyInitialized)
                throw new InvalidOperationException(Resources.UninitializedSymmetricKey);
            if (ShouldEncryptIV  &&  PrivateKey == null)
                throw new InvalidOperationException(PublicKey == null
                                                        ? "The method is not available on light clones."
                                                        : "The certificate does not contain a private key.");

            Symmetric.IV = ShouldEncryptIV
                                ? PrivateKey.Decrypt(encryptedIV, true)
                                : encryptedIV;
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~SymmetricCipher"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose"/>, it will try to release all managed resources 
        /// (usually aggregated objects which implement <see cref="IDisposable"/> as well) and then it will release all unmanaged resources if any.
        /// If the parameter is <c>false</c> then the method will only try to release the unmanaged resources.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (PrivateKey != null)
                    PrivateKey.Dispose();

                if (PublicKey != null)
                    PublicKey.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region ILightCipher
        /// <summary>
        /// Releases the asymmetric keys. By doing so the instance looses its <see cref="IKeyManagement" /> behavior but the memory footprint becomes much lighter.
        /// The asymmetric keys can be dropped only if the underlying symmetric algorithm instance is already initialized and
        /// the property <see cref="SymmetricKeyCipherBase.ShouldEncryptIV" /> is <see langword="false" />.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the underlying symmetric algorithm instance is not initialized yet or the property <see cref="SymmetricKeyCipherBase.ShouldEncryptIV" /> is <see langword="false" />.
        /// </exception>
        /// See also <seealso cref="CloneLightCipher"/>.
        public virtual ICipherAsync ReleaseCertificate()
        {

            if (ShouldEncryptIV)
                throw new InvalidOperationException("This object cannot reset its asymmetric keys because the property"+nameof(ShouldEncryptIV)+" is true.");

            if (PublicKey == null)
                return this;

            InitializeSymmetricKey();

            PublicKey.Dispose();
            PublicKey = null;

            PrivateKey?.Dispose();
            PrivateKey = null;

            KeyStorage = null;

            return this;
        }

        /// <summary>
        /// Creates a new, lightweight <see cref="EncryptedKeyCipher"/> instance and copies certain characteristics of this instance to it.
        /// A duplicate can be created only if the underlying symmetric algorithm instance is already initialized and the property <see cref="SymmetricKeyCipherBase.ShouldEncryptIV"/> is <see langword="false"/>.
        /// The duplicate can be used only for encryption and decryption of data (the <see cref="ICipher"/> and <see cref="ICipherAsync"/> behavior). The <see cref="IKeyManagement"/> behavior is disabled and
        /// calling any of its members would throw <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <returns>The duplicate.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the underlying symmetric algorithm instance is not initialized yet or the property <see cref="SymmetricKeyCipherBase.ShouldEncryptIV" /> is <see langword="false" />.
        /// </exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The caller must dispose it.")]
        public virtual ICipherAsync CloneLightCipher()
        {

            if (ShouldEncryptIV)
                throw new InvalidOperationException("This object cannot create light clones because the property "+nameof(ShouldEncryptIV)+" is true.");

            InitializeSymmetricKey();

            var cipher = new EncryptedKeyCipher(Symmetric.GetType().FullName);

            CopyTo(cipher);
            cipher.KeyStorage = null;

            return cipher;
        }
        #endregion
    }
}
