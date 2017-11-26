using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    [TestClass]
    public class ProtectedKeyXmlCipherTest : GenericXmlCipherTest<ProtectedKeyXmlCipher>
    {
        const string keyFileName = "protectedXml.key";

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var keyManagement = GetCipherImpl() as IKeyManagement;

            if (keyManagement.KeyLocation != null &&
                keyManagement.KeyLocation.EndsWith(keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        static IXmlCipher GetCipherImpl() => new ProtectedKeyXmlCipher(null, keyFileName);

        public override IXmlCipher GetCipher() => GetCipherImpl();
        class InheritedXmlCipherTest : ProtectedKeyXmlCipher
        {
            public InheritedXmlCipherTest(
                string symmetricAlgorithmName = null)
                : base(symmetricAlgorithmName)
            {
            }

            public void PublicEncryptElement(
                XmlElement element,
                EncryptedXml encryptedXml)
            {
                base.EncryptElement(element, encryptedXml);
            }

            public void PublicDecryptElement(
                XmlElement element)
            {
                base.DecryptElement(element);
            }

            public string PublicSymmetricXmlNamespace() => SymmetricXmlNamespace();

            public void SetLength(int length)
            {
                Symmetric.KeySize = length;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EncryptNullElementTest()
        {
            new InheritedXmlCipherTest().PublicEncryptElement(null, new EncryptedXml());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EncryptedXmlNullTest()
        {
            new InheritedXmlCipherTest().PublicEncryptElement(LoadXml(TestData.XmlOrder).DocumentElement, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DecryptNullElementTest()
        {
            new InheritedXmlCipherTest().PublicDecryptElement(null);
        }

        [TestMethod]
        public void SymmetricNamespaceTest()
        {
            var target = new InheritedXmlCipherTest(Algorithms.Symmetric.TripleDes);
            Assert.AreEqual(EncryptedXml.XmlEncTripleDESUrl, target.PublicSymmetricXmlNamespace());

#pragma warning disable 618
            target = new InheritedXmlCipherTest(Algorithms.Symmetric.Des);
            Assert.AreEqual(EncryptedXml.XmlEncDESUrl, target.PublicSymmetricXmlNamespace());
#pragma warning restore 618

            target = new InheritedXmlCipherTest(Algorithms.Symmetric.Aes);
            target.SetLength(256);
            Assert.AreEqual(EncryptedXml.XmlEncAES256Url, target.PublicSymmetricXmlNamespace());

            target = new InheritedXmlCipherTest(Algorithms.Symmetric.Aes);
            target.SetLength(192);
            Assert.AreEqual(EncryptedXml.XmlEncAES192Url, target.PublicSymmetricXmlNamespace());

            target = new InheritedXmlCipherTest(Algorithms.Symmetric.Aes);
            target.SetLength(128);
            Assert.AreEqual(EncryptedXml.XmlEncAES128Url, target.PublicSymmetricXmlNamespace());
        }


        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void SymmetricUnknownNamespaceTest()
        {
#pragma warning disable 618
            var target = new InheritedXmlCipherTest(Algorithms.Symmetric.RC2);
            target.PublicSymmetricXmlNamespace();
#pragma warning restore 618
        }

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (ProtectedKeyXmlCipher)GetCipher();

            Assert.IsNotNull(target);

            using (target as IDisposable)
                Assert.IsFalse(target.IsDisposed);
            Assert.IsTrue(target.IsDisposed);

            // should do nothing:
            target.Dispose();
        }

        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<ProtectedKeyXmlCipher>((ProtectedKeyXmlCipher)GetCipher());

            GC.Collect();

            ProtectedKeyXmlCipher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        }
        #endregion
    }
}
