using System;
using System.Security.Cryptography;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for SymmetricAlgorithmFactoryTest
    /// </summary>
    [TestClass]
    public class SymmetricAlgorithmFactoryTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void UninitializedTest()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            target.Initialize("foo");
            Assert.IsNull(target.Create());
        }

        [TestMethod]
        public void InitializedWithGoodNameTest()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            target.Initialize("TripleDES");
            var symmetric1 = target.Create();

            Assert.IsNotNull(symmetric1);
            Assert.AreEqual("TripleDES", target.SymmetricAlgorithmName);

            var symmetric2 = target.Create();

            Assert.IsNotNull(symmetric2);
            Assert.AreNotEqual(symmetric1, symmetric2);
        }

        [TestMethod]
        public void InitializedWithAlgorithmFromDITest()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            target.Initialize();
            var symmetric1 = target.Create();

            Assert.IsNotNull(symmetric1);

            var symmetric2 = target.Create();

            Assert.IsNotNull(symmetric2);
            Assert.IsFalse(object.ReferenceEquals(symmetric1, symmetric2));
            Assert.IsInstanceOfType(symmetric1, typeof(Aes));
            Assert.IsInstanceOfType(symmetric2, typeof(Aes));
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void FinalizerTest()
        {
            var target = new WeakReference<ISymmetricAlgorithmFactory>(DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>());

            Thread.Sleep(1000);
            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }

        [TestMethod]
        public void InitializedTest1()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            target.Initialize("Rijndael");

            var symmetric = target.Create();

            Assert.IsNotNull(symmetric);
            Assert.IsInstanceOfType(symmetric, typeof(Rijndael));
        }

        [TestMethod]
        public void InitializedTest2()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            target.Initialize("TripleDES");

            var symmetric = target.Create();

            Assert.IsNotNull(symmetric);
            Assert.IsInstanceOfType(symmetric, typeof(TripleDES));
        }

        [TestMethod]
        public void InitializedTest3()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            target.Initialize("DES");

            var symmetric = target.Create();

            Assert.IsNotNull(symmetric);
            Assert.IsInstanceOfType(symmetric, typeof(DES));
        }

        [TestMethod]
        public void InitializedTest4()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            //target.Initialize("Rijndael");
            target.Initialize();

            var symmetric = target.Create();

            Assert.IsNotNull(symmetric);
            Assert.IsInstanceOfType(symmetric, typeof(Aes));
        }

        [TestMethod]
        public void InitializedTest5()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            target.Initialize("RC2");

            var symmetric = target.Create();

            Assert.IsNotNull(symmetric);
            Assert.IsInstanceOfType(symmetric, typeof(RC2));
        }

        [TestMethod]
        public void InitializedWithEmptyNameFromDITest()
        {
            var target = DefaultServices.Resolver.GetInstance<ISymmetricAlgorithmFactory>();

            target.Initialize();
            var symmetric1 = target.Create();

            Assert.IsNotNull(symmetric1);
            Assert.AreEqual("AESManaged", target.SymmetricAlgorithmName);

            var symmetric2 = target.Create();

            Assert.IsNotNull(symmetric2);
            Assert.AreNotEqual(symmetric1, symmetric2);
        }
    }
}
