using System;
using System.Diagnostics.Contracts;
using System.Xml;
using vm.Aspects.Security.Cryptography.Ciphers.Xml;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts.Xml
{
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
