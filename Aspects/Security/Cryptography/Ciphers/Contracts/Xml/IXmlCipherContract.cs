using System;
using System.Diagnostics.Contracts;
using System.Xml;
using vm.Aspects.Security.Cryptography.Ciphers.Xml;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts.Xml
{
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
            Contract.Requires<ArgumentNullException>(document != null, "document");
            throw new NotImplementedException();
        }

        public void Decrypt(
            XmlDocument document)
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
