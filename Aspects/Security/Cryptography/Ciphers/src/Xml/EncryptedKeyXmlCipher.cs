﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// Class EncryptedKeyXmlCipher encrypts an XML document or selected elements of it with a symmetric key encryption.
    /// The symmetric key is protected with asymmetric encryption into a file.
    /// </summary>
    public class EncryptedKeyXmlCipher : SymmetricKeyCipherBase, IXmlCipher
    {
        readonly string _encryptionUri;

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
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or
        /// <see langword="null" />, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default" />.
        /// </param>
        /// <param name="symmetricKeyLocation">
        /// Seeding name of store location name of the encrypted symmetric key (e.g. relative or absolute path).
        /// Can be <see langword="null"/>, empty or whitespace characters only.
        /// The parameter will be passed to the <paramref name="symmetricKeyLocationStrategy"/> to determine the final store location name path (e.g. relative or absolute path).
        /// </param>
        /// <param name="symmetricKeyLocationStrategy">
        /// Object which implements the strategy for determining the store location name (e.g. path and filename) of the encrypted symmetric key.
        /// If <see langword="null"/> it defaults to a new instance of the class <see cref="DefaultServices.KeyFileLocationStrategy"/>.
        /// </param>
        /// <param name="keyStorage">
        /// Object which implements the storing and retrieving of the the encrypted symmetric key to and from the store with the determined location name.
        /// If <see langword="null"/> it defaults to a new instance of the class <see cref="DefaultServices.KeyFileStorage"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="certificate" /> is <see langword="null" />.
        /// </exception>
        public EncryptedKeyXmlCipher(
            X509Certificate2 certificate = null,
            string symmetricAlgorithmName = null,
            string symmetricKeyLocation = null,
            IKeyLocationStrategy symmetricKeyLocationStrategy = null,
            IKeyStorageTasks keyStorage = null)
            : this(symmetricAlgorithmName)
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
        /// </param>
        protected EncryptedKeyXmlCipher(
            string symmetricAlgorithmName)
            : base(symmetricAlgorithmName)
        {
            _encryptionUri = SymmetricXmlNamespace();
        }
        #endregion

        /// <summary>
        /// Determines the symmetric XML encryption method from the symmetric algorithm.
        /// </summary>
        /// <returns>EncryptionMethod.</returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The specified symmetric algorithm is not supported for XML encryption.</exception>
        protected string SymmetricXmlNamespace()
        {

            if (Symmetric is TripleDES)
                return EncryptedXml.XmlEncTripleDESUrl;

            if (Symmetric is DES)
                return EncryptedXml.XmlEncDESUrl;

            if (Symmetric is Aes || Symmetric is Rijndael)
                switch (Symmetric.KeySize)
                {
                    case 128:
                        return EncryptedXml.XmlEncAES128Url;
                    case 192:
                        return EncryptedXml.XmlEncAES192Url;
                    case 256:
                        return EncryptedXml.XmlEncAES256Url;
                }

            throw new CryptographicException("The specified symmetric algorithm is not supported for XML encryption.");
        }

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
                throw new ArgumentNullException(nameof(certificate));

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
        protected override byte[] EncryptSymmetricKey()
            => PublicKey.Encrypt(Symmetric.Key, true);

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
            if (PrivateKey == null)
                throw new InvalidOperationException("The certificate did not contain a private key.");

            Symmetric.Key = PrivateKey.Decrypt(encryptedKey, true);
        }
        #endregion

        #region IXmlCipher Members
        /// <summary>
        /// Gets or sets a value indicating whether to encrypt the content of the elements only or the entire elements, incl. the elements' names and attributes.
        /// </summary>
        public bool ContentOnly { get; set; }

        /// <summary>
        /// Encrypts in-place the elements of an XML document, which are specified with an XPath expression.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="xmlPath">
        /// The XML path expression which selects the elements that must be encrypted.
        /// If the parameter is <see langword="null" />, empty or all whitespace characters, the root element of the document will be encrypted.
        /// </param>
        /// <param name="namespaceManager">
        /// The namespace manager for the used XPath namespace prefixes. Can be <see langword="null"/> if no name-spaces are used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:System.Xml.XmlPathException">
        /// The specified XML path is invalid.
        /// </exception>
        /// <exception cref="CryptographicException">
        /// The specified symmetric algorithm is not supported. Only the TripleDES, DES, AES-128, AES-192 and AES-256 algorithms are supported.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual void Encrypt(
            XmlDocument document,
            string xmlPath = null,
            XmlNamespaceManager namespaceManager = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            InitializeSymmetricKey();

            var encryptedXml = new EncryptedXml(document);

            if (xmlPath.IsNullOrWhiteSpace())
                EncryptElement(document.DocumentElement, encryptedXml);
            else
                foreach (XmlElement element in document.SelectNodes(xmlPath, namespaceManager).OfType<XmlElement>())
                    EncryptElement(element, encryptedXml);
        }

        /// <summary>
        /// Decrypts in-place all encrypted elements from an XML document.
        /// </summary>
        /// <param name="document">A document containing encrypted elements.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual void Decrypt(
            XmlDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            InitializeSymmetricKey();

            foreach (XmlElement element in document.SelectNodes(XmlConstants.XPathEncryptedElements, XmlConstants.GetEncryptNamespaceManager(document)))
                DecryptElement(element);
        }
        #endregion

        #region IXmlCipher implementation helpers
        /// <summary>
        /// Encrypts the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="encryptedXml">The encrypted XML.</param>
        /// <exception cref="System.ArgumentNullException">
        /// element
        /// or
        /// encryptedXml
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "We encrypt/decrypt XmlElement-s only.")]
        protected virtual void EncryptElement(
            XmlElement element,
            EncryptedXml encryptedXml)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (encryptedXml == null)
                throw new ArgumentNullException(nameof(encryptedXml));

            var encryptedElement = new EncryptedData
            {
                Type             = EncryptedXml.XmlEncElementUrl,
                EncryptionMethod = new EncryptionMethod(_encryptionUri),
            };
            encryptedElement.CipherData.CipherValue = encryptedXml.EncryptData(element, Symmetric, ContentOnly);

            var encryptedKey = CreateEncryptedKey();

            if (encryptedKey != null)
            {
                encryptedElement.KeyInfo = new KeyInfo();
                encryptedElement.KeyInfo.AddClause(
                    new KeyInfoEncryptedKey(encryptedKey));
            }

            EncryptedXml.ReplaceElement(
                            element,
                            encryptedElement,
                            ContentOnly);
        }

        /// <summary>
        /// Decrypts the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <exception cref="System.ArgumentNullException">element</exception>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "We encrypt/decrypt XmlElement-s only.")]
        protected virtual void DecryptElement(
            XmlElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var encryptedElement = new EncryptedData();

            encryptedElement.LoadXml(element);

            var encryptedXml = new EncryptedXml();

            encryptedXml.ReplaceData(
                            element,
                            encryptedXml.DecryptData(encryptedElement, Symmetric));
        }

        /// <summary>
        /// Creates the encrypted key.
        /// </summary>
        /// <returns>EncryptedKey.</returns>
        protected virtual EncryptedKey CreateEncryptedKey()
        {
            return null;
        }
        #endregion
    }
}
