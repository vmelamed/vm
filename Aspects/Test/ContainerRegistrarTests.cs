using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Practices.Unity;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Summary description for ContainerRegistrarTests
    /// </summary>
    [TestClass]
    public class ContainerRegistrarTests
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

        class TestRegistrar : ContainerRegistrar
        {
            public bool DoRegistrarCalled
            {
                get
                {
                    var called = _ranRegistration;

                    _ranRegistration = false;
                    return called;
                }
            }

            bool _ranRegistration;
            bool _ranTestRegistration;

            public bool RanRegistration
            {
                get
                {
                    var called = _ranRegistration;

                    _ranRegistration = false;
                    return called;
                }
            }

            public bool RanTestRegistration
            {
                get
                {
                    var called = _ranTestRegistration;

                    _ranTestRegistration = false;
                    return called;
                }
            }

            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                _ranRegistration = true;
            }

            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                _ranTestRegistration = true;
            }
        }

        [TestMethod]
        public void TestRegisterTest()
        {
            var target = new TestRegistrar();

            Assert.IsFalse(target.AreRegistered());

            var unityMoq = new Mock<IUnityContainer>();

            target.Register(unityMoq.Object, true);

            Assert.IsTrue(target.AreRegistered(unityMoq.Object));
            Assert.IsFalse(target.RanRegistration);
            Assert.IsTrue(target.RanTestRegistration);

            target.Register(unityMoq.Object, true);

            Assert.IsTrue(target.AreRegistered(unityMoq.Object));
            Assert.IsFalse(target.RanRegistration);
            Assert.IsFalse(target.RanTestRegistration);
        }

        [TestMethod]
        public void NonTestRegisterTest()
        {
            // Arrange
            var target = new TestRegistrar();

            Assert.IsFalse(target.AreRegistered());

            var unityMoq = new Mock<IUnityContainer>();

            unityMoq.Setup(c => c.GetHashCode()).Returns(1);

            // Act
            target.Register(unityMoq.Object, false);

            // Assert
            Assert.IsTrue(target.AreRegistered(unityMoq.Object));
            Assert.IsTrue(target.RanRegistration);
            Assert.IsFalse(target.RanTestRegistration);

            // Act
            target.Register(unityMoq.Object, false);

            // Assert
            Assert.IsTrue(target.AreRegistered(unityMoq.Object));
            Assert.IsFalse(target.RanRegistration);
            Assert.IsFalse(target.RanTestRegistration);
        }

        [TestMethod]
        public void RegisterUnityTest()
        {
            // Arrange
            var target = new TestRegistrar();

            DIContainer.Reset();

            Assert.IsFalse(target.AreRegistered());

            // Act
            target.Register(null, false);

            // Assert
            Assert.IsTrue(target.AreRegistered());
            Assert.IsTrue(target.RanRegistration);
            Assert.IsFalse(target.RanTestRegistration);

            // Act
            target.Register(null, false);

            // Assert
            Assert.IsTrue(target.AreRegistered());
            Assert.IsFalse(target.RanRegistration);
            Assert.IsFalse(target.RanTestRegistration);
        }

        [TestMethod]
        public void NonTestUnsafeRegisterTest()
        {
            // Arrange
            var target = new TestRegistrar();
            var unityMoq = new Mock<IUnityContainer>();

            unityMoq.Setup(c => c.Registrations)
                    .Returns(new List<ContainerRegistration>());

            // Act
            target.UnsafeRegister(
                    unityMoq.Object,
                    unityMoq.Object.GetRegistrationsSnapshot(),
                    false);

            // Assert
            Assert.IsTrue(target.AreRegistered(unityMoq.Object));
            Assert.IsTrue(target.RanRegistration);
            Assert.IsFalse(target.RanTestRegistration);
        }

        [TestMethod]
        public void UnsafeRegisterUnityTest()
        {
            // Arrange
            var target = new TestRegistrar();

            DIContainer.Reset();

            // Act
            target.UnsafeRegister(
                    null,
                    null,
                    false);

            // Assert
            Assert.IsTrue(DIContainer.IsInitialized);
            Assert.IsTrue(target.AreRegistered());
            Assert.IsTrue(target.RanRegistration);
            Assert.IsFalse(target.RanTestRegistration);
        }

        [TestMethod]
        public void TestUnsafeRegisterTest()
        {
            // Arrange
            var target = new TestRegistrar();
            var unityMoq = new Mock<IUnityContainer>();

            unityMoq.Setup(c => c.Registrations)
                    .Returns(new List<ContainerRegistration>());

            // Act
            target.UnsafeRegister(
                    unityMoq.Object,
                    unityMoq.Object.GetRegistrationsSnapshot(),
                    true);

            // Assert
            Assert.IsTrue(target.AreRegistered(unityMoq.Object));
            Assert.IsFalse(target.RanRegistration);
            Assert.IsTrue(target.RanTestRegistration);
        }
    }
}
