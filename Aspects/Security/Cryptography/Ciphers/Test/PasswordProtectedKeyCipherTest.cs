using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Test
{
    [TestClass]
    public class PasswordProtectedKeyCipherTest : GenericCipherTest<PasswordProtectedKeyCipher>
    {
        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = new PasswordProtectedKeyCipher("password");

            cipher.Base64Encoded = base64;
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
        {
            throw new InvalidOperationException();
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
            var target = (PasswordProtectedKeyCipher)GetCipher();

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
            var target = new WeakReference<PasswordProtectedKeyCipher>((PasswordProtectedKeyCipher)GetCipher());

            GC.Collect();

            PasswordProtectedKeyCipher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        } 
#endif
        #endregion

        SecureString CreateSecureString(string password)
        {
            var secure = new SecureString();

            foreach (var c in password)
                secure.AppendChar(c);

            return secure;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorStringPasswordEmptyTest()
        {
            var target = new PasswordProtectedKeyCipher("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorStringPasswordBlankTest()
        {
            var target = new PasswordProtectedKeyCipher(" \t");
        }

        [TestMethod]
        public void ConstructorSecureStringTest()
        {
            var target = new PasswordProtectedKeyCipher(CreateSecureString("password"));
        }

        class InheritedPasswordProtectedKeyCipher : PasswordProtectedKeyCipher
        {
            public InheritedPasswordProtectedKeyCipher()
                : base("password")
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

#if NET45
            public Task PublicBeforeWriteEncryptedAsync(
                    Stream encryptedStream)
            {
                return base.BeforeWriteEncryptedAsync(encryptedStream);
            }

            public Task PublicBeforeReadDecryptedAsync(
                Stream encryptedStream)
            {
                return base.BeforeReadDecryptedAsync(encryptedStream);
            } 
#endif

            public byte[] PublicEncryptSymmetricKey()
            {
                return base.EncryptSymmetricKey();
            }

            public void PublicDecryptSymmetricKey(
                byte[] encryptedKey)
            {
                base.DecryptSymmetricKey(encryptedKey);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncryptedNonWritableStreamTest()
        {
            var target = new InheritedPasswordProtectedKeyCipher();

            target.PublicBeforeWriteEncrypted(TestUtilities.CreateNonWritableStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeReadDecryptedNonReadableStreamTest()
        {
            var target = new InheritedPasswordProtectedKeyCipher();

            target.PublicBeforeReadDecrypted(TestUtilities.CreateNonReadableStream());
        }

#if NET45
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void EncryptSymmetricKeyTest()
        {
            var target = new InheritedPasswordProtectedKeyCipher();

            Assert.IsNull(target.PublicEncryptSymmetricKey());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeforeWriteEncryptedNullStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedPasswordProtectedKeyCipher();

                target.PublicBeforeWriteEncryptedAsync(null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeWriteEncryptedNonWritableStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedPasswordProtectedKeyCipher();

                target.PublicBeforeWriteEncryptedAsync(TestUtilities.CreateNonWritableStream()).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeforeReadDecryptedNullStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedPasswordProtectedKeyCipher();

                target.PublicBeforeReadDecryptedAsync(null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeforeReadDecryptedNonReadableStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedPasswordProtectedKeyCipher();

                target.PublicBeforeReadDecryptedAsync(TestUtilities.CreateNonReadableStream()).Wait();
            });
        } 
#endif
    }
}
