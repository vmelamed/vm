using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for IHasherExtensionsTest
    /// </summary>
    [TestClass]
    public class IHasherExtensionsTest
    {
        public IHasherExtensionsTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        IHasherTasks GetHasher() => new Hasher();

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

#pragma warning disable 618
        #region Obsolete methods
        [TestMethod]
        public void HashNullTextHasherTest()
        {
            IHasherTasks target = GetHasher();

            Assert.IsNull(target.HashText(null));
        }

        [TestMethod]
        public void RoundTripHashTextTest()
        {
            IHasherTasks target = GetHasher();
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
            IHasherTasks target = GetHasher();
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
            IHasherTasks target = GetHasher();
            var testText = "test text";

            var cryptoResult = target.HashText(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length > testText.Length * 2);

            cryptoResult[5] = (byte)~(cryptoResult[5]);

            target.VerifyTextHash(testText, cryptoResult);
        }
        #endregion
#pragma warning restore 618

        [TestMethod]
        public void RoundTripHashTest()
        {
            IHasherTasks target = GetHasher();
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
            IHasherTasks target = GetHasher();
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            target.VerifyHash(data, null);
        }

        [TestMethod]
        public void RoundTripNullHashTest()
        {
            IHasherTasks target = GetHasher();
            byte[] data = null;

            var hash = target.Hash(data);

            Assert.IsNull(hash);

            target.VerifyHash(data, hash);
        }
        [TestMethod]
        public void RoundTripHashEmptyArrayTest()
        {
            IHasherTasks target = GetHasher();

            var data = new byte[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void RoundTripInvalidHashTest()
        {
            IHasherTasks target = GetHasher();
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            unchecked { hash[5]--; };

            target.VerifyHash(data, hash);
        }

        #region Hash stream tests
        [TestMethod]
        public void RoundTripHashStreamTest()
        {
            IHasherTasks target = GetHasher();
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
            IHasherTasks target = GetHasher();
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
            IHasherTasks target = GetHasher();
            Stream data = null;

            var hash = target.Hash(data);

            Assert.IsNull(hash);

            target.VerifyHash(data, hash);
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void RoundTripInvalidHashStreamTest()
        {
            IHasherTasks target = GetHasher();
            var data = new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);

            hash[5] ^= hash[5];

            target.VerifyHash(data, hash);
        }
        #endregion

        #region bool tests
        [TestMethod]
        public void RoundTripBoolTrueTest()
        {
            var target = GetHasher();
            var data = true;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripBoolFalseTest()
        {
            var target = GetHasher();
            var data = false;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripBoolArrayTest()
        {
            var target = GetHasher();
            var data = new bool[] { true, false, true, true, false };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyBoolArrayTest()
        {
            var target = GetHasher();
            var data = new bool[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullBoolArrayTest()
        {
            var target = GetHasher();
            bool[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region char tests
        [TestMethod]
        public void RoundTripCharNon0Test()
        {
            var target = GetHasher();
            var data = '\x0';
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripChar0Test()
        {
            var target = GetHasher();
            var data = 'Б';
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripCharArrayTest()
        {
            var target = GetHasher();
            var data = new char[] { '\x0', 'a', 'b', 'з', 'ю' };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyCharArrayTest()
        {
            var target = GetHasher();
            var data = new char[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullCharArrayTest()
        {
            var target = GetHasher();
            char[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region sbyte tests
        [TestMethod]
        public void RoundTripSByteNon0Test()
        {
            var target = GetHasher();
            var data = 0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripSByte0Test()
        {
            var target = GetHasher();
            var data = 127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripSByteArrayTest()
        {
            var target = GetHasher();
            var data = new sbyte[] { 0, 1, -1, 127, -128 };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptySByteArrayTest()
        {
            var target = GetHasher();
            var data = new sbyte[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullSByteArrayTest()
        {
            var target = GetHasher();
            sbyte[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region byte tests
        [TestMethod]
        public void RoundTripByteNon0Test()
        {
            var target = GetHasher();
            var data = 0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripByte0Test()
        {
            var target = GetHasher();
            var data = 127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripByteArrayTest()
        {
            var target = GetHasher();
            var data = new byte[] { 0, 1, 2, 254, 255 };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyByteArrayTest()
        {
            var target = GetHasher();
            var data = new byte[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullByteArrayTest()
        {
            var target = GetHasher();
            byte[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region short tests
        [TestMethod]
        public void RoundTripShortNon0Test()
        {
            var target = GetHasher();
            var data = 0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripShort0Test()
        {
            var target = GetHasher();
            var data = 127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripShortArrayTest()
        {
            var target = GetHasher();
            var data = new short[] { 0, 1, -2, 32767, -32768 };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyShortArrayTest()
        {
            var target = GetHasher();
            var data = new short[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullShortArrayTest()
        {
            var target = GetHasher();
            short[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region ushort tests
        [TestMethod]
        public void RoundTripUShortNon0Test()
        {
            var target = GetHasher();
            var data = 0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripUShort0Test()
        {
            var target = GetHasher();
            var data = 127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripUShortArrayTest()
        {
            var target = GetHasher();
            var data = new ushort[] { 0, 1, 2, 32767, 65535 };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyUShortArrayTest()
        {
            var target = GetHasher();
            var data = new ushort[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullUShortArrayTest()
        {
            var target = GetHasher();
            ushort[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region int tests
        [TestMethod]
        public void RoundTripIntNon0Test()
        {
            var target = GetHasher();
            var data = 0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripInt0Test()
        {
            var target = GetHasher();
            var data = 127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripIntArrayTest()
        {
            var target = GetHasher();
            var data = new int[] { 0, 1, -2, 32767, -32768 };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyIntArrayTest()
        {
            var target = GetHasher();
            var data = new int[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullIntArrayTest()
        {
            var target = GetHasher();
            int[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region uint tests
        [TestMethod]
        public void RoundTripUIntNon0Test()
        {
            var target = GetHasher();
            var data = 0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripUInt0Test()
        {
            var target = GetHasher();
            var data = 127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripUIntArrayTest()
        {
            var target = GetHasher();
            var data = new uint[] { 0, 1, 2, 32767, 65535 };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyUIntArrayTest()
        {
            var target = GetHasher();
            var data = new uint[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullUIntArrayTest()
        {
            var target = GetHasher();
            uint[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region long tests
        [TestMethod]
        public void RoundTripLongNon0Test()
        {
            var target = GetHasher();
            var data = 0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripLong0Test()
        {
            var target = GetHasher();
            var data = 127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripLongArrayTest()
        {
            var target = GetHasher();
            var data = new long[] { 0L, 1L, -2L, 32767L, -32768L };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyLongArrayTest()
        {
            var target = GetHasher();
            var data = new long[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullLongArrayTest()
        {
            var target = GetHasher();
            long[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region ulong tests
        [TestMethod]
        public void RoundTripULongNon0Test()
        {
            var target = GetHasher();
            var data = 0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripULong0Test()
        {
            var target = GetHasher();
            var data = 127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripULongArrayTest()
        {
            var target = GetHasher();
            var data = new ulong[] { 0, 1, 2, 32767, 65535 };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyULongArrayTest()
        {
            var target = GetHasher();
            var data = new ulong[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullULongArrayTest()
        {
            var target = GetHasher();
            ulong[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region float tests
        [TestMethod]
        public void RoundTripFloatNon0Test()
        {
            var target = GetHasher();
            var data = 0.0f;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripFloat0Test()
        {
            var target = GetHasher();
            var data = -127.127f;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripFloatArrayTest()
        {
            var target = GetHasher();
            var data = new float[] { 0.0f, 1.1f, -2.2f, 32767.3f, -32768.098765f };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyFloatArrayTest()
        {
            var target = GetHasher();
            var data = new float[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullFloatArrayTest()
        {
            var target = GetHasher();
            float[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region double tests
        [TestMethod]
        public void RoundTripDoubleNon0Test()
        {
            var target = GetHasher();
            var data = 0.0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripDouble0Test()
        {
            var target = GetHasher();
            var data = -127.127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripDoubleArrayTest()
        {
            var target = GetHasher();
            var data = new double[] { 0.0, 1.1, -2.2, 32767.3, -32768.098765 };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyDoubleArrayTest()
        {
            var target = GetHasher();
            var data = new double[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullDoubleArrayTest()
        {
            var target = GetHasher();
            double[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region decimal tests
        [TestMethod]
        public void RoundTripDecimalNon0Test()
        {
            var target = GetHasher();
            var data = 0.0;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripDecimal0Test()
        {
            var target = GetHasher();
            var data = -127.127;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripDecimalArrayTest()
        {
            var target = GetHasher();
            var data = new decimal[] { 0.0m, 1.1m, -2.2m, 32767.3m, -32768.098765m };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyDecimalArrayTest()
        {
            var target = GetHasher();
            var data = new decimal[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullDecimalArrayTest()
        {
            var target = GetHasher();
            decimal[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region Guid tests
        [TestMethod]
        public void RoundTripGuid0Test()
        {
            var target = GetHasher();
            var data = Guid.Empty;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripGuidNon0Test()
        {
            var target = GetHasher();
            var data = Guid.NewGuid();
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripGuidArrayTest()
        {
            var target = GetHasher();
            var data = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyGuidArrayTest()
        {
            var target = GetHasher();
            var data = new Guid[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullGuidArrayTest()
        {
            var target = GetHasher();
            Guid[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion

        #region DateTime tests
        [TestMethod]
        public void RoundTripDateTimeNon0Test()
        {
            var target = GetHasher();
            var data = DateTime.MinValue;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripDateTime0Test()
        {
            var target = GetHasher();
            var data = DateTime.MaxValue;
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripDateTimeArrayTest()
        {
            var target = GetHasher();
            var data = new DateTime[] { DateTime.MinValue, DateTime.Now, DateTime.UtcNow, DateTime.MaxValue };
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripEmptyDateTimeArrayTest()
        {
            var target = GetHasher();
            var data = new DateTime[0];
            var hash = target.Hash(data);

            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length == 40);
            target.VerifyHash(data, hash);
        }

        [TestMethod]
        public void RoundTripNullDateTimeArrayTest()
        {
            var target = GetHasher();
            DateTime[] data = null;
            var hash = target.Hash(data);

            Assert.IsNull(hash);
            target.VerifyHash(data, hash);
        }
        #endregion
    }
}
