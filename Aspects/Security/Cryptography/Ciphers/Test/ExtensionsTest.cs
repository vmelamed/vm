using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class ExtensionsTest
    {
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        void TestFillRandom(byte[] array)
        {
            try
            {
                var result = array.FillRandom();

                TestContext.WriteLine("{0}", BitConverter.ToString(result));
                Assert.AreSame(array, result);
            }
            catch (UnitTestAssertException)
            {
                throw;
            }
            catch (Exception x)
            {
                TestContext.WriteLine("{0}", x);
                throw;
            }
        }

        [TestMethod]
        public void ProtectNull()
        {
            byte[] data = null;

            Assert.IsNull(data.Protect());
        }

        [TestMethod]
        public void UnprotectNull()
        {
            byte[] data = null;

            Assert.IsNull(data.Unprotect());
        }

        [TestMethod]
        public void ProtectRoundTrip()
        {
            byte[] data = new byte[20].FillRandom();

            var protectedData = data.Protect();
            var unprotected = protectedData.Unprotect();

            Assert.IsTrue(data.SequenceEqual(unprotected));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FillRandomNullTest()
        {
            TestFillRandom(null);
        }

        [TestMethod]
        public void FillRandom0LengthTest()
        {
            TestFillRandom(new byte[0]);
        }

        [TestMethod]
        public void FillRandomTest()
        {
            TestFillRandom(new byte[16]);
        }

        [TestMethod]
        public void FillRandomIsRandomTest()
        {
            byte[] target1 = new byte[16];
            byte[] target2 = new byte[16];

            target1.FillRandom();
            target2.FillRandom();

            Assert.IsFalse(target1.SequenceEqual(target2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstantTimeEqualsArrayNullTest()
        {
            byte[] array = null;
            byte[] other = new byte[10];

            array.ConstantTimeEquals(other);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstantTimeEqualsOtherNullTest()
        {
            byte[] array = new byte[10];
            byte[] other = null;

            array.ConstantTimeEquals(other);
        }

        [TestMethod]
        public void ConstantTimeEqualsDifferentLengths1Test()
        {
            byte[] array = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            byte[] other = new byte[] { 1, 2, 3, 4, 5, 6, 7, };

            Assert.IsFalse(array.ConstantTimeEquals(other));
        }

        [TestMethod]
        public void ConstantTimeEqualsDifferentLengths2Test()
        {
            byte[] array = new byte[] { 1, 2, 3, 4, 5, 6, 7, };
            byte[] other = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            Assert.IsFalse(array.ConstantTimeEquals(other));
        }

        [TestMethod]
        public void ConstantTimeEqualsEqual0BytesTest()
        {
            byte[] array = new byte[] { };
            byte[] other = new byte[] { };

            Assert.IsTrue(array.ConstantTimeEquals(other));
        }

        [TestMethod]
        public void ConstantTimeEqualsEqual1BytesTest()
        {
            byte[] array = new byte[] { 0 };
            byte[] other = new byte[] { 0 };

            Assert.IsTrue(array.ConstantTimeEquals(other));
        }

        [TestMethod]
        public void ConstantTimeEqualsNotEqual1BytesTest()
        {
            byte[] array = new byte[] { 0 };
            byte[] other = new byte[] { 1 };

            Assert.IsFalse(array.ConstantTimeEquals(other));
        }

        [TestMethod]
        public void ConstantTimeEqualsEqualBytesTest()
        {
            byte[] array = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            byte[] other = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            Assert.IsTrue(array.ConstantTimeEquals(other));
        }

        [TestMethod]
        public void ConstantTimeEqualsDifferentBytesTest()
        {
            byte[] array = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1 };
            byte[] other = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            Assert.IsFalse(array.ConstantTimeEquals(other));
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void ConstantTimeEqualsEqualTimesTest()
        {
            const int length = 10000000;

            byte[] array = new byte[length];
            byte[] other = new byte[length];

            array.FillRandom();
            array.CopyTo(other, 0);

            var sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            array.ConstantTimeEquals(other);
            sw.Stop();

            var timeEquals = (double)sw.Elapsed.TotalMilliseconds;

            array[0] = 0;
            other[0] = 1;

            sw.Reset();
            sw.Start();
            array.ConstantTimeEquals(other);
            sw.Stop();

            var timeNotEqualsFirst = (double)sw.Elapsed.TotalMilliseconds;

            array[0] = 0;
            other[0] = 0;
            array[length-1] = 0;
            other[length-1] = 1;

            sw.Reset();
            sw.Start();
            array.ConstantTimeEquals(other);
            sw.Stop();

            var timeNotEqualsLast = (double)sw.Elapsed.TotalMilliseconds;

            array[0] = 0;
            other = new byte[] { 1 };

            sw.Reset();
            sw.Start();
            array.ConstantTimeEquals(other);
            sw.Stop();

            var timeNotEqualLengthsLast = (double)sw.Elapsed.TotalMilliseconds;

            TestContext.WriteLine("timeEquals/timeNotEqualsFirst => {0}", Math.Abs(timeEquals/timeNotEqualsFirst - 1.0));
            TestContext.WriteLine("timeEquals/timeNotEqualsLast => {0}", Math.Abs(timeEquals/timeNotEqualsLast - 1.0));
            TestContext.WriteLine("timeNotEqualsFirst/timeNotEqualsLast => {0}", Math.Abs(timeNotEqualsFirst/timeNotEqualsLast - 1.0));
            TestContext.WriteLine("timeNotEqualLengthsLast/timeNotEqualsFirst => {0}", Math.Abs(timeNotEqualLengthsLast/timeNotEqualsFirst - 1.0));
            TestContext.WriteLine("timeNotEqualLengthsLast/timeNotEqualsLast => {0}", Math.Abs(timeNotEqualLengthsLast/timeNotEqualsLast - 1.0));

            Assert.IsTrue(Math.Abs(timeEquals/timeNotEqualsFirst - 1.0) < 0.5);
            Assert.IsTrue(Math.Abs(timeEquals/timeNotEqualsLast - 1.0) < 0.5);
            Assert.IsTrue(Math.Abs(timeNotEqualsFirst/timeNotEqualsLast - 1.0) < 0.5);
            Assert.IsTrue(Math.Abs(timeNotEqualLengthsLast/timeNotEqualsFirst - 1.0) < 0.5);
            Assert.IsTrue(Math.Abs(timeNotEqualLengthsLast/timeNotEqualsLast - 1.0) < 0.5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HashAlgorithmNullCertTest()
        {
            X509Certificate2 cert = null;

            cert.HashAlgorithm();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void NotSupportedHashAlgorithm()
        {
            try
            {
                var target = CertificateFactory.GetNotSupportedHashCertificate();

                target.HashAlgorithm();
            }
            catch (InvalidOperationException x)
            {
                if (x.Message.StartsWith("Could not find the certificate"))
                    Assert.Inconclusive("Could not find the certificate - change the parameter in CertificateFactory.GetNotSupportedHashCertificate().");
                else
                    throw;
            }
        }

        ICipherAsync GetCipher()
        {
            return new DpapiCipher();
        }

        IHasherAsync GetHasher()
        {
            return new Hasher();
        }

        [TestMethod]
        public void RoundTripEncryptTextTest()
        {
            ICipherAsync target = GetCipher();
            var testText = "test text";

            var cryptoResult = target.EncryptText(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length > testText.Length*2);

            target = GetCipher();

            var textResult = target.DecryptText(cryptoResult);

            Assert.AreEqual(testText, textResult);
        }

        [TestMethod]
        public void HashNullTextHasherTest()
        {
            IHasherAsync target = GetHasher();

            Assert.IsNull(target.HashText(null));
        }

        [TestMethod]
        public void RoundTripHashTextTest()
        {
            IHasherAsync target = GetHasher();
            var testText = "test text";

            var cryptoResult = target.HashText(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length == 40);

            target.VerifyTextHash(testText, cryptoResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RoundTripHashTextNull2Test()
        {
            IHasherAsync target = GetHasher();
            var testText = "test text";

            var cryptoResult = target.HashText(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length == 40);

            target.VerifyTextHash(testText, null);
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void RoundTripInvalidHashTextTest()
        {
            IHasherAsync target = GetHasher();
            var testText = "test text";

            var cryptoResult = target.HashText(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length > testText.Length*2);

            cryptoResult[5] = (byte)~(cryptoResult[5]);

            target.VerifyTextHash(testText, cryptoResult);
        }

        [TestMethod]
        public void RoundTripHashTest()
        {
            IHasherAsync target = GetHasher();
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            target.VerifyHash(data, hash);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RoundTripHashNull2Test()
        {
            IHasherAsync target = GetHasher();
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            target.VerifyHash(data, null);
        }

        [TestMethod]
        public void RoundTripNullHashTest()
        {
            IHasherAsync target = GetHasher();
            byte[] data = null;

            var hash = target.Hash(data);

            Assert.IsNull(hash);

            target.VerifyHash(data, hash);
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void RoundTripInvalidHashTest()
        {
            IHasherAsync target = GetHasher();
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            unchecked { hash[5]--; };

            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripHashStreamTest()
        {
            IHasherAsync target = GetHasher();
            var data = new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            data.Seek(0, SeekOrigin.Begin);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RoundTripHashStreamNull2Test()
        {
            IHasherAsync target = GetHasher();
            var data = new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            data.Seek(0, SeekOrigin.Begin);
            target.VerifyHash(data, null);
        }

        [TestMethod]
        public void RoundTripNullStreamHashTest()
        {
            IHasherAsync target = GetHasher();
            Stream data = null;

            var hash = target.Hash(data);

            Assert.IsNull(hash);

            target.VerifyHash(data, hash);
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void RoundTripInvalidHashStreamTest()
        {
            IHasherAsync target = GetHasher();
            var data = new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            hash[5] ^= hash[5];

            target.VerifyHash(data, hash);
        }
    }
}
