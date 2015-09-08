using System;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Summary description for DIContainerExtentionsTests
    /// </summary>
    [TestClass]
    [DeploymentItem(".\\test.config")]
    [DeploymentItem(".\\testWithError.config")]
    public class DIContainerExtentionsTests
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

            container.RegisterTypeIfNot(typeof(TestTargetFromCode));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNullType()
        {
            DIContainer.Root.RegisterTypeIfNot((Type)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNullInjectionMembers()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), (InjectionMember[])null);
        }

        [TestMethod]
        public void RegisterUnregisteredTypeIfNotUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            // Act
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode));

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
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode));

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

            container.RegisterTypeIfNot(typeof(TestTargetFromCode), new TransientLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotLifetimeNullType()
        {
            DIContainer.Root.RegisterTypeIfNot((Type)null, new TransientLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotLifetimeNullInjectionMembers()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), new TransientLifetimeManager(), (InjectionMember[])null);
        }

        [TestMethod]
        public void RegisterUnregisteredTypeIfNotLifetimeUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            // Act
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), new TransientLifetimeManager());

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
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), new TransientLifetimeManager());

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

            container.RegisterTypeIfNot(typeof(TestTargetFromCode), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNameNullType()
        {
            DIContainer.Root.RegisterTypeIfNot((Type)null, "test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNameNullInjectionMembers()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), "test", (InjectionMember[])null);
        }

        [TestMethod]
        public void RegisterUnregisteredTypeIfNotNameUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<TestTargetFromCode>());

            // Act
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), "test");

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
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), "test");

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

            container.RegisterTypeIfNot(typeof(TestTargetFromCode), "test", new TransientLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNameLifetimeNullType()
        {
            DIContainer.Root.RegisterTypeIfNot((Type)null, "test", new TransientLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterTypeIfNotNameLifetimeNullInjectionMembers()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), "test", new TransientLifetimeManager(), (InjectionMember[])null);
        }

        [TestMethod]
        public void RegisterUnregisteredTypeIfNotNameLifetimeUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));

            // Act
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), "test", new TransientLifetimeManager());

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
            DIContainer.Root.RegisterTypeIfNot(typeof(TestTargetFromCode), "test", new TransientLifetimeManager());

            // Assert
            Assert.IsTrue(DIContainer.Root.IsRegistered<TestTargetFromCode>("test"));

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

            container.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNullType()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), (Type)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNullInjectionMembers()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), (InjectionMember[])null);
        }

        [TestMethod]
        public void RegisterFromTypeToUnregisteredTypeIfNotUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode));

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
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode));

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

            container.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), new TransientLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotLifetimeNullType()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), (Type)null, new TransientLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotLifetimeNullInjectionMembers()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), new TransientLifetimeManager(), (InjectionMember[])null);
        }

        [TestMethod]
        public void RegisterFromTypeToUnregisteredTypeIfNotLifetimeUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), new TransientLifetimeManager());

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
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), new TransientLifetimeManager());

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

            container.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNameNullType()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), (Type)null, "test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNameNullInjectionMembers()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), "test", (InjectionMember[])null);
        }

        [TestMethod]
        public void RegisterFromTypeToUnregisteredTypeIfNotNameUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), "test");

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
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), "test");

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

            container.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), "test", new TransientLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNameLifetimeNullType()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), (Type)null, "test", new TransientLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFromTypeToTypeIfNotNameLifetimeNullInjectionMembers()
        {
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), "test", new TransientLifetimeManager(), (InjectionMember[])null);
        }

        [TestMethod]
        public void RegisterFromTypeToUnregisteredTypeIfNotNameLifetimeUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            // Act
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), "test", new TransientLifetimeManager());

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
            DIContainer.Root.RegisterTypeIfNot(typeof(ITestTarget), typeof(TestTargetFromCode), "test", new TransientLifetimeManager());

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

            container.RegisterInstanceIfNot(typeof(ITestTarget), new TestTargetFromCode());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNullType()
        {
            DIContainer.Root.RegisterInstanceIfNot((Type)null, new TestTargetFromCode());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNullInstance()
        {
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), (TestTargetFromCode)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNullLifetime()
        {
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), new TestTargetFromCode(), (LifetimeManager)null);
        }

        [TestMethod]
        public void RegisterUnregisteredInstanceIfNotUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), new TestTargetFromCode());

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
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), new TestTargetFromCode());

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

            container.RegisterInstanceIfNot(typeof(ITestTarget), new TestTargetFromCode(), new ContainerControlledLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotLifetimeNullType()
        {
            DIContainer.Root.RegisterInstanceIfNot((Type)null, new TestTargetFromCode(), new ContainerControlledLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotLifetimeNullInstance()
        {
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), (TestTargetFromCode)null, new ContainerControlledLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotLifetimeNull()
        {
            DIContainer.Root.RegisterInstanceIfNot(typeof(TestTargetFromCode), (ContainerControlledLifetimeManager)null);
        }

        [TestMethod]
        public void RegisterUnregisteredInstanceIfNotLifetimeUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), new TestTargetFromCode(), new ContainerControlledLifetimeManager());

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
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), new TestTargetFromCode(), new ContainerControlledLifetimeManager());

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

            container.RegisterInstanceIfNot(typeof(ITestTarget), "test", new TestTargetFromCode());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNameNullType()
        {
            DIContainer.Root.RegisterInstanceIfNot((Type)null, "test", new TestTargetFromCode());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNameNullInstance()
        {
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), "test", (TestTargetFromCode)null);
        }

        [TestMethod]
        public void RegisterUnregisteredInstanceIfNotNameUnregistered()
        {
            // Arrange
            Utilities.ResetContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>());

            // Act
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), "test", new TestTargetFromCode());

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
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), "test", new TestTargetFromCode());

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

            container.RegisterInstanceIfNot(typeof(ITestTarget), "test", new TestTargetFromCode(), new ContainerControlledLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNameLifetimeNullType()
        {
            DIContainer.Root.RegisterInstanceIfNot((Type)null, "test", new TestTargetFromCode(), new ContainerControlledLifetimeManager());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInstanceIfNotNameLifetimeNullInstance()
        {
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), "test", (TestTargetFromCode)null, new ContainerControlledLifetimeManager());
        }

        [TestMethod]
        public void RegisterUnregisteredInstanceIfNotNameLifetimeUnregistered()
        {
            // Arrange
            Utilities.ReinitContainer();

            Assert.IsFalse(DIContainer.Root.IsRegistered<ITestTarget>("test"));

            // Act
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), "test", new TestTargetFromCode(), new ContainerControlledLifetimeManager());

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
            DIContainer.Root.RegisterInstanceIfNot(typeof(ITestTarget), "test", new TestTargetFromCode(), new ContainerControlledLifetimeManager());

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
