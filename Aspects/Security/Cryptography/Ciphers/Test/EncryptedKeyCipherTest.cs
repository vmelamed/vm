using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class EncryptedKeyCipherTest : GenericCipherTest<EncryptedKeyCipher>
    {
        const string _keyFileName = "encrypted.key";

        public override ICipherAsync GetCipher(bool base64 = false)
            => new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, _keyFileName)
            {
                Base64Encoded = base64,
            };

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
            => new EncryptedKeyCipher(CertificateFactory.GetEncryptingCertificate(), null, _keyFileName)
            {
                Base64Encoded = base64,
            };

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var keyManagement = new EncryptedKeyCipher(CertificateFactory.GetEncryptingCertificate(), null, _keyFileName) as IKeyManagement;

            if (keyManagement.KeyLocation.EndsWith(_keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void CertificateNullTest()
        {
            var target = new EncryptedKeyCipher(null, null, _keyFileName);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void PublicCertificateTest()
        {
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var target = GetCipher();
            var encrypted = target.Encrypt(input);

            target = new EncryptedKeyCipher(CertificateFactory.GetEncryptingCertificate(), null, _keyFileName);

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

        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<EncryptedKeyCipher>((EncryptedKeyCipher)GetCipher());

            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
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
        #endregion

        class InheritedEncryptedKeyCipher : EncryptedKeyCipher
        {
            public InheritedEncryptedKeyCipher()
                : base(CertificateFactory.GetDecryptingCertificate(), null)
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
