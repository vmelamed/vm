using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Xml;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// The <c>enum SignatureLocation</c> specifies the where the created signature will be located.
    /// </summary>
    public enum SignatureLocation
    {
        /// <summary>
        /// The signature is contained within the XML document being signed.
        /// </summary>
        Enveloped,

        /// <summary>
        /// The signed XML is contained within the &lt;Signature&gt; element at XPath <c>&quot;/Signature/Object&quot;</c>.
        /// </summary>
        Enveloping,

        /// <summary>
        /// The signature is in a separate document from the data being signed.
        /// </summary>
        Detached,
    }

    /// <summary>
    /// The interface <c>IXmlSigner</c> defines the behavior of objects for signing and signature verification of XML elements.
    /// </summary>
    [ContractClass(typeof(IXmlSignerContract))]
    public interface IXmlSigner : IDisposable
    {
        /// <summary>
        /// Gets or sets the signature type being created or verified. The default is <see cref="SignatureLocation"/><c>.Enveloped</c>
        /// </summary>
        SignatureLocation SignatureLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include key information in the signature.
        /// Including key information however would require the recipient to verify the key information with the expected used key, 
        /// otherwise a man-in-the-middle attack can replace the element contents and the signature along with the key information.
        /// Not including key information (the default) would require the recipient to know which key to use for signature verification.
        /// </summary>
        bool IncludeKeyInfo { get; set; }

        /// <summary>
        /// Signs the XML <paramref name="document"/> or only the elements specified by the <paramref name="xmlPath"/>.
        /// </summary>
        /// <param name="document">
        /// The document to be signed.
        /// </param>
        /// <param name="xmlPath">
        /// The XML path selecting the XML elements to be signed.
        /// Can be <see langword="null" />, empty or consisted of whitespace characters only (see the remarks below).
        /// The parameter is ignored if the property <see cref="SignatureLocation"/> is set to <see cref="SignatureLocation"/><c>.Enveloping</c>.
        /// </param>
        /// <param name="namespaceManager">
        /// The namespace manager which can resolve the namespace prefixes used in the XPath expression in <paramref name="xmlPath"/>.
        /// Can be <see langword="null"/> if <paramref name="xmlPath"/> is <see langword="null"/> or no name-space prefixes are used.
        /// The parameter is ignored if the property <see cref="SignatureLocation"/> is set to <see cref="SignatureLocation"/><c>.Enveloping</c>.
        /// </param>
        /// <param name="documentLocation">
        /// Specifies the location (URI) of the document if the property <see cref="SignatureLocation"/> is set to 
        /// <see cref="SignatureLocation"/><c>.Detached</c> and <paramref name="xmlPath"/> specifies the whole document (e.g. is <see langword="null"/>); 
        /// otherwise the parameter is ignored. Can be <see langword="null"/> in any case.
        /// </param>
        /// <returns>
        /// If the property <see cref="SignatureLocation"/> is set to:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="SignatureLocation"/><c>.Enveloped</c>: the method returns the modified <paramref name="document"/> with added signature element.
        ///             Also if the <paramref name="xmlPath"/> selects one or more descending elements and they do not attributes which unique identify the elements,
        ///             they will be modified too by adding an extra attribute that would look like this: <c>xml:id=&quot;signed0&quot;</c>.
        ///         </item>
        ///         <item>
        ///             <see cref="SignatureLocation"/><c>.Enveloping</c>: the method returns an XmlDocument of the signature which includes the root element of the <paramref name="document"/>;
        ///         </item>
        ///         <item>
        ///             <see cref="SignatureLocation"/><c>.Detached</c>: the method returns an XmlDocument containing the signature with a reference to the document location.
        ///         </item>
        ///     </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// If the parameter <paramref name="xmlPath"/> is <see langword="null" />, empty or consist of whitespace characters only,
        /// the whole document will be signed as if the parameter was set to <c>&quot;/&quot;</c>.
        /// </para><para>
        /// If the property <see cref="SignatureLocation"/> is set to <see cref="SignatureLocation"/><c>.Enveloped</c> (the default) and the parameter 
        /// <paramref name="xmlPath"/> specifies the document node (e.g. <see langword="null"/>), a signature element will be appended next to the children of the root element.
        /// If <paramref name="xmlPath"/> selects one or more elements of the document, each one will be represented in the inserted signature by a <c>Reference</c> element 
        /// (XPath <c>&quot;/Signature/SignedInfo/Reference&quot;</c>) with its corresponding URI (<c>&quot;/Signature/SignedInfo/Reference/@URI&quot;</c>) and 
        /// digest (<c>&quot;/Signature/SignedInfo/Reference/DigestValue&quot;</c>). The URI will be in the form &quot;#xxxxx&quot; and will refer to the signed element's 
        /// <c>xml:Id</c> attribute. If the element does not have one, the signer will create and add a new <c>Id</c> that will look like this: <c>xml:id=&quot;signed0&quot;</c>.
        /// </para><para>
        /// </para><para>
        /// If the property <see cref="SignatureLocation"/> is set to <see cref="SignatureLocation"/><c>.Enveloping</c> the parameter <paramref name="xmlPath"/> will be ignored,
        /// the root element of the document will be signed and inserted in the element of the generated XML signature at XPath <c>&quot;/Signature/Object&quot;</c>.
        /// </para><para>
        /// If the property <see cref="SignatureLocation"/> is set to <see cref="SignatureLocation"/><c>.Detached</c> and the path specifies the whole document 
        /// (e.g. <see langword="null"/>) the URI of the reference element of the signature (at XPath <c>&quot;/Signature/SignedInfo/Reference/@URI&quot;</c>) 
        /// will contain the value of the parameter <paramref name="documentLocation"/> if specified or &quot;&quot;. 
        /// If the <paramref name="xmlPath"/> selects node(s) other than the document itself, the <paramref name="documentLocation"/> will be ignored and the reference URI(s) will point
        /// to the signed elements in the form &quot;#xxxxx&quot;. If the elements do not have attributes specifying unique identifiers of type <c>xml:Id</c> they will be added in the 
        /// respective elements of the <paramref name="document"/>.
        /// </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "We need here the whole document.")]
        XmlDocument Sign(
            XmlDocument document,
            string xmlPath = null,
            XmlNamespaceManager namespaceManager = null,
            Uri documentLocation = null);

        /// <summary>
        /// Tries to verify the signature(s) of the elements in the specified document.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <param name="signature">
        /// Required parameter if the property <see cref="SignatureLocation"/> is set to <see cref="SignatureLocation"/><c>.Detached</c>; otherwise the parameter is ignored.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the signature verification succeeds, otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if
        ///     <list type="bullet">
        ///         <item>
        ///             <paramref name="document"/> is <see langword="null"/> or
        ///         </item>
        ///         <item>
        ///             <paramref name="signature"/> is <see langword="null"/> and 
        ///             the property <see cref="SignatureLocation"/> is set to <see cref="SignatureLocation"/><c>.Detached</c>.
        ///         </item>
        ///     </list>
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "We need here the whole document.")]
        bool TryVerifySignature(
            XmlDocument document,
            XmlDocument signature = null);
    }

    [ContractClassFor(typeof(IXmlSigner))]
    abstract class IXmlSignerContract : IXmlSigner
    {
        #region IXmlSigner Members

        public SignatureLocation SignatureLocation
        {
            get
            {
                Contract.Ensures(Enum.IsDefined(typeof(SignatureLocation), Contract.Result<SignatureLocation>()), "The value of the property is not a valid SignatureLocation value.");
                throw new NotImplementedException();
            }
            set
            {
                Contract.Requires<ArgumentException>(Enum.IsDefined(typeof(SignatureLocation), value), "The value is not a valid SignatureLocation value.");
                throw new NotImplementedException();
            }
        }

        public bool IncludeKeyInfo
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

        public XmlDocument Sign(
            XmlDocument document,
            string xmlPath = null,
            XmlNamespaceManager namespaceManager = null,
            Uri documentLocation = null)
        {
            Contract.Requires<ArgumentNullException>(document != null, "document");
            throw new NotImplementedException();
        }

        public bool TryVerifySignature(
            XmlDocument document,
            XmlDocument signature = null)
        {
            Contract.Requires<ArgumentNullException>(document != null, "document");
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
