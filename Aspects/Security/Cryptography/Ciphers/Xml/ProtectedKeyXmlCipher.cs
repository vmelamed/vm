using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// Class ProtectedKeyXmlCipher encrypts an XML document or selected elements of it with a symmetric key encryption.
    /// The symmetric key is protected with DPAPI into a file.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification="Nothing to dispose here.")]
    public class ProtectedKeyXmlCipher : SymmetricKeyCipherBase, IXmlCipher
    {
        readonly string _encryptionUri;

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
        /// </param>
        /// <param name="keyStorage">
        /// Object which implements the storing and retrieving of the the encrypted symmetric key to and from the store with the determined location name.
        /// If <see langword="null"/> it defaults to a new instance of the class <see cref="KeyFile"/>.
        /// </param>
        public ProtectedKeyXmlCipher(
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
        protected ProtectedKeyXmlCipher(
            string symmetricAlgorithmName)
            : base(symmetricAlgorithmName)
        {
            _encryptionUri = SymmetricXmlNamespace();
        }
        #endregion

        /// <summary>
        /// Encrypts the symmetric key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <returns>System.Byte[].</returns>
        protected override byte[] EncryptSymmetricKey()
        {
            return ProtectedData.Protect(Symmetric.Key, null, DataProtectionScope.LocalMachine);
        }

        /// <summary>
        /// Decrypts the symmetric key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedKey">The encrypted key.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected override void DecryptSymmetricKey(
            byte[] encryptedKey)
        {
            Symmetric.Key = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.LocalMachine);
        }

        #region IXmlCipher Members and implementation
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public virtual void Encrypt(
            XmlDocument document,
            string xmlPath = null,
            XmlNamespaceManager namespaceManager = null)
        {
            InitializeSymmetricKey();

            var encryptedXml = new EncryptedXml(document);

            if (string.IsNullOrWhiteSpace(xmlPath))
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public virtual void Decrypt(
            XmlDocument document)
        {
            InitializeSymmetricKey();

            foreach (XmlElement element in document.SelectNodes(XmlConstants.XPathEncryptedElements, XmlConstants.GetEncryptNamespaceManager(document)))
                DecryptElement(element);
        }
        #endregion

        #region IXmlCipher implementation

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
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId="System.Xml.XmlNode", Justification="We encrypt/decrypt XmlElement-s only.")]
        protected virtual void EncryptElement(
            XmlElement element,
            EncryptedXml encryptedXml)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (encryptedXml == null)
                throw new ArgumentNullException("encryptedXml");

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
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId="System.Xml.XmlNode", Justification="We encrypt/decrypt XmlElement-s only.")]
        protected virtual void DecryptElement(
            XmlElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

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
        #endregion
    }
}
