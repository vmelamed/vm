using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Security.Cryptography.Ciphers.Tests;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    [TestClass]
    public class EncryptedKeyXmlCipherTest : GenericXmlCipherTest<EncryptedKeyXmlCipher>
    {
        const string keyFileName = "encryptedXml.key";

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var keyManagement = GetCipherImpl() as IKeyManagement;

            if (keyManagement != null &&
                keyManagement.KeyLocation.EndsWith(keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        static IXmlCipher GetCipherImpl()
        {
            return new EncryptedKeyXmlCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName);
        }

        static IXmlCipher GetPublicCertCipher()
        {
            return new EncryptedKeyXmlCipher(CertificateFactory.GetEncryptingCertificate(), null, keyFileName);
        }

        public override IXmlCipher GetCipher()
        {
            return GetCipherImpl();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCertTest()
        {
            new EncryptedKeyXmlCipher(null, null, keyFileName);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DecryptEntireDocContentPublicCertFailTest()
        {
            var document = LoadXml(TestData.XmlOrder);
            var target = GetCipher();

            target.ContentOnly = true;
            target.Encrypt(document);

            target = GetPublicCertCipher();
            target.ContentOnly = true;

            target.Decrypt(document);
        }


        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (EncryptedKeyXmlCipher)GetCipher();

            Assert.IsNotNull(target);

            using (target as IDisposable)
                Assert.IsFalse(target.IsDisposed);
            Assert.IsTrue(target.IsDisposed);

            // should do nothing:
            target.Dispose();
        }

#if NET45
        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<EncryptedKeyXmlCipher>((EncryptedKeyXmlCipher)GetCipher());

            GC.Collect();

            EncryptedKeyXmlCipher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        } 
#endif
        #endregion
    }
}
