using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Test
{
    [TestClass]
    public class EncryptedNewKeySha256SignedCipherTest : GenericCipherTest<EncryptedNewKeySignedCipher>
    {
        static ICipherAsync GetCipherImpl()
        {
            return new EncryptedNewKeySignedCipher(
                                CertificateFactory.GetDecryptingSha256Certificate(),
                                CertificateFactory.GetSigningSha256Certificate()); // SHA1 also works with this cert
        }

        static ICipherAsync GetCipherPublicCertImpl()
        {
            return new EncryptedNewKeySignedCipher(
                                CertificateFactory.GetEncryptingSha256Certificate(),
                                CertificateFactory.GetSigningSha256Certificate()); // SHA1 also works with this cert
        }

        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = GetCipherImpl();

            // ignore the parameter base64
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
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

#if NET45
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
#endif
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

#if NET45
        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<EncryptedNewKeySignedCipher>((EncryptedNewKeySignedCipher)GetCipher());

            GC.Collect();

            EncryptedNewKeySignedCipher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        } 
#endif
        #endregion
    }
}
