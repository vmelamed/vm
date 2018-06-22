using System;
using System.Security.Cryptography;

using CommonServiceLocator;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Unity;
using Unity.ServiceLocation;

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

        #region Additional test attributes
        static UnityServiceLocator _unityServiceLocator;

        public static void InitializeHashNameTest(string name)
        {
            ServiceLocatorWrapper.Reset();
            _unityServiceLocator = new UnityServiceLocator(
                        new UnityContainer()
                            .RegisterInstance<string>(Algorithms.Hash.ResolveName, name));
            ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);
        }

        public static void InitializeHashAlgorithmTest()
        {
            ServiceLocatorWrapper.Reset();
            _unityServiceLocator = new UnityServiceLocator(
                        new UnityContainer()
                            .RegisterType<HashAlgorithm, MD5Cng>());
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
                var target = new HashAlgorithmFactory();

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
                InitializeHashNameTest("MD5");

                var target = new HashAlgorithmFactory();

                target.Initialize("SHA1");
                var hasher1 = target.Create();

                Assert.IsNotNull(hasher1);
                Assert.AreEqual("SHA1", target.HashAlgorithmName);

                var hasher2 = target.Create();

                Assert.IsNotNull(hasher2);
                Assert.AreNotEqual(hasher1, hasher2);
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void InitializedWithBadNameTest()
        {
            try
            {
                InitializeHashNameTest("MD5");

                var target = new HashAlgorithmFactory();

                target.Initialize("SHA123");
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
                InitializeHashAlgorithmTest();

                var target = new HashAlgorithmFactory();

                target.Initialize();
                var hasher1 = target.Create();

                Assert.IsNotNull(hasher1);

                var hasher2 = target.Create();

                Assert.IsNotNull(hasher2);
                Assert.IsFalse(object.ReferenceEquals(hasher1, hasher2));
                Assert.IsInstanceOfType(hasher1, typeof(MD5));
                Assert.IsInstanceOfType(hasher2, typeof(MD5));
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
                                .RegisterInstance<string>(Algorithms.Hash.ResolveName, "SHA1")
                                .RegisterType<HashAlgorithm, SHA512Cng>());
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new HashAlgorithmFactory();

                target.Initialize("SHA384");

                var hasher = target.Create();

                Assert.IsNotNull(hasher);
                Assert.IsInstanceOfType(hasher, typeof(SHA384));
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
                                .RegisterInstance<string>(Algorithms.Hash.ResolveName, "SHA1")
                                .RegisterType<HashAlgorithm, SHA512Cng>());
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new HashAlgorithmFactory();

                //target.Initialize("SHA384");
                target.Initialize();

                var hasher = target.Create();

                Assert.IsNotNull(hasher);
                Assert.IsInstanceOfType(hasher, typeof(SHA512));
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
                                .RegisterInstance<string>(Algorithms.Hash.ResolveName, "SHA1")
                    /*.RegisterType<HashAlgorithm, SHA512Cng>()*/);
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new HashAlgorithmFactory();

                //target.Initialize("SHA384");
                target.Initialize();

                var hasher = target.Create();

                Assert.IsNotNull(hasher);
                Assert.IsInstanceOfType(hasher, typeof(SHA1));
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
                    /*.RegisterInstance<string>(Algorithms.Hash.ResolveName, "SHA1")*/
                    /*.RegisterType<HashAlgorithm, SHA512Cng>()*/);
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new HashAlgorithmFactory();

                //target.Initialize("SHA384");
                target.Initialize();

                var hasher = target.Create();

                Assert.IsNotNull(hasher);
                Assert.IsInstanceOfType(hasher, typeof(SHA256));
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
                    /*.RegisterInstance<string>(Algorithms.Hash.ResolveName, "SHA1")*/
                    /*.RegisterType<HashAlgorithm, SHA512Cng>()*/);
                ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

                var target = new HashAlgorithmFactory();

                target.Initialize("SHA384");

                var hasher = target.Create();

                Assert.IsNotNull(hasher);
                Assert.IsInstanceOfType(hasher, typeof(SHA384));
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
                InitializeHashNameTest("");

                var target = new HashAlgorithmFactory();

                target.Initialize();
                var hasher1 = target.Create();

                Assert.IsNotNull(hasher1);
                Assert.AreEqual("SHA256", target.HashAlgorithmName);

                var hasher2 = target.Create();

                Assert.IsNotNull(hasher2);
                Assert.AreNotEqual(hasher1, hasher2);
            }
            finally
            {
                CleanupTest();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void InitializedWithBadNameFromDIHashTest()
        {
            try
            {
                InitializeHashNameTest("SHA222");

                var target = new HashAlgorithmFactory();

                target.Initialize();
            }
            finally
            {
                CleanupTest();
            }
        }
    }
}
