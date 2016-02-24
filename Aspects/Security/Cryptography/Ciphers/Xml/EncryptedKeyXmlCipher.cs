using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// Class EncryptedKeyXmlCipher encrypts an XML document or selected elements of it with a symmetric key encryption.
    /// The symmetric key is protected with asymmetric encryption into a file.
    /// </summary>
    public class EncryptedKeyXmlCipher : ProtectedKeyXmlCipher
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
        /// Initializes a new instance of the <see cref="EncryptedNewKeyXmlCipher" /> class.
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
        public EncryptedKeyXmlCipher(
            X509Certificate2 certificate = null,
            string symmetricAlgorithmName = null,
            string symmetricKeyLocation = null,
            IKeyLocationStrategy symmetricKeyLocationStrategy = null,
            IKeyStorageAsync keyStorage = null)
            : this(symmetricAlgorithmName, certificate)
        {
            ResolveKeyStorage(symmetricKeyLocation, symmetricKeyLocationStrategy, keyStorage);
            InitializeAsymmetricKeys(certificate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedKeyXmlCipher" /> class for initialization by the constructors of the inheriting classes.
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
        protected EncryptedKeyXmlCipher(
            string symmetricAlgorithmName,
            X509Certificate2 certificate)
            : base(symmetricAlgorithmName)
        {
            InitializeAsymmetricKeys(certificate);
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
                    throw new ArgumentNullException("The parameter "+nameof(certificate)+" was null and could not be resolved from the Common Service Locator.", x);
                }

            PublicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;

            if (certificate.HasPrivateKey)
                PrivateKey = (RSACryptoServiceProvider)certificate.PrivateKey;
        }

        #region Overrides of the crypto-operations called by the GoF template-methods
        /// <summary>
        /// Encrypts the symmetric key using the public key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <returns>System.Byte[].</returns>
        protected override byte[] EncryptSymmetricKey() => PublicKey.Encrypt(Symmetric.Key, true);

        /// <summary>
        /// Decrypts the symmetric key using the private key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedKey">The encrypted key.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected override void DecryptSymmetricKey(
            byte[] encryptedKey)
        {
            if (PrivateKey == null)
                throw new InvalidOperationException("The certificate did not contain a private key.");

            Symmetric.Key = PrivateKey.Decrypt(encryptedKey, true);
        }
        #endregion

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(PublicKey != null, "The public key cannot be null.");
        }
    }
}
