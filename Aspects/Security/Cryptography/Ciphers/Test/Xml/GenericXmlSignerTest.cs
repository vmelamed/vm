using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    public abstract class GenericXmlSignerTest<THasher> where THasher : IXmlSigner
    {
        public virtual IXmlSigner GetSigner(
            SignatureLocation signatureLocation = SignatureLocation.Enveloped)
        {
            Contract.Ensures(Contract.Result<IXmlSigner>() != null);

            throw new NotImplementedException();
        }

        public TestContext TestContext { get; set; }

        XmlDocument LoadXml(string xmlDocument)
        {
            var doc = new XmlDocument();

            doc.LoadXml(xmlDocument);

            return doc;
        }

        void DumpXml(XmlDocument document)
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (var x = XmlWriter.Create(w, new XmlWriterSettings() { Indent = true, NamespaceHandling = NamespaceHandling.OmitDuplicates }))
                    document.Save(x);

                TestContext.WriteLine("\n{0}", w.GetStringBuilder());
            }
        }

        XmlNamespaceManager GetNamespaceManager(XmlDocument document)
        {
            var nsMgr = new XmlNamespaceManager(document.NameTable);

            nsMgr.AddNamespace("pi", "urn:test:paymentInfo");
            nsMgr.AddNamespace("sign", "http://www.w3.org/2000/09/xmldsig#");
            nsMgr.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");

            return nsMgr;
        }

        [TestMethod]
        public void SignOrderEnvelopedTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloped))
            {
                var document = LoadXml(TestData.XmlOrder);

                var signed = target.Sign(document);

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order").Count);
            }
        }

        [TestMethod]
        public void SignOrder2EnvelopedTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloped))
            {
                var document = LoadXml(TestData.XmlOrder2);

                var signed = target.Sign(document, "/order/billing");

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(2, signed.SelectNodes("/order/billing/@*", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(2, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(2, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference/@URI", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignOrderWithIdsEnvelopedTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloped))
            {
                var document = LoadXml(TestData.XmlOrderWithIds);

                var signed = target.Sign(document, "/order/billing");

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(3, signed.SelectNodes("/order/billing/@*", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(3, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(3, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference/@URI", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing[@xml:Id=\"id1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#id1\"]", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing[@xml:Id=\"signed-1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-1\"]", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing[@xml:Id=\"signed-2\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-2\"]", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignNsOrderWithIdsEnvelopedTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloped))
            {
                var document = LoadXml(TestData.XmlNsOrderWithIds);

                var signed = target.Sign(document, "/order/billing/pi:paymentInfo", GetNamespaceManager(document));

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(6, signed.SelectNodes("/order/billing/pi:paymentInfo/@*", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(3, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(3, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference/@URI", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing/pi:paymentInfo[@xml:Id=\"id1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#id1\"]", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing/pi:paymentInfo[@xml:Id=\"signed-1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-1\"]", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing/pi:paymentInfo[@xml:Id=\"signed-2\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-2\"]", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignNsOrderWithCustomIdsEnvelopedTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloped))
            {
                ((RsaXmlSigner)target).IdAttributeNames = new string[] { "xml:Id", "customId" };

                var document = LoadXml(TestData.XmlNsOrderWithCustomIds);

                var signed = target.Sign(document, "/order/billing/pi:paymentInfo", GetNamespaceManager(document));

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(6, signed.SelectNodes("/order/billing/pi:paymentInfo/@*", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(3, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(3, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference/@URI", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing/pi:paymentInfo[@customId=\"id1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#id1\"]", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing/pi:paymentInfo[@xml:Id=\"signed-1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-1\"]", GetNamespaceManager(signed)).Count);

                Assert.AreEqual(1, signed.SelectNodes("/order/billing/pi:paymentInfo[@xml:Id=\"signed-2\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/order/sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-2\"]", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignOrderEnvelopingTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloping))
            {
                var document = LoadXml(TestData.XmlOrder);

                var signed = target.Sign(document);

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:Object/order", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignOrder2EnvelopingTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloping))
            {
                var document = LoadXml(TestData.XmlOrder2);

                var signed = target.Sign(document, "/order/billing");

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:Object/order", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:Object/@*", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:SignedInfo/sign:Reference", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:SignedInfo/sign:Reference/@URI", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignOrderWithIdsEnvelopingTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloping))
            {
                var document = LoadXml(TestData.XmlOrderWithIds);

                var signed = target.Sign(document, "/order/billing");

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:Object/order", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:Object/@*", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:SignedInfo/sign:Reference", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:SignedInfo/sign:Reference/@URI", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignNsOrderWithIdsEnvelopingTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloping))
            {
                var document = LoadXml(TestData.XmlOrderWithIds);

                var signed = target.Sign(document, "/order/billing/pi:paymentInfo");

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:Object/order", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:Object/@*", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:SignedInfo/sign:Reference", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature/sign:SignedInfo/sign:Reference/@URI", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignOrderDetachedTest()
        {
            using (var target = GetSigner(SignatureLocation.Detached))
            {
                var document = LoadXml(TestData.XmlOrder);

                var signed = target.Sign(document);

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(0, signed.SelectNodes("//order").Count);
            }
        }

        [TestMethod]
        [DeploymentItem("..\\..\\Xml\\TestOrder.xml")]
        public void SignOrderDetachedFileTest()
        {
            using (var target = GetSigner(SignatureLocation.Detached))
            {
                var document = new XmlDocument();

                document.Load("TestOrder.xml");

                var signed = target.Sign(document, null, null, new Uri("file://"+Path.GetFullPath("TestOrder.xml")));

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(0, signed.SelectNodes("//order").Count);
            }
        }

        [TestMethod]
        public void SignOrder2DetachedTest()
        {
            using (var target = GetSigner(SignatureLocation.Detached))
            {
                var document = LoadXml(TestData.XmlOrder2);

                var signed = target.Sign(document, "/order/billing");

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(0, signed.SelectNodes("//order").Count);
            }
        }

        [TestMethod]
        public void SignOrderWithIdsDetachedTest()
        {
            using (var target = GetSigner(SignatureLocation.Detached))
            {
                var document = LoadXml(TestData.XmlOrderWithIds);

                var signed = target.Sign(document, "/order/billing");

                DumpXml(document);

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(0, signed.SelectNodes("//order").Count);


                Assert.AreEqual(1, document.SelectNodes("/order/billing[@xml:Id=\"id1\"]", GetNamespaceManager(document)).Count);
                Assert.AreEqual(1, document.SelectNodes("/order/billing[@xml:Id=\"signed-1\"]", GetNamespaceManager(document)).Count);
                Assert.AreEqual(1, document.SelectNodes("/order/billing[@xml:Id=\"signed-2\"]", GetNamespaceManager(document)).Count);


                Assert.AreEqual(1, signed.SelectNodes("sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#id1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-2\"]", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignNsOrderWithIdsDetachedTest()
        {
            using (var target = GetSigner(SignatureLocation.Detached))
            {
                var document = LoadXml(TestData.XmlNsOrderWithIds);

                var signed = target.Sign(document, "/order/billing/pi:paymentInfo", GetNamespaceManager(document));

                DumpXml(document);

                DumpXml(signed);

                Assert.IsNotNull(signed);
                Assert.AreEqual(1, signed.SelectNodes("/sign:Signature", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(0, signed.SelectNodes("//order").Count);


                Assert.AreEqual(1, document.SelectNodes("/order/billing/pi:paymentInfo[@xml:Id=\"id1\"]", GetNamespaceManager(document)).Count);
                Assert.AreEqual(1, document.SelectNodes("/order/billing/pi:paymentInfo[@xml:Id=\"signed-1\"]", GetNamespaceManager(document)).Count);
                Assert.AreEqual(1, document.SelectNodes("/order/billing/pi:paymentInfo[@xml:Id=\"signed-2\"]", GetNamespaceManager(document)).Count);


                Assert.AreEqual(1, signed.SelectNodes("sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#id1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-1\"]", GetNamespaceManager(signed)).Count);
                Assert.AreEqual(1, signed.SelectNodes("sign:Signature/sign:SignedInfo/sign:Reference[@URI=\"#signed-2\"]", GetNamespaceManager(signed)).Count);
            }
        }

        [TestMethod]
        public void SignAndVerifyOrderEnvelopedTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloped))
            {
                var document = LoadXml(TestData.XmlOrder);

                var signed = target.Sign(document);

                DumpXml(signed);

                using (var target1 = GetSigner(SignatureLocation.Enveloped))
                    Assert.IsTrue(target1.TryVerifySignature(signed));
            }
        }

        [TestMethod]
        public void SignAndVerifyOrderWholeDocumentEnvelopedTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloped))
            {
                var document = LoadXml(TestData.XmlOrder);

                var signed = target.Sign(document, "/");

                DumpXml(signed);

                using (var target1 = GetSigner(SignatureLocation.Enveloped))
                    Assert.IsTrue(target1.TryVerifySignature(signed));
            }
        }

        [TestMethod]
        public void SignAndVerifyOrderEnvelopingTest()
        {
            using (var target = GetSigner(SignatureLocation.Enveloping))
            {
                var document = LoadXml(TestData.XmlOrder);

                var signed = target.Sign(document);

                DumpXml(signed);

                using (var target1 = GetSigner(SignatureLocation.Enveloping))
                    Assert.IsTrue(target1.TryVerifySignature(signed));
            }
        }

        [TestMethod]
        public void SignAndVerifyOrderDetachedTest()
        {
            using (var target = GetSigner(SignatureLocation.Detached))
            {
                var document = LoadXml(TestData.XmlOrder);

                var signed = target.Sign(document);

                DumpXml(signed);

                using (var target1 = GetSigner(SignatureLocation.Detached))
                    Assert.IsTrue(target1.TryVerifySignature(document, signed));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyNullSignatureDetachedTest()
        {
            using (var target = GetSigner(SignatureLocation.Detached))
                target.TryVerifySignature(new XmlDocument(), null);
        }

        [TestMethod]
        public void VerifyNotSigned()
        {
            using (var target = GetSigner(SignatureLocation.Enveloped))
                Assert.IsFalse(target.TryVerifySignature(LoadXml(TestData.XmlOrder), null));
        }
    }
}
