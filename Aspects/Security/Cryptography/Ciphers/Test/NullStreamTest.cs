using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for NullStreamTest
    /// </summary>
    [TestClass]
    public class NullStreamTest
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

        [TestMethod]
        public void ConstructorTest()
        {
            var target = new NullStream();

            Assert.IsTrue(target.CanWrite);
            Assert.IsTrue(target.CanRead);
            Assert.IsFalse(target.CanSeek);
            Assert.AreEqual(0, target.Position);
            Assert.AreEqual(0, target.Length);
        }

        [TestMethod]
        public void PositionTest()
        {
            var target = new NullStream();

            Assert.AreEqual(0, target.Position);

            target.Position = 10;

            Assert.AreEqual(0, target.Position);
        }

        [TestMethod]
        public void SetLengthTest()
        {
            var target = new NullStream();

            Assert.AreEqual(0, target.Length);

            target.SetLength(10);

            Assert.AreEqual(0, target.Length);
        }

        [TestMethod]
        public void ReadTest()
        {
            var target = new NullStream();
            var buffer = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            Assert.AreEqual(0, target.Read(buffer, 0, buffer.Length));
            Assert.IsTrue(buffer.SequenceEqual(new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
        }

#if NET45
        [TestMethod]
        public void ReadAsyncTest()
        {
            var target = new NullStream();
            var buffer = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            Assert.AreEqual(0, target.ReadAsync(buffer, 0, buffer.Length).Result);
            Assert.IsTrue(buffer.SequenceEqual(new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
        } 
#endif

        [TestMethod]
        public void WriteTest()
        {
            var target = new NullStream();
            var buffer = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            Assert.AreEqual(0, target.Position);
            Assert.AreEqual(0, target.Length);

            target.Write(buffer, 0, buffer.Length);

            Assert.AreEqual(0, target.Position);
            Assert.AreEqual(0, target.Length);
        }

#if NET45
        [TestMethod]
        public void WriteAsyncTest()
        {
            var target = new NullStream();
            var buffer = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            Assert.AreEqual(0, target.Position);
            Assert.AreEqual(0, target.Length);

            target.WriteAsync(buffer, 0, buffer.Length).Wait();

            Assert.AreEqual(0, target.Position);
            Assert.AreEqual(0, target.Length);
        } 

        [TestMethod]
        public void CopyToAsyncTest()
        {
            var target = new NullStream();

            target.Write(new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 }, 0, 10);

            using (var stream = new MemoryStream())
            {
                target.CopyToAsync(stream).Wait();

                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(0, stream.Length);
            }
        }
#endif

        [TestMethod]
        public void SeekTest()
        {
            var target = new NullStream();

            Assert.AreEqual(0, target.Position);
            target.Seek(10, SeekOrigin.Begin);
            Assert.AreEqual(0, target.Position);
        }
    }
}
