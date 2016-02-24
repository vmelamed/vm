using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Xml;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// The interface <c>IXmlCipher</c> defines the behavior of cipher objects for
    /// encryption and decryption of XML elements.
    /// </summary>
    [ContractClass(typeof(IXmlCipherContract))]
    public interface IXmlCipher : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether to encrypt the content of the elements only or the entire elements, 
        /// incl. the elements' names and attributes.
        /// </summary>
        bool ContentOnly { get; set; }

        /// <summary>
        /// Encrypts in-place the elements of an XML document, which are specified with an XPath expression.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="xmlPath">
        /// The XML path expression selecting the elements that must be encrypted.
        /// If the parameter is <see langword="null" />, empty or all whitespace characters, the root element of the document will be encrypted.
        /// </param>
        /// <param name="namespaceManager">
        /// The namespace manager for the used XPath namespace prefixes. Can be <see langword="null"/> if no name-spaces are used.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:System.Xml.XmlPathException">
        /// The specified XML path is invalid.
        /// </exception>
        /// <exception cref="T:System.Security.CryptographicException">
        /// The specified symmetric algorithm is not supported. Only the TripleDES, DES, AES-128, AES-192 and AES-256 algorithms are supported.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "We need here the whole document.")]
        void Encrypt(XmlDocument document, string xmlPath = null, XmlNamespaceManager namespaceManager = null);

        /// <summary>
        /// Decrypts in-place all encrypted elements from an XML document.
        /// </summary>
        /// <param name="document">A document containing encrypted elements.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "We need here the whole document.")]
        void Decrypt(XmlDocument document);
    }

    [ContractClassFor(typeof(IXmlCipher))]
    abstract class IXmlCipherContract : IXmlCipher
    {
        #region IXmlCipher Members

        public bool ContentOnly
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Encrypt(
            XmlDocument document,
            string xmlPath = null,
            XmlNamespaceManager namespaceManager = null)
        {
            Contract.Requires<ArgumentNullException>(document != null, nameof(document));
            throw new NotImplementedException();
        }

        public void Decrypt(
            XmlDocument document)
        {
            Contract.Requires<ArgumentNullException>(document != null, nameof(document));
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
