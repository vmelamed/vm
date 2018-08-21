using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for KeyFileTest
    /// </summary>
    [TestClass]
    [DeploymentItem("..\\..\\Readme.txt")]
    public class KeyFileTest
    {
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

        #region KeyLocationExists tests
        [TestMethod]
        public void KeyLocationExistsTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorage>();

            Assert.IsTrue(target.KeyLocationExists("Readme.txt"));
            Assert.IsFalse(target.KeyLocationExists("DontReadme.txt"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void KeyLocationExistsEmptyTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorage>();

            Assert.IsFalse(target.KeyLocationExists(""));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void KeyLocationExistsBlankTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorage>();

            Assert.IsFalse(target.KeyLocationExists(" \t"));
        }
        #endregion

        #region PutKey tests
        [TestMethod]
        public void PutKeyTest()
        {
            File.Delete("foo.key");

            var target = DefaultServices.Resolver.GetInstance<IKeyStorage>();

            target.PutKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "foo.key");

            Assert.IsTrue(File.Exists("foo.key"));

            File.Delete("foo.key");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PutKeyEmptyLocationTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorage>();

            target.PutKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PutKeyBlankLocationTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorage>();

            target.PutKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, " \t");
        }

        #endregion

        #region PutKeyAsync tests
        [TestMethod]
        public void PutKeyAsyncTest()
        {
            File.Delete("foo.key");

            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            target.PutKeyAsync(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "foo.key").Wait();

            Assert.IsTrue(File.Exists("foo.key"));

            File.Delete("foo.key");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PutKeyNullKeyAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

                target.PutKeyAsync(null, "foo.key").Wait();
                Assert.IsFalse(File.Exists("foo.key"));
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PutKeyNullLocationAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

                target.PutKeyAsync(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PutKeyEmptyLocationAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

                target.PutKeyAsync(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "").Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PutKeyBlankLocationAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

                target.PutKeyAsync(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, " \t").Wait();
            });
        }
        #endregion

        #region GetKey tests
        [TestMethod]
        public void GetKeyTest()
        {
            File.Delete("foo.key");

            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            target.PutKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "foo.key");

            Assert.IsTrue(File.Exists("foo.key"));

            var key = target.GetKey("foo.key");

            Assert.IsNotNull(key);
            Assert.IsTrue(key.SequenceEqual(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));

            File.Delete("foo.key");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetKeyEmptyLocationTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            target.GetKey("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetKeyBlankLocationTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            target.GetKey(" \t");
        }

        #endregion

        #region GetKeyAsync tests
        [TestMethod]
        public void GetKeyAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                File.Delete("foo.key");

                var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

                target.PutKeyAsync(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "foo.key").Wait();

                Assert.IsTrue(File.Exists("foo.key"));

                var key = target.GetKeyAsync("foo.key").Result;

                Assert.IsNotNull(key);
                Assert.IsTrue(key.SequenceEqual(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));

                File.Delete("foo.key");
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetKeyNullLocationAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

                target.GetKeyAsync(null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetKeyEmptyLocationAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

                target.GetKeyAsync("").Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetKeyBlankLocationAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

                target.GetKeyAsync(" \t").Wait();
            });
        }
        #endregion

        #region DeleteKey tests
        [TestMethod]
        public void DeleteKeyLocationTest()
        {
            File.Delete("foo.key");

            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            target.PutKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "foo.key");

            Assert.IsTrue(File.Exists("foo.key"));

            target.DeleteKeyLocation("foo.key");

            Assert.IsFalse(File.Exists("foo.key"));
        }

        [TestMethod]
        public void DeleteKeyLocationNonExistentTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            Assert.IsFalse(File.Exists("foo.key"));

            target.DeleteKeyLocation("foo.key");

            Assert.IsFalse(File.Exists("foo.key"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteKeyLocationNullTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            target.DeleteKeyLocation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteKeyLocationEmptyTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            target.DeleteKeyLocation("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteKeyLocationBlankTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyStorageTasks>();

            target.DeleteKeyLocation(" \t");
        }
        #endregion
    }
}
