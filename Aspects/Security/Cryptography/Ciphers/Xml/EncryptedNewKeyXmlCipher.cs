using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Xml;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// Class EncryptedNewKeyXmlCipher encrypts an XML document or selected elements of it with a symmetric key encryption.
    /// The symmetric key is protected with asymmetric encryption into the crypto-package.
    /// </summary>
    public class EncryptedNewKeyXmlCipher : EncryptedKeyXmlCipher
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedNewKeyXmlCipher" /> class.
        /// </summary>
        /// <param name="certificate">
        /// The certificate containing the public and optionally the private encryption keys.
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="certificate"/> is <see langword="null"/>.
        /// </exception>
        public EncryptedNewKeyXmlCipher(
            X509Certificate2 certificate = null,
            string symmetricAlgorithmName = null)
            : base(symmetricAlgorithmName, certificate)
        {
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

        #region Overrides of the crypto-operations called by the GoF template-method
        /// <summary>
        /// Initializes the symmetric key by either reading it from the storage with the specified key location name or by
        /// generating a new key and saving it in it.
        /// </summary>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected override void InitializeSymmetricKey()
        {
            Symmetric.GenerateKey();
            Symmetric.GenerateIV();
            IsSymmetricKeyInitialized = true;
        }
        #endregion

        #region IXmlCipher implementation
        /// <summary>
        /// Encrypts in-place the elements of an XML document, which are specified with an XPath expression.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="xmlPath">The XML path expression which selects the elements that must be encrypted.
        /// If the parameter is <see langword="null" />, empty or all whitespace characters, the root element of the document will be encrypted.</param>
        /// <param name="namespaceManager">The namespace manager for the used XPath namespace prefixes. Can be <see langword="null" /> if no name-spaces are used.</param>
        public override void Encrypt(
            XmlDocument document,
            string xmlPath = null, XmlNamespaceManager namespaceManager = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            base.Encrypt(document, xmlPath, namespaceManager);
        }

        /// <summary>
        /// Decrypts in-place all encrypted elements from an XML document.
        /// </summary>
        /// <param name="document">A document containing encrypted elements.</param>
        /// <exception cref="System.ArgumentNullException">document</exception>
        public override void Decrypt(
            XmlDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (PrivateKey == null)
                throw new InvalidOperationException("The certificate did not contain a private key.");

            var encryptedXml = new EncryptedXml(document);

            encryptedXml.AddKeyNameMapping(XmlConstants.SymmetricKeyInfoName, PrivateKey);
            encryptedXml.DecryptDocument();
        }

        /// <summary>
        /// Creates the encrypted key.
        /// </summary>
        /// <returns>EncryptedKey.</returns>
        protected override EncryptedKey CreateEncryptedKey()
        {
            var encryptedKey = new EncryptedKey
            {
                CipherData       = new CipherData(EncryptedXml.EncryptKey(Symmetric.Key, PublicKey, false)),
                EncryptionMethod = CreateAsymmetricXmlEncryptionMethod(),
            };

            encryptedKey.KeyInfo.AddClause(new KeyInfoName(XmlConstants.SymmetricKeyInfoName));
            return encryptedKey;
        }

        /// <summary>
        /// Creates an object representing the asymmetric XML encryption method.
        /// </summary>
        /// <returns>EncryptionMethod.</returns>
        protected virtual EncryptionMethod CreateAsymmetricXmlEncryptionMethod()
            => new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);
        #endregion
    }
}
