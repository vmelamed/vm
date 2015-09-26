﻿using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NET45
using System.Linq;
using System.Threading.Tasks;
#endif

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\Readme.txt")]
    public class EncryptedNewKeyHashedCipherTest : GenericCipherTest<EncryptedNewKeyHashedCipher>
    {
        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = new EncryptedNewKeyHashedCipher(CertificateFactory.GetDecryptingCertificate(), Algorithms.Symmetric.Aes, Algorithms.Hash.MD5);

            // ignore the parameter base64
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
        {
            return new EncryptedNewKeyHashedCipher(CertificateFactory.GetEncryptingCertificate(), Algorithms.Symmetric.Aes, Algorithms.Hash.MD5);
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

#if NET40
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
            var target = (EncryptedNewKeyHashedCipher)GetCipher();

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
            var target = new WeakReference<EncryptedNewKeyHashedCipher>((EncryptedNewKeyHashedCipher)GetCipher());

            GC.Collect();

            EncryptedNewKeyHashedCipher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        }
#endif
        #endregion

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void RoundTripModifiedHashTest()
        {
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            using (var target = GetCipher())
            {
                var encrypted = target.Encrypt(input);

                encrypted[5]++;

                using (var target1 = GetCipher())
                {

                    var output =  target1.Decrypt(encrypted);
                }
            }
        }

        class InheritedEncryptedNewKeyHashedCipher : EncryptedNewKeyHashedCipher
        {
            public InheritedEncryptedNewKeyHashedCipher()
                : base(CertificateFactory.GetEncryptingCertificate(), null)
            {
            }

            public string PublicHashAlgorithmName
            {
                get { return base.HashAlgorithmName; }
            }

            public HashAlgorithm PublicHasher
            {
                get { return base.Hasher; }
            }

            public void PublicInitializeHasher(
                string hashAlgorithm)
            {
                base.InitializeHasher(hashAlgorithm);
            }

            public void PublicBeforeWriteEncrypted(
                Stream encryptedStream)
            {
                base.BeforeWriteEncrypted(encryptedStream);
            }

            public void PublicReserveSpaceForHash(
                Stream encryptedStream)
            {
                base.ReserveSpaceForHash(encryptedStream);
            }

            public CryptoStream PublicCreateEncryptingStream(
                Stream encryptedStream)
            {
                return base.CreateEncryptingStream(encryptedStream);
            }

            public void PublicWriteHashInReservedSpace(
                Stream encryptedStream,
                byte[] hash)
            {
                base.WriteHashInReservedSpace(encryptedStream, hash);
            }

            public CryptoStream PublicCreateDecryptingStream(
                Stream encryptedStream)
            {
                return base.CreateDecryptingStream(encryptedStream);
            }

            public void PublicBeforeReadDecrypted(
                Stream encryptedStream)
            {
                base.BeforeReadDecrypted(encryptedStream);
            }

#if NET45
            public async Task PublicBeforeWriteEncryptedAsync(
                    Stream encryptedStream)
            {
                await base.BeforeWriteEncryptedAsync(encryptedStream);
            }

            public async Task PublicBeforeReadDecryptedAsync(
                Stream encryptedStream)
            {
                await base.BeforeReadDecryptedAsync(encryptedStream);
            }

            public void PublicLoadHashToValidate(
                Stream encryptedStream)
            {
                base.LoadHashToValidate(encryptedStream);
            }

            public async Task PublicLoadHashToValidateAsync(
                Stream encryptedStream)
            {
                await base.LoadHashToValidateAsync(encryptedStream);
            }
#endif

            public void PublicAfterReadDecrypted(
                Stream encryptedStream,
                CryptoStream cryptoStream)
            {
                base.AfterReadDecrypted(encryptedStream, cryptoStream);
            }

            public byte[] PublicFinalizeHashAfterWrite(
                Stream encryptedStream,
                CryptoStream cryptoStream)
            {
                return base.FinalizeHashAfterWrite(encryptedStream, cryptoStream);
            }

            public byte[] PublicFinalizeHashAfterRead(
                Stream encryptedStream,
                CryptoStream cryptoStream)
            {
                return base.FinalizeHashAfterRead(encryptedStream, cryptoStream);
            }
        }

        [TestMethod]
        public void InitializeHasher1Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicInitializeHasher(null);
                Assert.AreEqual(Algorithms.Hash.Default, target.PublicHashAlgorithmName);
            }
        }

        [TestMethod]
        public void InitializeHasher2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicInitializeHasher("");
                Assert.AreEqual(Algorithms.Hash.Default, target.PublicHashAlgorithmName);
            }
        }

        [TestMethod]
        public void InitializeHasher3Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicInitializeHasher(" \t");
                Assert.AreEqual(Algorithms.Hash.Default, target.PublicHashAlgorithmName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncrypted2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicBeforeWriteEncrypted(TestUtilities.CreateNonWritableStream());
            }
        }

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeforeWriteEncryptedAsync1Test()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = new InheritedEncryptedNewKeyHashedCipher())
                {
                    target.PublicBeforeWriteEncryptedAsync(null).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncryptedAsync2Test()
        {
            try
            {
                using (var target = new InheritedEncryptedNewKeyHashedCipher())
                {
                    target.PublicBeforeWriteEncryptedAsync(TestUtilities.CreateNonWritableStream()).Wait();
                }
            }
            catch (AggregateException x)
            {
                throw x.InnerExceptions.First();
            }
        }
#endif

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReserveSpaceForHash2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicReserveSpaceForHash(TestUtilities.CreateNonWritableStream());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateEncryptingStream2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicCreateEncryptingStream(TestUtilities.CreateNonWritableStream());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteHashInReservedSpace2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicWriteHashInReservedSpace(TestUtilities.CreateNonWritableStream(), null);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateDecryptingStream2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                using (var stream = TestUtilities.CreateNonReadableStream())
                    target.PublicCreateDecryptingStream(stream);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeReadDecrypted2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                using (var stream = TestUtilities.CreateNonReadableStream())
                    target.PublicBeforeReadDecrypted(stream);
            }
        }

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeforeReadDecryptedAsync1Test()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = new InheritedEncryptedNewKeyHashedCipher())
                {
                    target.PublicBeforeReadDecryptedAsync(null).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeReadDecryptedAsync2Test()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = new InheritedEncryptedNewKeyHashedCipher())
                {
                    using (var stream = TestUtilities.CreateNonReadableStream())
                        target.PublicBeforeReadDecryptedAsync(stream).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadHashToValidate2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                using (var stream = TestUtilities.CreateNonReadableStream())
                    target.PublicLoadHashToValidate(stream);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadHashToValidateAsync1Test()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = new InheritedEncryptedNewKeyHashedCipher())
                {
                    target.PublicLoadHashToValidateAsync(null).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadHashToValidateAsync2Test()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = new InheritedEncryptedNewKeyHashedCipher())
                {
                    using (var stream = TestUtilities.CreateNonReadableStream())
                        target.PublicLoadHashToValidateAsync(stream).Wait();
                }
            });
        }
#endif

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FinalizeHashAfterWrite2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicFinalizeHashAfterWrite(
                             TestUtilities.CreateNonWritableStream(),
                             new CryptoStream(new NullStream(), target.PublicHasher, CryptoStreamMode.Write));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FinalizeHashAfterWrite4Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                target.PublicFinalizeHashAfterWrite(
                            new MemoryStream(new byte[10], true),
                            new CryptoStream(TestUtilities.CreateNonWritableStream(), target.PublicHasher, CryptoStreamMode.Read));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FinalizeHashAfterRead2Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                using (var stream = TestUtilities.CreateNonReadableStream())
                    target.PublicFinalizeHashAfterRead(
                            stream,
                            new CryptoStream(new NullStream(), target.PublicHasher, CryptoStreamMode.Write));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FinalizeHashAfterRead4Test()
        {
            using (var target = new InheritedEncryptedNewKeyHashedCipher())
            {
                using (var stream = TestUtilities.CreateNonReadableStream())
                    target.PublicFinalizeHashAfterRead(
                            new MemoryStream(new byte[10], true),
                            new CryptoStream(stream, target.PublicHasher, CryptoStreamMode.Write));
            }
        }
    }
}
