using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Security.Cryptography.Ciphers.Test;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    [TestClass]
    public class RsaXmlSignerWithKeyInfoTest : GenericXmlSignerTest<RsaXmlSigner>
    {
        public override IXmlSigner GetSigner(SignatureLocation signatureLocation = SignatureLocation.Enveloped)
        {
            return new RsaXmlSigner(CertificateFactory.GetSigningCertificate()) { SignatureLocation = signatureLocation };
        }
    }
}
