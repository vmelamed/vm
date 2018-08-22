using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Security.Cryptography.Ciphers.Tests;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\Xml\\TestOrder.xml")]
    public class RsaXmlSignerWithKeyInfoTest : GenericXmlSignerTest<RsaXmlSigner>
    {
        public override IXmlSigner GetSigner(SignatureLocation signatureLocation = SignatureLocation.Enveloped) => new RsaXmlSigner(CertificateFactory.GetSigningCertificate()) { SignatureLocation = signatureLocation };
    }
}
