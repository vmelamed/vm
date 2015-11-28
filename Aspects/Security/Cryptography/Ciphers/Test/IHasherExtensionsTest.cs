using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        IHasherAsync GetHasher()
        {
            return new Hasher();
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

#pragma warning disable 618
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
            Assert.IsTrue(cryptoResult.Length > testText.Length * 2);

            cryptoResult[5] = (byte)~(cryptoResult[5]);

            target.VerifyTextHash(testText, cryptoResult);
        }
#pragma warning restore 618

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
