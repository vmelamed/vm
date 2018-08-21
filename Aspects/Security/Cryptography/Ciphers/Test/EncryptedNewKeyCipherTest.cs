using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\Readme.txt")]
    public class EncryptedNewKeyCipherTest : GenericCipherTest<EncryptedNewKeyCipher>
    {
        public override ICipherTasks GetCipher(bool base64 = false)
            => new EncryptedNewKeyCipher(CertificateFactory.GetDecryptingCertificate())
            {
                Base64Encoded = base64,
            };

        public override ICipherTasks GetPublicCertCipher(bool base64 = false)
            => new EncryptedNewKeyCipher(CertificateFactory.GetEncryptingCertificate())
            {
                Base64Encoded = base64,
            };

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
            var target = (EncryptedNewKeyCipher)GetCipher();

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
            var target = new WeakReference<EncryptedNewKeyCipher>((EncryptedNewKeyCipher)GetCipher());

            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
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
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncryptedNonWritableStreamTest()
        {
            var target = new InheritedEncryptedNewKeyCipher();

            target.PublicBeforeWriteEncrypted(TestUtilities.CreateNonWritableStream());
        }

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
