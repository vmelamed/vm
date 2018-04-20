using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Unity;
using Unity.Lifetime;
using Unity.Registration;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Summary description for DIContainerExtensionsTests
    /// </summary>
    [TestClass]
    [DeploymentItem(".\\test.config")]
    [DeploymentItem(".\\testWithError.config")]
    public class DIContainerGenericExtensions2Tests
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

        #region Register type
        #region IUnityContainer RegisterTypeIfNot(this IUnityContainer container,Type type,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterTypeIfNot<TestTargetFromCode>(regs);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNullInjectionMembers()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, (InjectionMember[])null);
            }
        }

        [TestMethod]
        public void RegisterUnregisteredTypeIfNotUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs);
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            var t = DIContainer.Root.Resolve<TestTargetFromCode>();

            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterRegisteredTypeIfNot()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterType<TestTargetFromCode>(new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            var t1 = DIContainer.Root.Resolve<TestTargetFromCode>();
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs);
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>());
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            var t2 = DIContainer.Root.Resolve<TestTargetFromCode>();
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterTypeIfNot(this IUnityContainer container,Type type,LifetimeManager lifetimeManager,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotLifetimeNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterTypeIfNot<TestTargetFromCode>(regs, new TransientLifetimeManager());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotLifetimeNullInjectionMembers()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, new TransientLifetimeManager(), (InjectionMember[])null);
            }
        }

        [TestMethod]
        public void RegisterUnregisteredTypeIfNotLifetimeUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, new TransientLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            var t = DIContainer.Root.Resolve<TestTargetFromCode>();

            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterRegisteredTypeIfNotLifetime()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterType<TestTargetFromCode>(new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            var t1 = DIContainer.Root.Resolve<TestTargetFromCode>();
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, new TransientLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            var t2 = DIContainer.Root.Resolve<TestTargetFromCode>();
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterTypeIfNot(this IUnityContainer container,Type type,string name,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNameNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterTypeIfNot<TestTargetFromCode>(regs, "test");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNameNullInjectionMembers()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, "test", (InjectionMember[])null);
            }
        }

        [TestMethod]
        public void RegisterUnregisteredTypeIfNotNameUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, "test");
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));

            var t = DIContainer.Root.Resolve<TestTargetFromCode>("test");

            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterRegisteredTypeIfNotName()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterType<TestTargetFromCode>("test", new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));

            var t1 = DIContainer.Root.Resolve<TestTargetFromCode>("test");
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, "test");
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));

            var t2 = DIContainer.Root.Resolve<TestTargetFromCode>("test");
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterTypeIfNot(this IUnityContainer container,Type type,string name,LifetimeManager lifetimeManager,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNameLifetimeNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterTypeIfNot<TestTargetFromCode>(regs, "test", new TransientLifetimeManager());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNameLifetimeNullInjectionMembers()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, "test", new TransientLifetimeManager(), (InjectionMember[])null);
            }
        }

        [TestMethod]
        public void RegisterUnregisteredTypeIfNotNameLifetimeUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, "test", new TransientLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));

            var t = DIContainer.Root.Resolve<TestTargetFromCode>("test");

            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterRegisteredTypeIfNotNameLifetime()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterType<TestTargetFromCode>("test", new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));

            var t1 = DIContainer.Root.Resolve<TestTargetFromCode>("test");
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<TestTargetFromCode>(regs, "test", new TransientLifetimeManager());
            }

            // Assert
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));
            }

            var t2 = DIContainer.Root.Resolve<TestTargetFromCode>("test");
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion
        #endregion

        #region Register from type-1 to type-2
        #region IUnityContainer RegisterFromTypeToTypeIfNot(this IUnityContainer container,Type typeFrom,Type typeTo,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNullInjectionMembers()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, (InjectionMember[])null);
            }
        }

        [TestMethod]
        public void RegisterFromTypeToUnregisteredTypeIfNotUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs);
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t = DIContainer.Root.Resolve<ITestTarget>();

            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterFromTypeToRegisteredTypeIfNot()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterType<ITestTarget, TestTargetFromCode>(new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t1 = DIContainer.Root.Resolve<ITestTarget>();
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs);
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t2 = DIContainer.Root.Resolve<ITestTarget>();
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterTypeIfNot(this IUnityContainer container,Type typeFrom,Type typeTo,LifetimeManager lifetimeManager,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotLifetimeNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, new TransientLifetimeManager());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotLifetimeNullInjectionMembers()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, new TransientLifetimeManager(), (InjectionMember[])null);
            }
        }

        [TestMethod]
        public void RegisterFromTypeToUnregisteredTypeIfNotLifetimeUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, new TransientLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t = DIContainer.Root.Resolve<ITestTarget>();

            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterFromTypeToRegisteredTypeIfNotLifetime()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterType<ITestTarget, TestTargetFromCode>(new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t1 = DIContainer.Root.Resolve<ITestTarget>();
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, new TransientLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t2 = DIContainer.Root.Resolve<ITestTarget>();
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterTypeIfNot(this IUnityContainer container,Type typeFrom,Type typeTo,string name,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNameNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, "test");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNameNullInjectionMembers()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, "test", (InjectionMember[])null);
            }
        }

        [TestMethod]
        public void RegisterFromTypeToUnregisteredTypeIfNotNameUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, "test");
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t = DIContainer.Root.Resolve<ITestTarget>("test");

            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterFromTypeToRegisteredTypeIfNotName()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterType<ITestTarget, TestTargetFromCode>("test", new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t1 = DIContainer.Root.Resolve<ITestTarget>("test");
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, "test");
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t2 = DIContainer.Root.Resolve<ITestTarget>("test");
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterTypeIfNot(this IUnityContainer container,Type typeFrom,Type typeTo,string name,LifetimeManager lifetimeManager,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNameLifetimeNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, "test", new TransientLifetimeManager());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNameLifetimeNullInjectionMembers()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, "test", new TransientLifetimeManager(), (InjectionMember[])null);
            }
        }

        [TestMethod]
        public void RegisterFromTypeToUnregisteredTypeIfNotNameLifetimeUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, "test", new TransientLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t = DIContainer.Root.Resolve<ITestTarget>("test");

            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterFromTypeToRegisteredTypeIfNotNameLifetime()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterType<ITestTarget, TestTargetFromCode>("test", new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t1 = DIContainer.Root.Resolve<ITestTarget>("test");
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterTypeIfNot<ITestTarget, TestTargetFromCode>(regs, "test", new TransientLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t2 = DIContainer.Root.Resolve<ITestTarget>("test");
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion
        #endregion

        #region Register instance
        #region IUnityContainer RegisterInstanceIfNot(this IUnityContainer container,Type type,object instance)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterInstanceIfNot<ITestTarget>(regs, new TestTargetFromCode());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNullInstance()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, (TestTargetFromCode)null);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNullLifetime()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, new TestTargetFromCode(), (LifetimeManager)null);
            }
        }

        [TestMethod]
        public void RegisterUnregisteredInstanceIfNotUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, new TestTargetFromCode());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t = DIContainer.Root.Resolve<ITestTarget>();

            Assert.IsTrue(t is TestTargetFromCode);
            Assert.AreEqual("from code", t.IdentifySource());
        }

        [TestMethod]
        public void RegisterRegisteredInstanceIfNot()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterInstance<ITestTarget>(new TestTargetFromCode());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t1 = DIContainer.Root.Resolve<ITestTarget>();
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, new TestTargetFromCode());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t2 = DIContainer.Root.Resolve<ITestTarget>();
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterInstanceIfNot(this IUnityContainer container,Type type,object instance,LifetimeManager lifetimeManager)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotLifetimeNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterInstanceIfNot<ITestTarget>(regs, new TestTargetFromCode(), new ContainerControlledLifetimeManager());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotLifetimeNullInstance()
        {
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstance<ITestTarget>((TestTargetFromCode)null, new ContainerControlledLifetimeManager());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotLifetimeNull()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<TestTargetFromCode>(regs, new TestTargetFromCode(), (ContainerControlledLifetimeManager)null);
            }
        }

        [TestMethod]
        public void RegisterUnregisteredInstanceIfNotLifetimeUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, new TestTargetFromCode(), new ContainerControlledLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t = DIContainer.Root.Resolve<TestTargetFromCode>();

            Assert.AreEqual("from code", t.IdentifySource());
            Assert.IsTrue(t is TestTargetFromCode);
        }

        [TestMethod]
        public void RegisterRegisteredInstanceIfNotLifetime()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterInstance<ITestTarget>(new TestTargetFromCode());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t1 = DIContainer.Root.Resolve<ITestTarget>();
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, new TestTargetFromCode(), new ContainerControlledLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>());

            var t2 = DIContainer.Root.Resolve<ITestTarget>();
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterInstanceIfNot(this IUnityContainer container,Type type,string name,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNameNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterInstanceIfNot<ITestTarget>(regs, "test", new TestTargetFromCode());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNameNullInstance()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, "test", (TestTargetFromCode)null);
            }
        }

        [TestMethod]
        public void RegisterUnregisteredInstanceIfNotNameUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, "test", new TestTargetFromCode());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t = DIContainer.Root.Resolve<ITestTarget>("test");

            Assert.AreEqual("from code", t.IdentifySource());
            Assert.IsTrue(t is TestTargetFromCode);
        }

        [TestMethod]
        public void RegisterRegisteredRegisteredIfNotName()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterInstance<ITestTarget>("test", new TestTargetFromCode());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t1 = DIContainer.Root.Resolve<ITestTarget>("test");
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, "test", new TestTargetFromCode());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t2 = DIContainer.Root.Resolve<ITestTarget>("test");
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion

        #region IUnityContainer RegisterInstanceIfNot(this IUnityContainer container,Type type,string name,LifetimeManager lifetimeManager,params InjectionMember[] injectionMembers)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNameLifetimeNullContainer()
        {
            IUnityContainer container = null;

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                container.RegisterInstanceIfNot<ITestTarget>(regs, "test", new TestTargetFromCode(), new ContainerControlledLifetimeManager());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNameLifetimeNullInstance()
        {
            Utilities.ReinitContainer();

            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, "test", (TestTargetFromCode)null, new ContainerControlledLifetimeManager());
            }
        }

        [TestMethod]
        public void RegisterUnregisteredInstanceIfNotNameLifetimeUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, "test", new TestTargetFromCode(), new ContainerControlledLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t = DIContainer.Root.Resolve<ITestTarget>("test");

            Assert.AreEqual("from code", t.IdentifySource());
            Assert.IsTrue(t is TestTargetFromCode);
        }

        [TestMethod]
        public void RegisterRegisteredInstanceIfNotNameLifetime()
        {
            // Arrange
            Utilities.ReinitContainer();
            DIContainer.Root.RegisterInstance<ITestTarget>("test", new TestTargetFromCode(), new ContainerControlledLifetimeManager());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t1 = DIContainer.Root.Resolve<ITestTarget>("test");
            var id1 = t1.IdentifyUniqueSource();

            // Act
            lock (DIContainer.Root)
            {
                var regs = DIContainer.Root.GetRegistrationsSnapshot();
                DIContainer.Root.RegisterInstanceIfNot<ITestTarget>(regs, "test", new TestTargetFromCode(), new ContainerControlledLifetimeManager());
            }

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            var t2 = DIContainer.Root.Resolve<ITestTarget>("test");
            var id2 = t1.IdentifyUniqueSource();

            Assert.AreEqual(id1, id2);
        }
        #endregion
        #endregion
    }
}
