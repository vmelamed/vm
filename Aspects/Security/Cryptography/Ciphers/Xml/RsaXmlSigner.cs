using System;
using System.Collections.Generic;
using System.Deployment.Internal.CodeSigning;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Xml;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// Class RsaSigner generates an encrypted hash (signature) of the protected data. 
    /// The signer supports only SHA1-RSA and SHA256-RSA.
    /// </summary>
    public class RsaXmlSigner : IXmlSigner
    {
        const int Sha1ProviderType   = 1;
        const int Sha256ProviderType = 24;
        const int DsaProviderType    = 13;

        const string XPathAllAttributes    = "//@{0}";
        const string XPathRootElement      = "/*";
        const string XmlSignatureLocalName = "Signature";
        const string XmlSignedIdFormat     = "signed-{0}";

        readonly string _hashAlgorithmName;
        readonly string _canonicalizationMethod;
        readonly string _signatureMethod;
        readonly string _digestMethod;
        readonly AsymmetricAlgorithm _asymmetric;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "N/A")]
        static RsaXmlSigner()
        {
            CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), XmlConstants.XmlDsigRSAPKCS1SHA256Url);
        }

        #region Properties
        /// <summary>
        /// Gets or sets the name of the hash algorithm.
        /// </summary>
        public string HashAlgorithmName
        {
            get
            {


                return _hashAlgorithmName;
            }
        }

        /// <summary>
        /// Gets or sets the implementation of the asymmetric algorithm.
        /// </summary>
        protected AsymmetricAlgorithm Asymmetric
        {
            get
            {

                return _asymmetric;
            }
        }

        /// <summary>
        /// Gets or sets a set of possible unique identifier attribute names. Default - { &quot;xml:id&quot;, &quot;Id&quot;, &quot;id&quot;, &quot;ID&quot; }.
        /// <para>
        /// Allows the caller to specify custom names of unique attributes to be searched for by the signer in order to list them in the references of the signature. 
        /// </para>
        /// </summary>
        public IEnumerable<string> IdAttributeNames { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedNewKeyCipher" /> class.
        /// </summary>
        /// <param name="hashAlgorithmName">
        /// The name of the hash algorithm implementation. Use any of the constants from <see cref="Algorithms.Hash"/> or
        /// <see langword="null"/>, empty or whitespace characters only - it will default to the default algorithm specified in the certificate.
        /// </param>
        /// <param name="signCertificate">
        /// The certificate containing the public and optionally the private key.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;SigningCertificate&quot;.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="signCertificate"/> is <see langword="null"/> and could not be resolved from the Common Service Locator.
        /// </exception>
        public RsaXmlSigner(
            X509Certificate2 signCertificate = null,
            string hashAlgorithmName = null)
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

            if (hashAlgorithmName == null)
                hashAlgorithmName = signCertificate.HashAlgorithm();

            _hashAlgorithmName = hashAlgorithmName;

            int providerType;

            switch (hashAlgorithmName)
            {
            case Algorithms.Hash.Sha256:
                _canonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
                _signatureMethod        = XmlConstants.XmlDsigRSAPKCS1SHA256Url;
                _digestMethod           = XmlConstants.Sha256DigestMethod;
                providerType            = Sha256ProviderType;
                break;

#pragma warning disable CS0612 // Type or member is obsolete - used for bacwards compatibility
            case Algorithms.Hash.Sha1:
#pragma warning restore CS0612 // Type or member is obsolete
                _canonicalizationMethod = SignedXml.XmlDsigCanonicalizationUrl;
                _signatureMethod        = SignedXml.XmlDsigRSASHA1Url;
                _digestMethod           = XmlConstants.Sha1DigestMethod;
                providerType            = Sha1ProviderType;
                break;

            default:
                throw new NotSupportedException("The signer does not support the hashing algorithm specified in the certificate.");
            }

            using (var key = signCertificate.HasPrivateKey
                                ? (RSACryptoServiceProvider)signCertificate.PrivateKey
                                : (RSACryptoServiceProvider)signCertificate.PublicKey.Key)
            {
                _asymmetric = new RSACryptoServiceProvider(new CspParameters(providerType));
                _asymmetric.FromXmlString(key.ToXmlString(signCertificate.HasPrivateKey));
            }
        }
        #endregion

        #region IXmlSigner Members

        SignatureLocation _signatureLocation;
        /// <summary>
        /// Gets or sets the signature type being created or verified. 
        /// The default is <see cref="SignatureLocation" /><c>.Enveloped</c>
        /// </summary>
        public SignatureLocation SignatureLocation
        {
            get { return _signatureLocation; }
            set
            {
                if (!Enum.IsDefined(typeof(SignatureLocation), value))
                    throw new ArgumentException(Resources.InvalidArgument, nameof(value));

                _signatureLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include key information in the signature.
        /// Including key information however would require the recipient to verify the key information with the expected used key, 
        /// otherwise a man-in-the-middle attack can replace the element contents and the signature along with the key information.
        /// Not including key information (the default) would require the recipient to know which key to use for signature verification.
        /// </summary>
        public bool IncludeKeyInfo { get; set; }

        /// <summary>
        /// Signs the XML <paramref name="document" /> or only the elements specified by the <paramref name="xmlPath" />.
        /// </summary>
        /// <param name="document">The document to be signed.</param>
        /// <param name="xmlPath">The XML path selecting the XML elements to be signed.
        /// Can be <see langword="null" />, empty or consisted of whitespace characters only (see the remarks below).
        /// The parameter is ignored if the property <see cref="SignatureLocation" /> is set to <see cref="SignatureLocation" /><c>.Enveloping</c>.</param>
        /// <param name="namespaceManager">The namespace manager which can resolve the namespace prefixes used in the XPath expression in <paramref name="xmlPath" />.
        /// Can be <see langword="null" /> if <paramref name="xmlPath" /> is <see langword="null" /> or no name-space prefixes are used.
        /// The parameter is ignored if the property <see cref="SignatureLocation" /> is set to <see cref="SignatureLocation" /><c>.Enveloping</c>.</param>
        /// <param name="documentLocation">Specifies the location (URI) of the document if the property <see cref="SignatureLocation" /> is set to
        /// <see cref="SignatureLocation" /><c>.Detached</c> and <paramref name="xmlPath" /> specifies the whole document (e.g. is <see langword="null" />);
        /// otherwise the parameter is ignored. Can be <see langword="null" /> in any case.</param>
        /// <returns>If the property <see cref="SignatureLocation" /> is set to:
        /// <list type="bullet"><item><see cref="SignatureLocation" /><c>.Enveloped</c>: the method returns the modified <paramref name="document" /> with added signature element.
        /// Also if the <paramref name="xmlPath" /> selects one or more descending elements they will be modified too by adding
        /// an extra attribute that would look like this: <c>xml:id="signed0"</c>.
        /// </item><item><see cref="SignatureLocation" /><c>.Enveloping</c>: the method returns an XmlDocument of the signature which includes the root element of the <paramref name="document" />;
        /// </item><item><see cref="SignatureLocation" /><c>.Detached</c>: the method returns an XmlDocument containing the signature with a reference to the document location.
        /// </item></list></returns>
        /// <exception cref="System.ArgumentNullException">document</exception>
        /// <remarks><para>
        /// If the parameter <paramref name="xmlPath" /> is <see langword="null" />, empty or consist of whitespace characters only,
        /// the whole document will be signed as if the parameter was set to <c>"/"</c>.
        /// </para>
        /// <para>
        /// If the property <see cref="SignatureLocation" /> is set to <see cref="SignatureLocation" /><c>.Enveloped</c> (the default) and the parameter
        /// <paramref name="xmlPath" /> specifies the document node (e.g. <see langword="null" />), a signature element will be appended next to the children of the root element.
        /// If <paramref name="xmlPath" /> selects one or more elements of the document, each one will be represented in the inserted signature by a <c>Reference</c> element
        /// (XPath <c>"/Signature/SignedInfo/Reference"</c>) with its corresponding URI (<c>"/Signature/SignedInfo/Reference/@URI"</c>) and
        /// digest (<c>"/Signature/SignedInfo/Reference/DigestValue"</c>). The URI will be in the form "#xxxxx" and will refer to the signed element's
        /// <c>xml:Id</c> attribute. If the element does not have one, the signer will create and add a new <c>Id</c> that will look like this: <c>xml:id="signed0"</c>.
        /// </para>
        /// <para></para>
        /// <para>
        /// If the property <see cref="SignatureLocation" /> is set to <see cref="SignatureLocation" /><c>.Enveloping</c> the parameter <paramref name="xmlPath" /> will be ignored,
        /// the root element of the document will be signed and inserted in the element of the generated XML signature at XPath <c>"/Signature/Object"</c>.
        /// </para>
        /// <para>
        /// If the property <see cref="SignatureLocation" /> is set to <see cref="SignatureLocation" /><c>.Detached</c> and the path specifies the whole document
        /// (e.g. <see langword="null" />) the URI of the reference element of the signature (at XPath <c>"/Signature/SignedInfo/Reference/@URI"</c>)
        /// will contain the value of the parameter <paramref name="documentLocation" /> if specified or "".
        /// If the <paramref name="xmlPath" /> selects node(s) other than the document itself, the <paramref name="documentLocation" /> will be ignored and the reference URI(s) will point
        /// to the signed elements in the form "#xxxxx". If the elements do not have attributes specifying unique identifiers of type <c>xml:Id</c> they will be added in the
        /// respective elements of the <paramref name="document" />.
        /// </para></remarks>
        public XmlDocument Sign(
            XmlDocument document,
            string xmlPath = null,
            XmlNamespaceManager namespaceManager = null,
            Uri documentLocation = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var signer = new SignedXmlWithId(document, IdAttributeNames) { SigningKey = Asymmetric };

            signer.SignedInfo.CanonicalizationMethod = _canonicalizationMethod;
            signer.SignedInfo.SignatureMethod        = _signatureMethod;

            if (IncludeKeyInfo)
            {
                signer.KeyInfo = new KeyInfo();
                signer.KeyInfo.AddClause(new RSAKeyValue((RSA)Asymmetric));
            }

            foreach (var reference in GetReferences(signer, document, xmlPath, namespaceManager, documentLocation))
                signer.AddReference(reference);

            signer.ComputeSignature();

            var xmlSignature = signer.GetXml();
            XmlDocument returnedDoc = null;
            XmlNode signatureParent = null;

            switch (SignatureLocation)
            {
            case SignatureLocation.Enveloped:
                returnedDoc     = document;
                signatureParent = returnedDoc.DocumentElement;
                break;

            case SignatureLocation.Enveloping:
            case SignatureLocation.Detached:
                returnedDoc     = new XmlDocument();
                signatureParent = returnedDoc;
                break;
            }

            signatureParent.AppendChild(
                returnedDoc.ImportNode(xmlSignature, true));

            return returnedDoc;
        }

        /// <summary>
        /// Tries to verify the signature(s) of the elements in the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="signature">Required parameter if the property <see cref="SignatureLocation" /> is set to <see cref="SignatureLocation" /><c>.Detached</c>; otherwise the parameter is ignored.</param>
        /// <returns><see langword="true" /> if the signature verification succeeds, otherwise <see langword="false" />.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// document
        /// or
        /// signature
        /// </exception>
        public bool TryVerifySignature(
            XmlDocument document,
            XmlDocument signature = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (signature == null && SignatureLocation == SignatureLocation.Detached)
                throw new ArgumentNullException(nameof(signature));

            var signedXml = new SignedXmlWithId(document, IdAttributeNames);

            if (SignatureLocation != SignatureLocation.Detached)
                signature = document;

            var signatureElement = signature.GetElementsByTagName(XmlSignatureLocalName)
                                            .OfType<XmlElement>()
                                            .FirstOrDefault();

            if (signatureElement == null)
                return false;

            signedXml.LoadXml(signatureElement);

            return signedXml.CheckSignature(Asymmetric);
        }

        /// <summary>
        /// Gets the references.
        /// </summary>
        /// <param name="signedXml">The signed XML.</param>
        /// <param name="document">The document.</param>
        /// <param name="xmlPath">The XML path.</param>
        /// <param name="namespaceManager">The namespace manager.</param>
        /// <param name="documentLocation">The document location.</param>
        /// <returns>IEnumerable&lt;Reference&gt;.</returns>
        IEnumerable<Reference> GetReferences(
            SignedXml signedXml,
            XmlDocument document,
            string xmlPath,
            XmlNamespaceManager namespaceManager,
            Uri documentLocation)
        {
            XmlNodeList elements = null;

            if (SignatureLocation == SignatureLocation.Enveloping)
                // ignore the path - it doesn't make sense - always sign the document's root element and import it into /Signature/Object
                xmlPath = XPathRootElement;

            if (xmlPath != null)
            {
                elements = document.SelectNodes(xmlPath, namespaceManager);

                if (elements.Count == 1  &&  elements[0] is XmlDocument)
                    // the path points to the document node
                    elements = null;
            }

            if (elements == null)
            {
                // sign the whole document
                var reference = new Reference();

                if (SignatureLocation == SignatureLocation.Enveloped)
                    reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());

                if (SignatureLocation == SignatureLocation.Detached  &&  documentLocation!=null  &&  !documentLocation.IsFile)
                    reference.Uri = documentLocation.ToString();
                else
                    reference.Uri = "";

                yield return reference;
            }
            else
            {
                // build a set of all possible unique ID attribute names:
                var nameIds = new HashSet<string> { "id", "Id", "ID" };     // these are always here

                if (IdAttributeNames != null)
                    nameIds.UnionWith(IdAttributeNames);    // add the custom ones
                else
                    nameIds.Add(XmlConstants.Id);           // add the XML standard one

                var nsManager = XmlConstants.GetXmlNamespaceManager(document);

                // a set of all unique id-s in the document will help us generate new unique xml:Id-s
                var xmlIds = new HashSet<string>();

                foreach (var name in nameIds)
                    xmlIds.UnionWith(
                        document.SelectNodes(
                                    string.Format(CultureInfo.InvariantCulture, XPathAllAttributes, name),
                                    nsManager)
                                .OfType<XmlAttribute>()
                                .Select(a => a.Value)
                                .Distinct());

                var id = 0;

                foreach (var element in elements.OfType<XmlElement>())
                {
                    string xmlId = null;

                    if (SignatureLocation == SignatureLocation.Enveloping)
                    {
                        // we need a new unique xml:Id for the Object element
                        xmlId = GetNewId(ref id, xmlIds);

                        // wrap the root element in a /Signature/Object element
                        signedXml.AddObject(
                            new DataObject
                            {
                                Data = elements,  // contains the root element only
                                Id   = xmlId,     // add the xml:Id to the object, so that we can refer to it from the reference object
                            });
                    }
                    else
                    {
                        // find a unique ID - any one of the set should do
                        foreach (var name in nameIds)
                        {
                            var attribute = element.SelectSingleNode("@"+name, nsManager) as XmlAttribute;

                            if (attribute!=null  &&  attribute.Value != null)
                            {
                                xmlId = attribute.Value;
                                break;
                            }
                        }

                        // if it doesn't have unique id, generate a new one and add it to the element, so that we can refer to it from the reference object
                        if (xmlId.IsNullOrWhiteSpace())
                        {
                            xmlId = GetNewId(ref id, xmlIds);

                            var attribute = document.CreateAttribute(XmlConstants.Prefix, XmlConstants.IdLocalName, XmlConstants.Namespace);

                            attribute.Value = xmlId;
                            element.Attributes.Append(attribute);
                        }
                    }

                    // create the reference object
                    var reference = new Reference("#"+xmlId);

                    reference.DigestMethod = _digestMethod;

                    switch (HashAlgorithmName)
                    {
                    case Algorithms.Hash.Sha256:
                        reference.AddTransform(new XmlDsigExcC14NTransform());
                        break;

                    case Algorithms.Hash.Sha1:
                        reference.AddTransform(new XmlDsigC14NTransform());
                        break;
                    }

                    if (SignatureLocation == SignatureLocation.Enveloped)
                        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());

                    yield return reference;
                }
            }
        }

        /// <summary>
        /// Gets a new identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="xmlIds">The XML ids.</param>
        /// <returns>System.String.</returns>
        static string GetNewId(
            ref int id,
            HashSet<string> xmlIds)
        {
            if (xmlIds == null)
                throw new ArgumentNullException(nameof(xmlIds));

            string xmlId;

            do
                xmlId = string.Format(CultureInfo.InvariantCulture, XmlSignedIdFormat, ++id);
            while (xmlIds.Contains(xmlId));

            xmlIds.Add(xmlId);

            return xmlId;
        }

        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(true)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~RsaXmlSigner"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing)
                _asymmetric.Dispose();
        }
        #endregion
    }
}
