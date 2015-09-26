using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NET45
using System.Threading.Tasks;
#endif

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\Readme.txt")]
    public class EncryptedNewKeyCipherTest : GenericCipherTest<EncryptedNewKeyCipher>
    {
        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = new EncryptedNewKeyCipher(CertificateFactory.GetDecryptingCertificate());

            cipher.Base64Encoded = base64;
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
        {
            var cipher = new EncryptedNewKeyCipher(CertificateFactory.GetEncryptingCertificate());

            cipher.Base64Encoded = base64;
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
            var target = (EncryptedNewKeyCipher)GetCipher();

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
            var target = new WeakReference<EncryptedNewKeyCipher>((EncryptedNewKeyCipher)GetCipher());

            GC.Collect();

            EncryptedNewKeyCipher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        }
#endif
        #endregion

        class InheritedEncryptedNewKeyCipher : EncryptedNewKeyCipher
        {
            public InheritedEncryptedNewKeyCipher()
                : base(CertificateFactory.GetDecryptingCertificate(), null)
            {
            }

            public void PublicBeforeWriteEncrypted(
                Stream encryptedStream)
            {
                InitializeSymmetricKey();
                base.BeforeWriteEncrypted(encryptedStream);
            }

            public void PublicBeforeReadDecrypted(
                Stream encryptedStream)
            {
                InitializeSymmetricKey();
                base.BeforeReadDecrypted(encryptedStream);
            }

#if NET45
            public async Task PublicBeforeWriteEncryptedAsync(
                    Stream encryptedStream)
            {
                await InitializeSymmetricKeyAsync();
                await base.BeforeWriteEncryptedAsync(encryptedStream);
            }

            public async Task PublicBeforeReadDecryptedAsync(
                Stream encryptedStream)
            {
                await InitializeSymmetricKeyAsync();
                await base.BeforeReadDecryptedAsync(encryptedStream);
            }
#endif
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncryptedNonWritableStreamTest()
        {
            var target = new InheritedEncryptedNewKeyCipher();

            target.PublicBeforeWriteEncrypted(TestUtilities.CreateNonWritableStream());
        }

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeforeWriteEncryptedAsyncNullStreamTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedEncryptedNewKeyCipher();

                target.PublicBeforeWriteEncryptedAsync(null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncryptedAsyncNonWritableStreamTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedEncryptedNewKeyCipher();

                target.PublicBeforeWriteEncryptedAsync(TestUtilities.CreateNonWritableStream()).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeforeReadDecryptedAsyncNullStreamTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedEncryptedNewKeyCipher();

                target.PublicBeforeReadDecryptedAsync(null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeReadDecryptedAsyncNonReadableStreamTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedEncryptedNewKeyCipher();

                using (var stream = TestUtilities.CreateNonReadableStream())
                    target.PublicBeforeReadDecryptedAsync(stream).Wait();
            });
        }
#endif

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeReadDecryptedNonReadableStreamTest()
        {
            var target = new InheritedEncryptedNewKeyCipher();

            using (var stream = TestUtilities.CreateNonReadableStream())
                target.PublicBeforeReadDecrypted(stream);
        }
    }
}
