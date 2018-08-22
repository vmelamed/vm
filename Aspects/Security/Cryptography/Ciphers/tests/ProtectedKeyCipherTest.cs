using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\Readme.txt")]
    public class ProtectedKeyCipherDIAlgorithmTest : GenericCipherTest<ProtectedKeyCipher>
    {
        const string _keyFileName = "protected.key";

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
                keyManagement.KeyLocation.EndsWith(_keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        static ICipherTasks GetCipherImpl() => new ProtectedKeyCipher(_keyFileName);

        public override ICipherTasks GetCipher(bool base64 = false)
        {
            var cipher = GetCipherImpl();

            cipher.Base64Encoded = base64;
            return cipher;
        }

        public override ICipherTasks GetPublicCertCipher(bool base64 = false)
        {
            throw new InvalidOperationException();
        }

        [TestMethod]
        public void RoundTripTest0()
        {
            string keyFile = null;

            try
            {
                var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

                using (var target = new ProtectedKeyCipher())
                {
                    var encrypted = target.Encrypt(input);

                    keyFile = target.KeyLocation;
                    Assert.IsTrue(File.Exists(keyFile));

                    using (var target1 = new ProtectedKeyCipher())
                    {
                        var output =  target1.Decrypt(encrypted);

                        Assert.IsTrue(input.SequenceEqual(output));
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(keyFile))
                    File.Delete(keyFile);
            }
        }

        [TestMethod]
        public void RoundTripAsyncTest0()
        {
            string keyFile = null;

            try
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
                var encrypted = new MemoryStream();

                using (var target = new ProtectedKeyCipher())
                {
                    target.EncryptAsync(input, encrypted).Wait();

                    var buffer = encrypted.ToArray();

                    encrypted = new MemoryStream(buffer, 0, buffer.Length, false);
                    keyFile = target.KeyLocation;

                    Assert.IsTrue(File.Exists(keyFile));

                    using (var target1 = new ProtectedKeyCipher())
                    {
                        var output = new MemoryStream();
                        target1.DecryptAsync(encrypted, output).Wait();

                        input.Close();
                        output.Close();
                        Assert.IsTrue(input.ToArray().SequenceEqual(output.ToArray()));
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(keyFile))
                    File.Delete(keyFile);
            }
        }

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (ProtectedKeyCipher)GetCipher();

            Assert.IsNotNull(target);

            Assert.IsFalse(target.IsDisposed);
            (target as IDisposable)?.Dispose();
            Assert.IsTrue(target.IsDisposed);

            // should do nothing:
            target.Dispose();
        }

        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<ProtectedKeyCipher>((ProtectedKeyCipher)GetCipher());

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
            Assert.IsTrue(key.Length >= 24);
        }

        [TestMethod]
        public void ExportSymmetricKeyAsyncTest()
        {
            var target = GetCipher() as IKeyManagementTasks;

            Assert.IsNotNull(target);

            var keyTask = target.ExportSymmetricKeyAsync();
            var key = keyTask.Result;

            Assert.IsNotNull(key);
            Assert.IsTrue(key.Length >= 24);
        }

        [TestMethod]
        public void ImportSymmetricKeyAsyncTest()
        {
            var target = GetCipher() as IKeyManagementTasks;

            Assert.IsNotNull(target);

            var keyTask = target.ExportSymmetricKeyAsync();
            var keyOld = keyTask.Result;
            var key = new byte[keyOld.Length].FillRandom();

            target.ImportSymmetricKeyAsync(key).Wait();
        }
        #endregion

        class InheritedProtectedKeyCipher : ProtectedKeyCipher
        {
            public InheritedProtectedKeyCipher()
                : base(_keyFileName, (IKeyLocationStrategy)null)
            {
            }

            public void PublicBeforeWriteEncrypted(
                Stream encryptedStream)
            {
                base.BeforeWriteEncrypted(encryptedStream);
            }

            public void PublicBeforeReadDecrypted(
                Stream encryptedStream)
            {
                base.BeforeReadDecrypted(encryptedStream);
            }

            public Task PublicBeforeWriteEncryptedAsync(
                    Stream encryptedStream) => base.BeforeWriteEncryptedAsync(encryptedStream);

            public Task PublicBeforeReadDecryptedAsync(
                Stream encryptedStream) => base.BeforeReadDecryptedAsync(encryptedStream);

            public CryptoStream PublicCreateEncryptingStream(
                Stream encryptedStream) => base.CreateEncryptingStream(encryptedStream);

            public CryptoStream PublicCreateDecryptingStream(
                Stream encryptedStream) => base.CreateDecryptingStream(encryptedStream);

            public void PublicDoEncrypt(
                Stream dataStream,
                Stream cryptoStream)
            {
                base.DoEncrypt(dataStream, cryptoStream);
            }

            public void PublicDoDecrypt(
                Stream cryptoStream,
                Stream dataStream)
            {
                base.DoDecrypt(cryptoStream, dataStream);
            }

            public Task PublicDoEncryptAsync(
                    Stream dataStream,
                    Stream cryptoStream) => base.DoEncryptAsync(dataStream, cryptoStream);

            public Task PublicDoDecryptAsync(
                Stream cryptoStream,
                Stream dataStream) => base.DoDecryptAsync(cryptoStream, dataStream);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncryptedNonWritableStreamTest()
        {
            var target = new InheritedProtectedKeyCipher();

            target.PublicBeforeWriteEncrypted(TestUtilities.CreateNonWritableStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeReadDecryptedNonReadableStreamTest()
        {
            var target = new InheritedProtectedKeyCipher();

            target.PublicBeforeReadDecrypted(TestUtilities.CreateNonReadableStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeforeWriteEncryptedNullStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicBeforeWriteEncryptedAsync(null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncryptedNonWritableStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicBeforeWriteEncryptedAsync(TestUtilities.CreateNonWritableStream()).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeforeReadDecryptedNullStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicBeforeReadDecryptedAsync(null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeReadDecryptedNonReadableStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicBeforeReadDecryptedAsync(TestUtilities.CreateNonReadableStream()).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateEncryptingStreamNonWritableTest()
        {
            var target = new InheritedProtectedKeyCipher();

            target.PublicCreateEncryptingStream(TestUtilities.CreateNonWritableStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateDecryptingStreamNonReadableTest()
        {
            var target = new InheritedProtectedKeyCipher();

            target.PublicCreateDecryptingStream(TestUtilities.CreateNonReadableStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DoEncryptNonReadableDataTest()
        {
            var target = new InheritedProtectedKeyCipher();

            target.PublicDoEncrypt(TestUtilities.CreateNonReadableStream(), new MemoryStream(new byte[10]));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DoEncryptNonWritableCryptoTest()
        {
            var target = new InheritedProtectedKeyCipher();

            target.PublicDoEncrypt(new MemoryStream(new byte[10]), TestUtilities.CreateNonWritableStream());
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DoEncryptNullDataAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicDoEncryptAsync(null, new MemoryStream(new byte[10])).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DoEncryptNonReadableDataAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicDoEncryptAsync(TestUtilities.CreateNonReadableStream(), new MemoryStream(new byte[10])).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DoEncryptNullCryptoAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicDoEncryptAsync(new MemoryStream(new byte[10]), null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DoEncryptNonWritableCryptoAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicDoEncryptAsync(new MemoryStream(new byte[10]), TestUtilities.CreateNonWritableStream()).Wait();
            });
        }

        ////

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DoDecryptNonReadableCryptoTest()
        {
            var target = new InheritedProtectedKeyCipher();

            target.PublicDoDecrypt(TestUtilities.CreateNonReadableStream(), new MemoryStream(new byte[10]));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DoDecryptNonWritableDataTest()
        {
            var target = new InheritedProtectedKeyCipher();

            target.PublicDoDecrypt(new MemoryStream(new byte[10]), TestUtilities.CreateNonWritableStream());
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DoDecryptNullCryptoAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicDoDecryptAsync(null, new MemoryStream(new byte[10])).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DoDecryptNonReadableCryptoAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicDoDecryptAsync(TestUtilities.CreateNonReadableStream(), new MemoryStream(new byte[10])).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DoDecryptNullDataAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicDoDecryptAsync(new MemoryStream(new byte[10]), null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DoDecryptNonWritableDataAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedProtectedKeyCipher();

                target.PublicDoDecryptAsync(new MemoryStream(new byte[10]), TestUtilities.CreateNonWritableStream()).Wait();
            });
        }
    }
}
