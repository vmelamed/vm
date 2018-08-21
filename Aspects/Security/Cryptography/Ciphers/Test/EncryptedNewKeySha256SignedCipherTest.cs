using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class EncryptedNewKeySha256SignedCipherTest : GenericCipherTest<EncryptedNewKeySignedCipher>
    {
        static ICipherTasks GetCipherImpl() => new EncryptedNewKeySignedCipher(
                                                        CertificateFactory.GetDecryptingSha256Certificate(),
                                                        CertificateFactory.GetSigningSha256Certificate()); // SHA1 also works with this cert

        static ICipherTasks GetCipherPublicCertImpl() => new EncryptedNewKeySignedCipher(
                                                                CertificateFactory.GetEncryptingSha256Certificate(),
                                                                CertificateFactory.GetSigningSha256Certificate()); // SHA1 also works with this cert

        public override ICipherTasks GetCipher(bool base64 = false)
        {
            var cipher = GetCipherImpl();

            // ignore the parameter base64
            return cipher;
        }

        public override ICipherTasks GetPublicCertCipher(bool base64 = false)
        {
            var cipher = GetCipherPublicCertImpl();

            // ignore the parameter base64
            return cipher;
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
            var target = GetKeyManagerTasks();
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
            var target = GetKeyManagerTasks();
            using (target as IDisposable)
            {
                Assert.IsNotNull(target);
                target.ImportSymmetricKeyAsync(new byte[17]).Wait();
                Assert.IsNull(target.ExportSymmetricKeyAsync().Result);
            }
        }
        #endregion

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (EncryptedNewKeySignedCipher)GetCipher();

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
            var target = new WeakReference<EncryptedNewKeySignedCipher>((EncryptedNewKeySignedCipher)GetCipher());

            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
        #endregion
    }
}
