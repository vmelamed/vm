using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Test
{
    [TestClass]
    public class EncryptedKeyCipherTest1 : GenericCipherTest<EncryptedKeyCipher>
    {
        const string keyFileName = "encrypted.key";

        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName);

            cipher.Base64Encoded = base64;
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
        {
            var cipher = new EncryptedKeyCipher(CertificateFactory.GetEncryptingCertificate(), null, keyFileName);

            cipher.Base64Encoded = base64;
            return cipher;
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var keyManagement = new EncryptedKeyCipher(CertificateFactory.GetEncryptingCertificate(), null, keyFileName) as IKeyManagement;

            if (keyManagement.KeyLocation.EndsWith(keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void CertificateNullTest()
        {
            var target = new EncryptedKeyCipher(null, null, keyFileName);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void PublicCertificateTest()
        {
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var target = GetCipher();
            var encrypted = target.Encrypt(input);

            target = new EncryptedKeyCipher(CertificateFactory.GetEncryptingCertificate(), null, keyFileName);

            var output =  target.Decrypt(encrypted);
        }

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (EncryptedKeyCipher)GetCipher();

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
            var target = new WeakReference<EncryptedKeyCipher>((EncryptedKeyCipher)GetCipher());

            GC.Collect();

            EncryptedKeyCipher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        } 
#endif
        #endregion

        #region IKeyManagement tests
        [TestMethod]
        public void ImportSymmetricKeyTest()
        {
            var target = GetCipher() as IKeyManagement;

            Assert.IsNotNull(target);

            var key = new byte[target.ExportSymmetricKey().Length].FillRandom();

            target.ImportSymmetricKey(key);
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void ImportWrongLengthSymmetricKeyTest()
        {
            var target = GetCipher() as IKeyManagement;

            Assert.IsNotNull(target);

            target.ImportSymmetricKey(new byte[17]);
        }

        [TestMethod]
        public void ExportSymmetricKeyTest()
        {
            var target = GetCipher() as IKeyManagement;

            Assert.IsNotNull(target);

            var key = target.ExportSymmetricKey();

            Assert.IsNotNull(key);
            Assert.AreEqual(32, key.Length);
        }

#if NET45
        [TestMethod]
        public void ExportSymmetricKeyAsyncTest()
        {
            var target = GetCipher() as IKeyManagement;

            Assert.IsNotNull(target);

            var keyTask = target.ExportSymmetricKeyAsync();
            var key = keyTask.Result;

            Assert.IsNotNull(key);
            Assert.AreEqual(32, key.Length);
        } 

        [TestMethod]
        public void ImportSymmetricKeyAsyncTest()
        {
            var target = GetCipher() as IKeyManagement;

            Assert.IsNotNull(target);

            var keyTask = target.ExportSymmetricKeyAsync();
            var keyOld = keyTask.Result;

            var key = new byte[keyOld.Length].FillRandom();

            target.ImportSymmetricKeyAsync(key).Wait();
        }
#endif

        #endregion

        class InheritedEncryptedKeyCipher : EncryptedKeyCipher
        {
            public InheritedEncryptedKeyCipher()
                : base(null, CertificateFactory.GetEncryptingCertificate())
            {
            }

            public void PublicDecryptIV(
                byte[] encryptedIV)
            {
                base.DecryptIV(encryptedIV);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DecryptIVNoKeyTest()
        {
            var target = new InheritedEncryptedKeyCipher();

            target.PublicDecryptIV(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }
    }
}
