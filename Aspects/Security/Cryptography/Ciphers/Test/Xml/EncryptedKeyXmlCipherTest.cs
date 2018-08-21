using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using vm.Aspects.Security.Cryptography.Ciphers.Tests;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    [TestClass]
    public class EncryptedKeyXmlCipherTest : GenericXmlCipherTest<EncryptedKeyXmlCipher>
    {
        const string _keyFileName = "encryptedXml.key";

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

            if (GetCipherImpl() is IKeyManagement keyManagement &&
                keyManagement.KeyLocation.EndsWith(_keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        static IXmlCipher GetCipherImpl() => new EncryptedKeyXmlCipher(CertificateFactory.GetDecryptingCertificate(), null, _keyFileName);

        static IXmlCipher GetPublicCertCipher() => new EncryptedKeyXmlCipher(CertificateFactory.GetEncryptingCertificate(), null, _keyFileName);

        public override IXmlCipher GetCipher() => GetCipherImpl();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCertTest()
        {
            new EncryptedKeyXmlCipher(null, null, _keyFileName);
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

        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<EncryptedKeyXmlCipher>((EncryptedKeyXmlCipher)GetCipher());

            GC.Collect();

            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
        #endregion
    }
}
