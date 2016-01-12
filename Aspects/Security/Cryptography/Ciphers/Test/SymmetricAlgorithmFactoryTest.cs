using System;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
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

        #region Additional test attributes
        static UnityServiceLocator _unityServiceLocator;

        public static void InitializeSymmetricNameTest(string name)
        {
            ServiceLocatorWrapper.Reset();
            _unityServiceLocator = new UnityServiceLocator(
                        new UnityContainer()
                            .RegisterInstance<string>(Algorithms.Symmetric.ResolveName, name));
            ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);
        }

        public static void InitializeSymmetricAlgorithmTest()
        {
            ServiceLocatorWrapper.Reset();
            _unityServiceLocator = new UnityServiceLocator(
                        new UnityContainer()
                            .RegisterType<SymmetricAlgorithm, Rijndael>());
            ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);
        }

        public static void CleanupTest()
        {
            ServiceLocator.SetLocatorProvider(null);
            if (_unityServiceLocator != null)
                _unityServiceLocator.Dispose();
            ServiceLocatorWrapper.Reset();
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UninitializedTest()
        {
            try
            {
                var target = new SymmetricAlgorithmFactory();

                target.Create();
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        public void InitializedWithGoodNameTest()
        {
            try
            {
                InitializeSymmetricNameTest("DES");

                var target = new SymmetricAlgorithmFactory();

                target.Initialize("TripleDES");
                var symmetric1 = target.Create();

                Assert.IsNotNull(symmetric1);
                Assert.AreEqual("TripleDES", target.SymmetricAlgorithmName);

                var symmetric2 = target.Create();

                Assert.IsNotNull(symmetric2);
                Assert.AreNotEqual(symmetric1, symmetric2);
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        public void InitializedWithAlgorithmFromDITest()
        {
            try
            {
                InitializeSymmetricAlgorithmTest();

                var target = new SymmetricAlgorithmFactory();

                target.Initialize();
                var symmetric1 = target.Create();

                Assert.IsNotNull(symmetric1);

                var symmetric2 = target.Create();

                Assert.IsNotNull(symmetric2);
                Assert.IsFalse(object.ReferenceEquals(symmetric1, symmetric2));
                Assert.IsInstanceOfType(symmetric1, typeof(Aes));
                Assert.IsInstanceOfType(symmetric2, typeof(Aes));
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        [TestCategory("SlowTest")]
        public void FinalizerTest()
        {
            var target = new WeakReference<SymmetricAlgorithmFactory>(new SymmetricAlgorithmFactory());

            Thread.Sleep(1000);
            GC.Collect();

            SymmetricAlgorithmFactory collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void InitializedWithBadNameTest()
        {
            try
            {
                InitializeSymmetricNameTest("DES");

                var target = new SymmetricAlgorithmFactory();

                target.Initialize("foo");
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        public void InitializedTest1()
        {
            try
            {
                ServiceLocatorWrapper.Reset();
                _unityServiceLocator = new UnityServiceLocator(
                            new UnityContainer()
                                .RegisterInstance<string>(Algorithms.Symmetric.ResolveName, "DES")
                                .RegisterType<SymmetricAlgorithm, TripleDESCryptoServiceProvider>());
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new SymmetricAlgorithmFactory();

                target.Initialize("Rijndael");

                var symmetric = target.Create();

                Assert.IsNotNull(symmetric);
                Assert.IsInstanceOfType(symmetric, typeof(Rijndael));
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        public void InitializedTest2()
        {
            try
            {
                ServiceLocatorWrapper.Reset();
                _unityServiceLocator = new UnityServiceLocator(
                            new UnityContainer()
                                .RegisterInstance<string>(Algorithms.Symmetric.ResolveName, "DES")
                                .RegisterType<SymmetricAlgorithm, TripleDESCryptoServiceProvider>());
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new SymmetricAlgorithmFactory();

                //target.Initialize("Rijndael");
                target.Initialize();

                var symmetric = target.Create();

                Assert.IsNotNull(symmetric);
                Assert.IsInstanceOfType(symmetric, typeof(TripleDES));
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        public void InitializedTest3()
        {
            try
            {
                ServiceLocatorWrapper.Reset();
                _unityServiceLocator = new UnityServiceLocator(
                            new UnityContainer()
                                .RegisterInstance<string>(Algorithms.Symmetric.ResolveName, "DES")
                    /*.RegisterType<Symmetric, TripleDESCryptoServiceProvider>()*/);
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new SymmetricAlgorithmFactory();

                //target.Initialize("Rijndael");
                target.Initialize();

                var symmetric = target.Create();

                Assert.IsNotNull(symmetric);
                Assert.IsInstanceOfType(symmetric, typeof(DES));
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        public void InitializedTest4()
        {
            try
            {
                ServiceLocatorWrapper.Reset();
                _unityServiceLocator = new UnityServiceLocator(
                            new UnityContainer()
                    /*.RegisterInstance<string>(Algorithms.Symmetric.ResolveName, "DES")*/
                    /*.RegisterType<Symmetric, TripleDESCryptoServiceProvider>()*/);
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new SymmetricAlgorithmFactory();

                //target.Initialize("Rijndael");
                target.Initialize();

                var symmetric = target.Create();

                Assert.IsNotNull(symmetric);
                Assert.IsInstanceOfType(symmetric, typeof(Aes));
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        public void InitializedTest5()
        {
            try
            {
                ServiceLocatorWrapper.Reset();
                _unityServiceLocator = new UnityServiceLocator(
                            new UnityContainer()
                    /*.RegisterInstance<string>(Algorithms.Symmetric.ResolveName, "DES")*/
                    /*.RegisterType<Symmetric, TripleDESCryptoServiceProvider>()*/);
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new SymmetricAlgorithmFactory();

                target.Initialize("RC2");

                var symmetric = target.Create();

                Assert.IsNotNull(symmetric);
                Assert.IsInstanceOfType(symmetric, typeof(RC2));
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        public void InitializedWithEmptyNameFromDITest()
        {
            try
            {
                InitializeSymmetricNameTest("");

                var target = new SymmetricAlgorithmFactory();

                target.Initialize();
                var symmetric1 = target.Create();

                Assert.IsNotNull(symmetric1);
                Assert.AreEqual("AESManaged", target.SymmetricAlgorithmName);

                var symmetric2 = target.Create();

                Assert.IsNotNull(symmetric2);
                Assert.AreNotEqual(symmetric1, symmetric2);
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void InitializedWithBadNameFromDISymmetricTest()
        {
            try
            {
                InitializeSymmetricNameTest("foo");

                var target = new SymmetricAlgorithmFactory();

                target.Initialize();
            }
            finally
            {
                CleanupTest();
            }
        }
    }
}
