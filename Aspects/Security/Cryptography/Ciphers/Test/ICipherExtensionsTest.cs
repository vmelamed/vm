using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for ICipherExtensionsTest
    /// </summary>
    [TestClass]
    public class ICipherExtensionsTest
    {
        public ICipherExtensionsTest()
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

        ICipherAsync GetCipher()
        {
            return new DpapiCipher();
        }

        #region Encrypt strings
#pragma warning disable 618
        [TestMethod]
        public void RoundTripEncryptTextTest()
        {
            ICipherAsync target = GetCipher();
            var testText = "test text";

            var cryptoResult = target.EncryptText(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length > testText.Length * 2);

            target = GetCipher();

            var textResult = target.DecryptText(cryptoResult);

            Assert.AreEqual(testText, textResult);
        }
#pragma warning restore 618

        [TestMethod]
        public void RoundTripEncryptText64Test()
        {
            ICipherAsync target = GetCipher();
            var testText = "test text";

            var cryptoResult = target.EncryptText64(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length > testText.Length * 2);

            target = GetCipher();

            var textResult = target.DecryptText64(cryptoResult);

            Assert.AreEqual(testText, textResult);
        }

        [TestMethod]
        public void RoundTripEncryptStringTest()
        {
            ICipherAsync target = GetCipher();
            var testText = "test text";

            var cryptoResult = target.Encrypt(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length > testText.Length * 2);

            target = GetCipher();

            var textResult = target.DecryptString(cryptoResult);

            Assert.AreEqual(testText, textResult);
        }

        [TestMethod]
        public void RoundTripEncryptEmptyStringTest()
        {
            ICipherAsync target = GetCipher();
            var testText = string.Empty;

            var cryptoResult = target.Encrypt(testText);

            Assert.IsNotNull(cryptoResult);
            Assert.IsTrue(cryptoResult.Length > testText.Length * 2);

            target = GetCipher();

            var textResult = target.DecryptString(cryptoResult);

            Assert.AreEqual(testText, textResult);
        }

        [TestMethod]
        public void RoundTripEncryptNullStringTest()
        {
            ICipherAsync target = GetCipher();
            string testText = null;

            var cryptoResult = target.Encrypt(testText);

            Assert.IsNull(cryptoResult);

            target = GetCipher();

            var textResult = target.DecryptString(cryptoResult);

            Assert.AreEqual(testText, textResult);
        }
        #endregion

        #region Encrypt bool
        [TestMethod]
        public void EncryptBooleanRoundTripTest()
        {
            var target = GetCipher();
            var expected = true;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptBoolean(cryptoText);

            Assert.AreEqual(expected, actual);

            expected = false;

            cryptoText = target.Encrypt(expected);
            actual = target.DecryptBoolean(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptBooleanArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new bool[] { true, false, true, true, false };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptBooleanArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptEmptyBooleanArrayRoundTripTest()
        {
            var target = GetCipher();
            bool[] expected = new bool[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptBooleanArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullBooleanArrayRoundTripTest()
        {
            var target = GetCipher();
            bool[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptBooleanArray(cryptoText);

            Assert.IsNull(actual);
        }
        #endregion

        #region Encrypt char
        [TestMethod]
        public void EncryptCharRoundTripTest()
        {
            var target = GetCipher();
            var expected = 'Ю';

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptChar(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyCharArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new char[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptCharArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullCharArrayRoundTripTest()
        {
            var target = GetCipher();
            char[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptCharArray(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptCharArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new char[] { 'A', 'B', 'C', 'Б', 'Ч', 'Й' };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptCharArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt sbyte
        [TestMethod]
        public void EncryptSByteRoundTripTest()
        {
            var target = GetCipher();
            sbyte expected = -3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSByte(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptySByteArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new sbyte[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSByteArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullSByteArrayRoundTripTest()
        {
            var target = GetCipher();
            sbyte[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSByteArray(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptSByteArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new sbyte[] { 0, 1, 2, 127, -1, -2, -128 };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSByteArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt byte
        [TestMethod]
        public void EncryptByteRoundTripTest()
        {
            var target = GetCipher();
            sbyte expected = -3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSByte(cryptoText);

            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Encrypt short
        [TestMethod]
        public void EncryptInt16RoundTripTest()
        {
            var target = GetCipher();
            short expected = -3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt16(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyInt16ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new short[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt16Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullInt16ArrayRoundTripTest()
        {
            var target = GetCipher();
            short[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt16Array(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptInt16ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new short[] { 0, 1, 2, 127, -1, -2, -128 };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt16Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt ushort
        [TestMethod]
        public void EncryptUInt16RoundTripTest()
        {
            var target = GetCipher();
            ushort expected = 3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt16(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyUInt16ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new ushort[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt16Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullUInt16ArrayRoundTripTest()
        {
            var target = GetCipher();
            ushort[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt16Array(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptUInt16ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new ushort[] { 0, 1, 2, 127, 32565 };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt16Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt int
        [TestMethod]
        public void EncryptInt32RoundTripTest()
        {
            var target = GetCipher();
            int expected = 3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt32(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyInt32ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new int[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt32Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullInt32ArrayRoundTripTest()
        {
            var target = GetCipher();
            int[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt32Array(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptInt32ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new int[] { 0, 1, 2, 127, 32565 };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt32Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt uint
        [TestMethod]
        public void EncryptUInt32RoundTripTest()
        {
            var target = GetCipher();
            uint expected = 3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt32(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyUUInt32ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new uint[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt32Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullUUInt32ArrayRoundTripTest()
        {
            var target = GetCipher();
            uint[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt32Array(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptUUInt32ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new uint[] { 0, 1, 2, 127, 32565 };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt32Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt long
        [TestMethod]
        public void EncryptInt64RoundTripTest()
        {
            var target = GetCipher();
            long expected = 3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt64(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyInt64ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new long[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt64Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullInt64ArrayRoundTripTest()
        {
            var target = GetCipher();
            long[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt64Array(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptInt64ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new long[] { 0, 1, 2, 127, 32565 };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptInt64Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt ulong
        [TestMethod]
        public void EncryptUInt64RoundTripTest()
        {
            var target = GetCipher();
            ulong expected = 3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt64(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyUInt64ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new ulong[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt64Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullUInt64ArrayRoundTripTest()
        {
            var target = GetCipher();
            ulong[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt64Array(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptUInt64ArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new ulong[] { 0, 1, 2, 127, 32565 };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptUInt64Array(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt decimal
        [TestMethod]
        public void EncryptDecimalRoundTripTest()
        {
            var target = GetCipher();
            decimal expected = 3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDecimal(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyDecimalArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new decimal[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDecimalArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullDecimalArrayRoundTripTest()
        {
            var target = GetCipher();
            decimal[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDecimalArray(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptDecimalArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new decimal[] { 0m, 1.0m, 2.123456m, -127.789456123m, 32565123.123456789m };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDecimalArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt Single
        [TestMethod]
        public void EncryptSingleRoundTripTest()
        {
            var target = GetCipher();
            Single expected = 3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSingle(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptySingleArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new Single[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSingleArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullSingleArrayRoundTripTest()
        {
            var target = GetCipher();
            Single[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSingleArray(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptSingleArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new Single[] { 0, 1.0f, 2.123456f, -127.789456123f, 32565123.123456789f };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptSingleArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt Double
        [TestMethod]
        public void EncryptDoubleRoundTripTest()
        {
            var target = GetCipher();
            Double expected = 3;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDouble(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyDoubleArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new Double[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDoubleArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullDoubleArrayRoundTripTest()
        {
            var target = GetCipher();
            Double[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDoubleArray(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptDoubleArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new Double[] { 0, 1.0, 2.123456, -127.789456123, 32565123.123456789 };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDoubleArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion

        #region Encrypt Guid
        [TestMethod]
        public void EncryptDateTimeRoundTripTest()
        {
            var target = GetCipher();
            DateTime expected = new DateTime(3);

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDateTime(cryptoText);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptEmptyDateTimeArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new DateTime[0];

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDateTimeArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void EncryptNullDateTimeArrayRoundTripTest()
        {
            var target = GetCipher();
            DateTime[] expected = null;

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDateTimeArray(cryptoText);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EncryptDateTimeArrayRoundTripTest()
        {
            var target = GetCipher();
            var expected = new DateTime[] { new DateTime(0), new DateTime(1), new DateTime(2), new DateTime(127), new DateTime(32565) };

            var cryptoText = target.Encrypt(expected);
            var actual = target.DecryptDateTimeArray(cryptoText);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
        #endregion
    }
}
