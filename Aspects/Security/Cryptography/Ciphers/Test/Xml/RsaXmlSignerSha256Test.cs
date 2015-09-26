using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Security.Cryptography.Ciphers.Tests;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
#if NET45
    [TestClass]
    public class RsaXmlSignerSha256Test : GenericXmlSignerTest<RsaXmlSigner>
    {
        public override IXmlSigner GetSigner(SignatureLocation signatureLocation = SignatureLocation.Enveloped)
        {
            return new RsaXmlSigner(CertificateFactory.GetSigningSha256Certificate()) { SignatureLocation = signatureLocation };     // SHA1 also works with this cert
        }
    } 
#endif
}
