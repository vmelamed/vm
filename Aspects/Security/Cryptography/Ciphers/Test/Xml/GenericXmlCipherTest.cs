using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    public abstract class GenericXmlCipherTest<TXmlCipher> where TXmlCipher : IXmlCipher
    {
        public virtual IXmlCipher GetCipher()
        {
            throw new NotImplementedException();
        }

        public virtual IKeyManagement GetKeyManager()
        {
            var keyMgr = GetCipher() as IKeyManagement;

            Assert.IsNotNull(keyMgr);
            return keyMgr;
        }

        public TestContext TestContext { get; set; }

        public void DumpXml(XmlDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (var x = XmlWriter.Create(w, new XmlWriterSettings() { Indent = true, NamespaceHandling = NamespaceHandling.OmitDuplicates }))
                    document.Save(x);

                TestContext.WriteLine("\n{0}", w.GetStringBuilder());
            }
        }

        public XmlDocument LoadXml(string xmlDocument)
        {
            var doc = new XmlDocument();

            doc.LoadXml(xmlDocument);

            return doc;
        }

        public XmlNamespaceManager GetNamespaceManager(XmlDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var nsMgr = new XmlNamespaceManager(document.NameTable);

            nsMgr.AddNamespace("pi", "urn:test:paymentInfo");
            nsMgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");

            return nsMgr;
        }

        [TestMethod]
        public void EncryptEntireDocTest()
        {
            var document = LoadXml(TestData.XmlOrder);

            using (var target = GetCipher())
                target.Encrypt(document);

            DumpXml(document);
            Assert.AreEqual(0, document.SelectNodes("/order").Count);
            Assert.AreEqual(1, document.SelectNodes("/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void DecryptEntireDocTest()
        {
            var document = LoadXml(TestData.XmlOrder);

            using (var target = GetCipher())
                target.Encrypt(document);

            DumpXml(document);

            using (var target1 = GetCipher())
                target1.Decrypt(document);

            DumpXml(document);
            Assert.AreEqual(1, document.SelectNodes("/order").Count);
            Assert.AreEqual(0, document.SelectNodes("/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void EncryptEntireDocContentTest()
        {
            var document = LoadXml(TestData.XmlOrder);
            using (var target = GetCipher())
            {

                target.ContentOnly = true;
                target.Encrypt(document);

                DumpXml(document);
                Assert.AreEqual(1, document.SelectNodes("/order").Count);
                Assert.AreEqual(1, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
            }
        }

        [TestMethod]
        public void DecryptEntireDocContentTest()
        {
            var document = LoadXml(TestData.XmlOrder);
            using (var target = GetCipher())
            {
                target.ContentOnly = true;
                target.Encrypt(document);
                DumpXml(document);

                using (var target1 = GetCipher())
                {
                    target1.ContentOnly = true;
                    target1.Decrypt(document);
                }

                DumpXml(document);
                Assert.AreEqual(1, document.SelectNodes("/order/billing").Count);
                Assert.AreEqual(0, document.SelectNodes("/enc:EncryptedData", GetNamespaceManager(document)).Count);
            }
        }

        [TestMethod]
        public void Encrypt1Test()
        {
            var document = LoadXml(TestData.XmlOrder);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing");

            DumpXml(document);
            Assert.AreEqual(0, document.SelectNodes("/order/billing").Count);
            Assert.AreEqual(1, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void Decrypt1Test()
        {
            var document = LoadXml(TestData.XmlOrder);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing");

            DumpXml(document);

            using (var target1 = GetCipher())
                target1.Decrypt(document);

            DumpXml(document);
            Assert.AreEqual(1, document.SelectNodes("/order/billing").Count);
            Assert.AreEqual(0, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void Encrypt2Test()
        {
            var document = LoadXml(TestData.XmlOrder2);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing");

            DumpXml(document);
            Assert.AreEqual(0, document.SelectNodes("/order/billing").Count);
            Assert.AreEqual(2, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void Decrypt2Test()
        {
            var document = LoadXml(TestData.XmlOrder2);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing");
            DumpXml(document);

            using (var target1 = GetCipher())
                target1.Decrypt(document);

            DumpXml(document);
            Assert.AreEqual(2, document.SelectNodes("/order/billing").Count);
            Assert.AreEqual(0, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void Encrypt1NsTest()
        {
            var document = LoadXml(TestData.XmlNsOrder);
            var nsMgr = GetNamespaceManager(document);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing", nsMgr);

            DumpXml(document);
            Assert.AreEqual(0, document.SelectNodes("/order/billing").Count);
            Assert.AreEqual(1, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void Decrypt1NsTest()
        {
            var document = LoadXml(TestData.XmlNsOrder);
            var nsMgr = GetNamespaceManager(document);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing", nsMgr);

            DumpXml(document);

            using (var target1 = GetCipher())
                target1.Decrypt(document);

            DumpXml(document);
            Assert.AreEqual(1, document.SelectNodes("/order/billing").Count);
            Assert.AreEqual(0, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void Encrypt2NsTest()
        {
            var document = LoadXml(TestData.XmlNsOrder2);
            var nsMgr = GetNamespaceManager(document);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing", nsMgr);

            DumpXml(document);
            Assert.AreEqual(0, document.SelectNodes("/order/billing").Count);
            Assert.AreEqual(2, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void Decrypt2NsTest()
        {
            var document = LoadXml(TestData.XmlNsOrder2);
            var nsMgr = GetNamespaceManager(document);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing", nsMgr);

            DumpXml(document);

            using (var target1 = GetCipher())
                target1.Decrypt(document);

            DumpXml(document);
            Assert.AreEqual(2, document.SelectNodes("/order/billing").Count);
            Assert.AreEqual(0, document.SelectNodes("/order/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void EncryptNs1Test()
        {
            var document = LoadXml(TestData.XmlNsOrder);
            var nsMgr = GetNamespaceManager(document);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing/pi:paymentInfo", nsMgr);

            DumpXml(document);
            Assert.AreEqual(0, document.SelectNodes("/order/billing/pi:paymentInfo", GetNamespaceManager(document)).Count);
            Assert.AreEqual(1, document.SelectNodes("/order/billing/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void DecryptNs1Test()
        {
            var document = LoadXml(TestData.XmlNsOrder);
            var nsMgr = GetNamespaceManager(document);

            using (var target = GetCipher())
                target.Encrypt(document, "/order/billing/pi:paymentInfo", nsMgr);

            DumpXml(document);

            using (var target = GetCipher())
                target.Decrypt(document);

            DumpXml(document);
            Assert.AreEqual(1, document.SelectNodes("/order/billing/pi:paymentInfo", GetNamespaceManager(document)).Count);
            Assert.AreEqual(0, document.SelectNodes("/order/billing/enc:EncryptedData", GetNamespaceManager(document)).Count);
        }

        [TestMethod]
        public void EncryptContentTest()
        {
            var document = LoadXml(TestData.XmlOrder);
            using (var target = GetCipher())
            {
                target.ContentOnly = true;
                target.Encrypt(document, "/order/billing");

                DumpXml(document);
                Assert.AreEqual(1, document.SelectNodes("/order/billing").Count);
                Assert.AreEqual(0, document.SelectNodes("/order/billing/paymentInfo").Count);
                Assert.AreEqual(1, document.SelectNodes("/order/billing/enc:EncryptedData", GetNamespaceManager(document)).Count);
            }
        }

        [TestMethod]
        public void DecryptContentTest()
        {
            var document = LoadXml(TestData.XmlOrder);
            using (var target = GetCipher())
            {
                target.ContentOnly = true;
                target.Encrypt(document, "/order/billing");
                DumpXml(document);

                using (var target1 = GetCipher())
                {
                    target1.ContentOnly = true;
                    target1.Decrypt(document);
                }

                DumpXml(document);
                Assert.AreEqual(1, document.SelectNodes("/order/billing").Count);
                Assert.AreEqual(1, document.SelectNodes("/order/billing/paymentInfo").Count);
                Assert.AreEqual(0, document.SelectNodes("/order/billing/enc:EncryptedData", GetNamespaceManager(document)).Count);
            }
        }
    }
}
