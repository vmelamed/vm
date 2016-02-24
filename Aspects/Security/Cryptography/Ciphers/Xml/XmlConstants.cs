using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    static class XmlConstants
    {
        /// <summary>
        /// The string value marking the section containing the encrypted symmetric key.
        /// </summary>
        public const string SymmetricKeyInfoName = "symmetric-key";
        /// <summary>
        /// The standard W3 XML encryption namespace - http://www.w3.org/2001/04/xmlenc#
        /// </summary>
        public const string XmlEncryptNamespace = "http://www.w3.org/2001/04/xmlenc#";
        /// <summary>
        /// The XML encryption prefix used here - xmlenc
        /// </summary>
        public const string XmlEncryptPrefix = "xmlenc";
        /// <summary>
        /// The XPath expression for "anywhere" - //
        /// </summary>
        public const string XPathAnywhere = "//";
        /// <summary>
        /// The XPath expression that selects the encrypted elements - //xmlenc:EncryptedData
        /// </summary>
        public const string XPathEncryptedElements = XPathAnywhere+XmlEncryptPrefix+":EncryptedData";
        /// <summary>
        /// The reserved namespace - http://www.w3.org/XML/1998/namespace
        /// </summary>
        public const string Namespace = "http://www.w3.org/XML/1998/namespace";
        /// <summary>
        /// The reserved prefix - xml
        /// </summary>
        public const string Prefix = "xml";
        /// <summary>
        /// The local name for unique identifier - Id
        /// </summary>
        public const string IdLocalName = "Id";
        /// <summary>
        /// The prefixed name for unique identifier - xml:Id
        /// </summary>
        public const string Id = Prefix+":"+IdLocalName;

        public const string XmlDsigRSAPKCS1SHA256Url = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
        public const string Sha1DigestMethod         = "http://www.w3.org/2000/09/xmldsig#sha1";
        public const string Sha256DigestMethod       = "http://www.w3.org/2001/04/xmlenc#sha256";

        public static XmlNamespaceManager GetEncryptNamespaceManager(
            XmlDocument document)
        {
            Contract.Requires<ArgumentNullException>(document!=null, nameof(document));
            Contract.Ensures(Contract.Result<XmlNamespaceManager>() != null, "Could not create namespace manager.");

            var namespaceManager = new XmlNamespaceManager(document.NameTable);

            namespaceManager.AddNamespace(XmlEncryptPrefix, XmlEncryptNamespace);

            return namespaceManager;
        }

        public static XmlNamespaceManager GetXmlNamespaceManager(
            XmlDocument document)
        {
            Contract.Requires<ArgumentNullException>(document!=null, nameof(document));
            Contract.Ensures(Contract.Result<XmlNamespaceManager>() != null, "Could not create namespace manager.");

            var namespaceManager = new XmlNamespaceManager(document.NameTable);

            namespaceManager.AddNamespace(Prefix, Namespace);

            return namespaceManager;
        }
    }
}
