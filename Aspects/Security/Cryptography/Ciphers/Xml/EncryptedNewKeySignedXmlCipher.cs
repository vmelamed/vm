using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

using CommonServiceLocator;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// Class EncryptedNewKeySignedXmlCipher encrypts an XML document or selected elements of it with a symmetric key encryption and a signature.
    /// The symmetric key is protected with asymmetric encryption into the crypto-package. Supports only SHA+RSA signing.
    /// </summary>
    public class EncryptedNewKeySignedXmlCipher : EncryptedNewKeyXmlCipher
    {
        readonly RsaXmlSigner _signer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedNewKeyCipher" /> class.
        /// </summary>
        /// <param name="exchangeCertificate">
        /// The certificate containing the public and optionally the private keys for encrypting the symmetric key. Cannot be <see langword="null"/>.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;EncryptingCertificate&quot;.
        /// </param>
        /// <param name="signCertificate">
        /// The certificate containing the public and optionally the private keys for encrypting the hash - signing. Cannot be <see langword="null"/>.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;SigningCertificate&quot;.
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or
        /// <see langword="null" />, empty or whitespace characters only - which will default to <see cref="Algorithms.Symmetric.Default" />.
        /// Also a string instance with name &quot;DefaultSymmetricEncryption&quot; can be defined in a Common Service Locator compatible dependency injection container.
        /// </param>
        /// <param name="hashAlgorithmName">
        /// The name of the hash algorithm. By default the cipher will pick the algorithm from the <paramref name="signCertificate"/> but the caller
        /// may choose to use lower length signature key, e.g. the certificate may be for SHA256 but the caller may override that to SHA1. The caller 
        /// cannot specify higher length then the one on the certificate.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="exchangeCertificate" /> or the <paramref name="signCertificate"/> is <see langword="null" /> 
        /// and could not be resolved from the Common Service Locator.
        /// </exception>
        /// <remarks>
        /// Note that for XML signing the cipher supports only SHA1 and SHA256.
        /// </remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Owned by this object, disposed in Dispose()")]
        public EncryptedNewKeySignedXmlCipher(
            X509Certificate2 exchangeCertificate = null,
            X509Certificate2 signCertificate = null,
            string symmetricAlgorithmName = null,
            string hashAlgorithmName = null)
            : base(exchangeCertificate, symmetricAlgorithmName)
        {
            if (signCertificate == null)
                try
                {
                    signCertificate = ServiceLocatorWrapper.Default.GetInstance<X509Certificate2>(Algorithms.Hash.CertificateResolveName);
                }
                catch (ActivationException x)
                {
                    throw new ArgumentNullException("The argument \"signCertificate\" was null and could not be resolved from the Common Service Locator.", x);
                }

            _signer = new RsaXmlSigner(signCertificate, hashAlgorithmName)
            {
                SignatureLocation = SignatureLocation.Enveloped,
            };
        }

        #region Xml signing
        /// <summary>
        /// Encrypts in-place the elements of an XML document, which are specified with an XPath expression.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="xmlPath">The XML path expression which selects the elements that must be encrypted.
        /// If the parameter is <see langword="null" />, empty or all whitespace characters, the root element of the document will be encrypted.
        /// </param>
        /// <param name="namespaceManager">
        /// The namespace manager for the used XPath namespace prefixes. Can be <see langword="null" /> if no name-spaces are used.
        /// </param>
        public override void Encrypt(
            XmlDocument document,
            string xmlPath = null,
            XmlNamespaceManager namespaceManager = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _signer.Sign(document, xmlPath, namespaceManager);
            base.Encrypt(document, xmlPath, namespaceManager);
        }

        /// <summary>
        /// Decrypts in-place all encrypted elements from an XML document.
        /// </summary>
        /// <param name="document">A document containing encrypted elements.</param>
        public override void Decrypt(XmlDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            base.Decrypt(document);
            _signer.TryVerifySignature(document);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
                _signer.Dispose();

            base.Dispose(disposing);
        }
        #endregion
    }
}
