using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    class SignedXmlWithId : SignedXml
    {
        const string xmlIdXPathFormat = "//*[@{0}=\"{1}\"]";

        static readonly string[] idAttributeDefaultNames = 
        {
            XmlConstants.Id,
        };

        IEnumerable<string> _idAttributeNames;

        public SignedXmlWithId(
            XmlDocument document,
            IEnumerable<string> idAttributeNames = null)
            : base(document)
        {
            SetIdAttributeNames(idAttributeNames);
        }

        void SetIdAttributeNames(
            IEnumerable<string> idAttributeNames)
        {
            if (idAttributeNames == null)
                _idAttributeNames = idAttributeDefaultNames;
            else
                _idAttributeNames = idAttributeNames;
        }

        /// <summary>
        /// Returns the <see cref="T:System.Xml.XmlElement" /> object with the specified ID from the specified <see cref="T:System.Xml.XmlDocument" /> object.
        /// </summary>
        /// <param name="document">The <see cref="T:System.Xml.XmlDocument" /> object to retrieve the <see cref="T:System.Xml.XmlElement" /> object from.</param>
        /// <param name="idValue">The ID of the <see cref="T:System.Xml.XmlElement" /> object to retrieve from the <see cref="T:System.Xml.XmlDocument" /> object.</param>
        /// <returns>The <see cref="T:System.Xml.XmlElement" /> object with the specified ID from the specified <see cref="T:System.Xml.XmlDocument" /> object, or null if it could not be found.</returns>
        public override XmlElement GetIdElement(
            XmlDocument document,
            string idValue)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (string.IsNullOrWhiteSpace(idValue))
                throw new ArgumentNullException("idValue");

            var idElement = base.GetIdElement(document, idValue);

            if (idElement != null)
                return idElement;

            var nsManager = XmlConstants.GetXmlNamespaceManager(document);

            foreach (var idName in _idAttributeNames)
            {
                var element = document.SelectSingleNode(
                                        string.Format(CultureInfo.InvariantCulture, xmlIdXPathFormat, idName, idValue),
                                        nsManager) as XmlElement;

                if (element != null)
                    return element;
            }

            return null;
        }
    }
}
