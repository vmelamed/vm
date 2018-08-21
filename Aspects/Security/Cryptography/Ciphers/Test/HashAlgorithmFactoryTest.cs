using System;
using System.Security.Cryptography;

using CommonServiceLocator;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for HasherBadNameTest
    /// </summary>
    [TestClass]
    public class HashAlgorithmFactoryTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void UninitializedTest()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Create();
        }

        [TestMethod]
        public void InitializedWithGoodNameTest()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize("SHA1");
            var hasher1 = target.Create();

            Assert.IsNotNull(hasher1);
            Assert.AreEqual("SHA1", target.HashAlgorithmName);

            var hasher2 = target.Create();

            Assert.IsNotNull(hasher2);
            Assert.AreNotEqual(hasher1, hasher2);
        }

        [TestMethod]
        public void InitializedWithAlgorithmFromDITest()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize("MD5");
            var hasher1 = target.Create();

            Assert.IsNotNull(hasher1);

            var hasher2 = target.Create();

            Assert.IsNotNull(hasher2);
            Assert.IsFalse(object.ReferenceEquals(hasher1, hasher2));
            Assert.IsInstanceOfType(hasher1, typeof(MD5));
            Assert.IsInstanceOfType(hasher2, typeof(MD5));
        }

        [TestMethod]
        public void InitializedTest1()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize("SHA384");

            var hasher = target.Create();

            Assert.IsNotNull(hasher);
            Assert.IsInstanceOfType(hasher, typeof(SHA384));
        }

        [TestMethod]
        public void InitializedTest2()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize("SHA512");

            var hasher = target.Create();

            Assert.IsNotNull(hasher);
            Assert.IsInstanceOfType(hasher, typeof(SHA512));
        }

        [TestMethod]
        public void InitializedTest3()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize("SHA1");

            var hasher = target.Create();

            Assert.IsNotNull(hasher);
            Assert.IsInstanceOfType(hasher, typeof(SHA1));
        }

        [TestMethod]
        public void InitializedTest4()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize("SHA256");

            var hasher = target.Create();

            Assert.IsNotNull(hasher);
            Assert.IsInstanceOfType(hasher, typeof(SHA256));
        }

        [TestMethod]
        public void InitializedTest5()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize("SHA384");

            var hasher = target.Create();

            Assert.IsNotNull(hasher);
            Assert.IsInstanceOfType(hasher, typeof(SHA384));
        }

        [TestMethod]
        public void InitializedWithEmptyNameFromDITest()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize();
            var hasher1 = target.Create();

            Assert.IsNotNull(hasher1);
            Assert.AreEqual("SHA256", target.HashAlgorithmName);

            var hasher2 = target.Create();

            Assert.IsNotNull(hasher2);
            Assert.AreNotEqual(hasher1, hasher2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitializedWithBadNameHashTest()
        {
            var target = new DefaultServices.HashAlgorithmFactory();

            target.Initialize("SHA222");
        }
    }
}
