using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Security.Cryptography.Ciphers.Tests;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    [TestClass]
    public class RsaXmlSignerSha256_1Test : GenericXmlSignerTest<RsaXmlSigner>
    {
        public override IXmlSigner GetSigner(SignatureLocation signatureLocation = SignatureLocation.Enveloped)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new RsaXmlSigner(CertificateFactory.GetSigningSha256Certificate(), Algorithms.Hash.Sha1) { SignatureLocation = signatureLocation };     // SHA1 also works with this cert
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
