using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Security.Cryptography.Ciphers.Tests;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    [TestClass]
    public class RsaXmlSignerSha256Test : GenericXmlSignerTest<RsaXmlSigner>
    {
        public override IXmlSigner GetSigner(SignatureLocation signatureLocation = SignatureLocation.Enveloped) => new RsaXmlSigner(CertificateFactory.GetSigningSha256Certificate())
                                                                                                                    { SignatureLocation = signatureLocation };     // SHA1 also works with this cert
    }
}
