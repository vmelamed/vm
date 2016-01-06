using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for DuplicateGenericTest
    /// </summary>
    [TestClass]
    public class DuplicateTest
    {
        const string keyFileName = "duplicate.key";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        protected virtual EncryptedKeyCipher GetCipher(bool base64 = false)
        {
            using (var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName))
            {
                cipher.Base64Encoded = base64;
                cipher.ExportSymmetricKey();

                return cipher.Duplicate() as EncryptedKeyCipher;
            }
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

        [TestMethod]
        public virtual void DuplicateCanDecryptFromOriginal()
        {
            using (var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName))
            {
                cipher.ExportSymmetricKey();

                using (var dupe = cipher.Duplicate())
                {
                    var encrypted = cipher.Encrypt(keyFileName);
                    var decrypted = dupe.Decrypt<string>(encrypted);

                    Assert.AreEqual(keyFileName, decrypted);
                }
            }
        }

        [TestMethod]
        public virtual void OriginalCanDecryptFromDuplicate()
        {
            using (var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName))
            {
                cipher.ExportSymmetricKey();

                using (var dupe = cipher.Duplicate())
                {
                    var encrypted = dupe.Encrypt(keyFileName);
                    var decrypted = cipher.Decrypt<string>(encrypted);

                    Assert.AreEqual(keyFileName, decrypted);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetDuplicateWithShouldEncryptIVTest()
        {
            var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName);

            cipher.ShouldEncryptIV = true;
            cipher.Duplicate();
        }

        #region IKeyManagement tests
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ImportSymmetricKeyTest()
        {
            var target = GetCipher() as IKeyManagement;

            Assert.IsNotNull(target);

            var key = new byte[target.ExportSymmetricKey().Length].FillRandom();

            target.ImportSymmetricKey(key);
        }

        [TestMethod]
        public void ExportSymmetricKeyTest()
        {
            var target = GetCipher() as IKeyManagement;

            Assert.IsNotNull(target);

            target.ExportSymmetricKey();
        }

        [TestMethod]
        public void ExportSymmetricKeyAsyncTest()
        {
            var target = GetCipher() as IKeyManagement;

            Assert.IsNotNull(target);

            var keyTask = target.ExportSymmetricKeyAsync();
            var key = keyTask.Result;
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
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

        [TestMethod]
        public void NullEncryptTest()
        {
            using (var target = GetCipher())
            {
                var actual = target.Encrypt(null);

                Assert.IsNull(actual);
            }
        }

        [TestMethod]
        public void NullEncryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var actual = target.Encrypt(null);

                Assert.IsNull(actual);
            }
        }

        [TestMethod]
        public void Length0EncryptTest()
        {
            using (var target = GetCipher())
            {
                var expected = new byte[0];
                var actual = target.Encrypt(new byte[0]);

                Assert.IsFalse(actual.SequenceEqual(expected));
            }
        }

        [TestMethod]
        public void Length0EncryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var expected = new byte[0];
                var actual = target.Encrypt(new byte[0]);

                Assert.IsFalse(actual.SequenceEqual(expected));
            }
        }

        [TestMethod]
        public void EncryptTest()
        {
            using (var target = GetCipher())
            {
                var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                var actual = target.Encrypt(input);

                Assert.IsFalse(actual.SequenceEqual(input));
            }
        }

        [TestMethod]
        public void EncryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                var actual = target.Encrypt(input);

                Assert.IsFalse(actual.SequenceEqual(input));
            }
        }

        [TestMethod]
        public void NullDecryptTest()
        {
            using (var target = GetCipher())
            {
                var actual = target.Decrypt(null);

                Assert.IsNull(actual);
            }
        }

        [TestMethod]
        public void NullDecryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var actual = target.Decrypt(null);

                Assert.IsNull(actual);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Length0DecryptTest()
        {
            using (var target = GetCipher())
            {
                var expected = new byte[0];
                var actual = target.Decrypt(new byte[0]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Length0DecryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var expected = new byte[0];
                var actual = target.Decrypt(new byte[0]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Length6DecryptTest()
        {
            using (var target = GetCipher())
            {
                var actual = target.Decrypt(new byte[] { 8, 0, 0, 0, 0, 0 });
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Length6DecryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var actual = target.Decrypt(new byte[] { 8, 0, 0, 0, 0, 0 });
            }
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void RoundTripEncryptTest()
        {
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            using (var target = GetCipher())
            {
                var encrypted = target.Encrypt(input);

                using (var target1 = GetCipher())
                {
                    var output =  target1.Decrypt(encrypted);

                    Assert.IsTrue(input.SequenceEqual(output));
                }
            }
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void RoundTripEncryptTest64()
        {
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            using (var target = GetCipher(true))
            {
                var encrypted = target.Encrypt(input);

                using (var target1 = GetCipher(true))
                {
                    var output =  target1.Decrypt(encrypted);

                    Assert.IsTrue(input.SequenceEqual(output));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullInputEncryptTest()
        {
            using (var target = GetCipher())
            {
                var input = (Stream)null;
                var output = new MemoryStream();

                target.Encrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullInputEncryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var input = (Stream)null;
                var output = new MemoryStream();

                target.Encrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullOutputEncryptTest()
        {
            using (var target = GetCipher())
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                var output = (Stream)null;

                target.Encrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullOutputEncryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                var output = (Stream)null;

                target.Encrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamReadableOutputEncryptTest()
        {
            using (var target = GetCipher())
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                target.Encrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamReadableOutputEncryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                target.Encrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamWritableInputEncryptTest()
        {
            var fileName = Path.GetRandomFileName();
            try
            {
                using (var target = GetCipher())
                using (var input = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, true);

                    target.Encrypt(input, output);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamWritableInputEncryptTest64()
        {
            var fileName = Path.GetRandomFileName();
            try
            {
                using (var target = GetCipher(true))
                using (var input = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, true);

                    target.Encrypt(input, output);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullInputDecryptTest()
        {
            using (var target = GetCipher())
            {
                var input = (Stream)null;
                var output = new MemoryStream();

                target.Decrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullInputDecryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var input = (Stream)null;
                var output = new MemoryStream();

                target.Decrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullOutputDecryptTest()
        {
            using (var target = GetCipher())
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                var output = (Stream)null;

                target.Decrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullOutputDecryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                var output = (Stream)null;

                target.Decrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamReadableOutputDecryptTest()
        {
            using (var target = GetCipher())
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                target.Decrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamReadableOutputDecryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                target.Decrypt(input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamWritableInputDecryptTest()
        {
            var fileName = Path.GetRandomFileName();
            try
            {
                using (var target = GetCipher())
                using (var input = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                    target.Decrypt(input, output);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamWritableInputDecryptTest64()
        {
            var fileName = Path.GetRandomFileName();
            try
            {
                using (var target = GetCipher(true))
                using (var input = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                    target.Decrypt(input, output);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        void StreamParameterizedEncryptTest(int length)
        {
            using (var target = GetCipher())
            {
                var inputData = new byte[length].FillRandom();
                var input = new MemoryStream(inputData);
                var output = new MemoryStream();

                target.Encrypt(input, output);
                output.Close();

                var outputData = output.ToArray();

                Assert.IsFalse(inputData.SequenceEqual(outputData));
                Assert.IsTrue(inputData.Length < outputData.Length);
            }
        }

        void StreamParameterizedEncryptTest64(int length)
        {
            using (var target = GetCipher(true))
            {
                var inputData = new byte[length].FillRandom();
                var input = new MemoryStream(inputData);
                var output = new MemoryStream();

                target.Encrypt(input, output);
                output.Close();

                var outputData = output.ToArray();

                Assert.IsFalse(inputData.SequenceEqual(outputData));
                Assert.IsTrue(inputData.Length < outputData.Length);
            }
        }

        async Task StreamParameterizedAsyncEncryptTest(int length)
        {
            using (var target = GetCipher())
            {
                var inputData = new byte[length].FillRandom();
                var input = new MemoryStream(inputData);
                var output = new MemoryStream();

                await target.EncryptAsync(input, output);
                output.Close();

                var outputData = output.ToArray();

                Assert.IsFalse(inputData.SequenceEqual(outputData));
                Assert.IsTrue(inputData.Length < outputData.Length);
            }
        }

        async Task StreamParameterizedAsyncEncryptTest64(int length)
        {
            using (var target = GetCipher(true))
            {
                var inputData = new byte[length].FillRandom();
                var input = new MemoryStream(inputData);
                var output = new MemoryStream();

                await target.EncryptAsync(input, output);
                output.Close();

                var outputData = output.ToArray();

                Assert.IsFalse(inputData.SequenceEqual(outputData));
                Assert.IsTrue(inputData.Length < outputData.Length);
            }
        }

        void RoundTripParameterizedEncryptTest(int length)
        {
            byte[] clearData = new byte[length].FillRandom();
            byte[] encryptedData;

            using (var target = GetCipher())
            {
                var input = new MemoryStream(clearData);
                var output = new MemoryStream();

                target.Encrypt(input, output);
                input.Seek(0, SeekOrigin.Begin);
                output.Close();

                encryptedData = output.ToArray();

                Assert.IsFalse(clearData.SequenceEqual(encryptedData));
                Assert.IsTrue(clearData.Length < encryptedData.Length);
            }

            var decrypted = new MemoryStream();

            using (var target1 = GetCipher())
            {
                var input = new MemoryStream(encryptedData);
                var output = new MemoryStream();

                target1.Decrypt(input, output);
                output.Close();

                var decryptedData = output.ToArray();
                Assert.IsTrue(clearData.SequenceEqual(decryptedData));
            }
        }


        void RoundTripParameterizedEncryptTest64(int length)
        {
            byte[] clearData = new byte[length].FillRandom();
            byte[] encryptedData;

            using (var target = GetCipher(true))
            {
                var input = new MemoryStream(clearData);
                var output = new MemoryStream();

                target.Encrypt(input, output);
                input.Seek(0, SeekOrigin.Begin);
                output.Close();

                encryptedData = output.ToArray();

                Assert.IsFalse(clearData.SequenceEqual(encryptedData));
                Assert.IsTrue(clearData.Length < encryptedData.Length);
            }

            var decrypted = new MemoryStream();

            using (var target1 = GetCipher(true))
            {
                var input = new MemoryStream(encryptedData);
                var output = new MemoryStream();

                target1.Decrypt(input, output);
                output.Close();

                var decryptedData = output.ToArray();
                Assert.IsTrue(clearData.SequenceEqual(decryptedData));
            }
        }

        async Task RoundTripParameterizedAsyncEncryptTest(int length)
        {
            byte[] clearData = new byte[length].FillRandom();
            byte[] encryptedData;

            using (var target = GetCipher())
            {
                var input = new MemoryStream(clearData);
                var output = new MemoryStream();

                await target.EncryptAsync(input, output);
                input.Seek(0, SeekOrigin.Begin);
                output.Close();

                encryptedData = output.ToArray();

                Assert.IsFalse(clearData.SequenceEqual(encryptedData));
                Assert.IsTrue(clearData.Length < encryptedData.Length);
            }

            var decrypted = new MemoryStream();

            using (var target1 = GetCipher())
            {
                var input = new MemoryStream(encryptedData);
                var output = new MemoryStream();

                await target1.DecryptAsync(input, output);
                output.Close();

                var decryptedData = output.ToArray();
                Assert.IsTrue(clearData.SequenceEqual(decryptedData));
            }
        }

        async Task RoundTripParameterizedAsyncEncryptTest64(int length)
        {
            byte[] clearData = new byte[length].FillRandom();
            byte[] encryptedData;

            using (var target = GetCipher(true))
            {
                var input = new MemoryStream(clearData);
                var output = new MemoryStream();

                await target.EncryptAsync(input, output);
                input.Seek(0, SeekOrigin.Begin);
                output.Close();

                encryptedData = output.ToArray();

                Assert.IsFalse(clearData.SequenceEqual(encryptedData));
                Assert.IsTrue(clearData.Length < encryptedData.Length);
            }

            var decrypted = new MemoryStream();

            using (var target1 = GetCipher(true))
            {
                var input = new MemoryStream(encryptedData);
                var output = new MemoryStream();

                await target1.DecryptAsync(input, output);
                output.Close();

                var decryptedData = output.ToArray();
                Assert.IsTrue(clearData.SequenceEqual(decryptedData));
            }
        }

        // --------------------------------------

        [TestMethod]
        public void Stream0EncryptTest()
        {
            StreamParameterizedEncryptTest(0);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStream0EncryptTest()
        {
            RoundTripParameterizedEncryptTest(0);
        }

        [TestMethod]
        public void StreamLessThan4kEncryptTest()
        {
            StreamParameterizedEncryptTest(1024);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamLessThan4kEncryptTest()
        {
            RoundTripParameterizedEncryptTest(1024);
        }

        [TestMethod]
        public void Stream4kEncryptTest()
        {
            StreamParameterizedEncryptTest(4096);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStream4kEncryptTest()
        {
            RoundTripParameterizedEncryptTest(4096);
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void StreamNx4kEncryptTest()
        {
            StreamParameterizedEncryptTest(3*4096);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamNx4kEncryptTest()
        {
            RoundTripParameterizedEncryptTest(3*4096);
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void StreamMoreThanNx4kEncryptTest()
        {
            StreamParameterizedEncryptTest(3*4096+734);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamMoreThanNx4kEncryptTest()
        {
            RoundTripParameterizedEncryptTest(3*4096+734);
        }

        // --------------------------------------

        [TestMethod]
        public void Stream0EncryptTest64()
        {
            StreamParameterizedEncryptTest64(0);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStream0EncryptTest64()
        {
            RoundTripParameterizedEncryptTest64(0);
        }

        [TestMethod]
        public void StreamLessThan4kEncryptTest64()
        {
            StreamParameterizedEncryptTest64(1024);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamLessThan4kEncryptTest64()
        {
            RoundTripParameterizedEncryptTest64(1024);
        }

        [TestMethod]
        public void Stream4kEncryptTest64()
        {
            StreamParameterizedEncryptTest64(4096);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStream4kEncryptTest64()
        {
            RoundTripParameterizedEncryptTest64(4096);
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void StreamNx4kEncryptTest64()
        {
            StreamParameterizedEncryptTest64(3*4096);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamNx4kEncryptTest64()
        {
            RoundTripParameterizedEncryptTest64(3*4096);
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void StreamMoreThanNx4kEncryptTest64()
        {
            StreamParameterizedEncryptTest64(3*4096+734);
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamMoreThanNx4kEncryptTest64()
        {
            RoundTripParameterizedEncryptTest64(3*4096+734);
        }

        // --------------------------------------

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullInputAsyncEncryptTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = GetCipher())
                {
                    var input = (Stream)null;
                    var output = new MemoryStream();

                    target.EncryptAsync(input, output).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullOutputAsyncEncryptTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = GetCipher())
                {
                    var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                    var output = (Stream)null;

                    target.EncryptAsync(input, output).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamReadableOutputAsyncEncryptTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = GetCipher())
                {
                    var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                    var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                    target.EncryptAsync(input, output).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamWritableInputAsyncEncryptTest()
        {
            var fileName = Path.GetRandomFileName();
            try
            {
                TestUtilities.AsyncTestWrapper(TestContext, () =>
                {
                    using (var target = GetCipher())
                    using (var input = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, true);

                        target.EncryptAsync(input, output).Wait();
                    }
                });
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullInputAsyncDecryptTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = GetCipher())
                {
                    var input = (Stream)null;
                    var output = new MemoryStream();

                    target.DecryptAsync(input, output).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamNullOutputAsyncDecryptTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = GetCipher())
                {
                    var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                    var output = (Stream)null;

                    target.DecryptAsync(input, output).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamReadableOutputAsyncDecryptTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = GetCipher())
                {
                    var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
                    var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                    target.DecryptAsync(input, output).Wait();
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StreamWritableInputAsyncDecryptTest()
        {
            var fileName = Path.GetRandomFileName();
            try
            {
                TestUtilities.AsyncTestWrapper(TestContext, () =>
                {
                    using (var target = GetCipher())
                    using (var input = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var output = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

                        target.DecryptAsync(input, output).Wait();
                    }
                });
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Length0AsyncDecryptTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = GetCipher())
                {
                    target.DecryptAsync(new MemoryStream(new byte[0]), new MemoryStream()).Wait();
                }
            });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Length6AsyncDecryptTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var target = GetCipher())
                {
                    target.DecryptAsync(new MemoryStream(new byte[] { 8, 0, 0, 0, 0, 0 }), new MemoryStream()).Wait();
                }
            });
        }

        //-----------------------------------------------------

        [TestMethod]
        public void Stream0AsyncEncryptTest()
        {
            StreamParameterizedAsyncEncryptTest(0).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStream0AsyncEncryptTest()
        {
            RoundTripParameterizedAsyncEncryptTest(0).Wait();
        }

        [TestMethod]
        public void StreamLessThan4kAsyncEncryptTest()
        {
            StreamParameterizedAsyncEncryptTest(1024).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamLessThan4kAsyncEncryptTest()
        {
            RoundTripParameterizedAsyncEncryptTest(1024).Wait();
        }

        [TestMethod]
        public void Stream4kAsyncEncryptTest()
        {
            StreamParameterizedAsyncEncryptTest(4096).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStream4kAsyncEncryptTest()
        {
            RoundTripParameterizedAsyncEncryptTest(4096).Wait();
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void EncryptStreamNx4kAsyncTest()
        {
            StreamParameterizedAsyncEncryptTest(3*4096).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamNx4kAsyncEncryptTest()
        {
            RoundTripParameterizedAsyncEncryptTest(3*4096).Wait();
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void StreamMoreThanNx4kAsyncEncryptTest()
        {
            StreamParameterizedAsyncEncryptTest(3*4096+734).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamMoreThanNx4kAsyncEncryptTest()
        {
            RoundTripParameterizedAsyncEncryptTest(3*4096+734).Wait();
        }

        //-----------------------------------------------------

        [TestMethod]
        public void Stream0AsyncEncryptTest64()
        {
            StreamParameterizedAsyncEncryptTest64(0).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStream0AsyncEncryptTest64()
        {
            RoundTripParameterizedAsyncEncryptTest64(0).Wait();
        }

        [TestMethod]
        public void StreamLessThan4kAsyncEncryptTest64()
        {
            StreamParameterizedAsyncEncryptTest64(1024).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamLessThan4kAsyncEncryptTest64()
        {
            RoundTripParameterizedAsyncEncryptTest64(1024).Wait();
        }

        [TestMethod]
        public void Stream4kAsyncEncryptTest64()
        {
            StreamParameterizedAsyncEncryptTest64(4096).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStream4kAsyncEncryptTest64()
        {
            RoundTripParameterizedAsyncEncryptTest64(4096).Wait();
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void EncryptStreamNx4kAsyncTest64()
        {
            StreamParameterizedAsyncEncryptTest64(3*4096).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamNx4kAsyncEncryptTest64()
        {
            RoundTripParameterizedAsyncEncryptTest64(3*4096).Wait();
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void StreamMoreThanNx4kAsyncEncryptTest64()
        {
            StreamParameterizedAsyncEncryptTest64(3*4096+734).Wait();
        }

        [TestMethod]
        [TestCategory("RoundTripTest")]
        public void RoundTripStreamMoreThanNx4kAsyncEncryptTest64()
        {
            RoundTripParameterizedAsyncEncryptTest64(3*4096+734).Wait();
        }
    }
}
