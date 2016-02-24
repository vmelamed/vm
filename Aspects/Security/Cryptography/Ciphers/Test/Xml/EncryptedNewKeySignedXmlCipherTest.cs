using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Security.Cryptography.Ciphers.Tests;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    [TestClass]
    public class EncryptedNewKeySignedXmlCipherTest : GenericXmlCipherTest<EncryptedNewKeySignedXmlCipher>
    {
        public override IXmlCipher GetCipher() => new EncryptedNewKeySignedXmlCipher(
                                                            CertificateFactory.GetDecryptingCertificate(),
                                                            CertificateFactory.GetSigningCertificate()); // default SHA1, SHA256 doesn't work here

        public IXmlCipher GetPublicCertCipher() => new EncryptedNewKeySignedXmlCipher(
                                                            CertificateFactory.GetEncryptingCertificate(),
                                                            CertificateFactory.GetSignVerifyCertificate()); // default SHA1, SHA256 doesn't work here

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSignCertGetCipherTest()
        {
            new EncryptedNewKeySignedXmlCipher(
                    CertificateFactory.GetDecryptingCertificate(),
                    null); // default SHA1, SHA256 doesn't work here
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCertTest()
        {
            new EncryptedNewKeySignedXmlCipher(null, null);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DecryptEntireDocContentPublicCertFailTest()
        {
            var document = LoadXml(TestData.XmlOrder);

            using (var target = GetCipher())
            {

                target.ContentOnly = true;
                target.Encrypt(document);

                using (var target1 = GetPublicCertCipher())
                {
                    target1.ContentOnly = true;
                    target1.Decrypt(document);
                }
            }
        }

        class InheritedEncryptedNewKeySignedXmlCipher : EncryptedNewKeySignedXmlCipher
        {
            public InheritedEncryptedNewKeySignedXmlCipher()
                : base(null)
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitializeSignerNullCertTest()
        {
            new EncryptedNewKeySignedXmlCipher(CertificateFactory.GetDecryptingCertificate());
        }

        #region Test disabled IKeyManagement
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void KeyLocationTest()
        {
            var target = GetKeyManager();
            using (target as IDisposable)
            {
                Assert.IsNotNull(target);
                Assert.IsNull(target.KeyLocation);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ExportSymmetricKeyTest()
        {
            var target = GetKeyManager();
            using (target as IDisposable)
            {
                Assert.IsNotNull(target);
                Assert.IsNull(target.ExportSymmetricKey());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ImportSymmetricKeyTest()
        {
            var target = GetKeyManager();
            using (target as IDisposable)
            {
                Assert.IsNotNull(target);
                target.ImportSymmetricKey(new byte[17]);
                Assert.IsNull(target.ExportSymmetricKey());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ExportSymmetricKeyAsyncTest()
        {
            var target = GetKeyManager();
            using (target as IDisposable)
            {
                Assert.IsNotNull(target);
                Assert.IsNull(target.ExportSymmetricKeyAsync().Result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ImportSymmetricKeyAsyncTest()
        {
            var target = GetKeyManager();
            using (target as IDisposable)
            {
                Assert.IsNotNull(target);
                target.ImportSymmetricKey(new byte[17]);
                Assert.IsNull(target.ExportSymmetricKeyAsync().Result);
            }
        }
        #endregion

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (EncryptedNewKeySignedXmlCipher)GetCipher();

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
            var target = new WeakReference<EncryptedNewKeySignedXmlCipher>((EncryptedNewKeySignedXmlCipher)GetCipher());

            GC.Collect();

            EncryptedNewKeySignedXmlCipher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        }
        #endregion
    }
}
