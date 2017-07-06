using System;
using System.IO;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Summary description for DIContainerTests
    /// </summary>
    [TestClass]
    [DeploymentItem(".\\test.config")]
    [DeploymentItem(".\\testWithError.config")]
    public class DIContainerTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
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
        public void InitializeDefaultFromAppConfigTest()
        {
            Utilities.ResetContainer();

            DIContainer.Initialize();
            DIContainer.Root.DebugDump();

            Assert.IsNotNull(DIContainer.Root);
            Assert.IsTrue(DIContainer.IsInitialized);
            Assert.AreEqual(4, DIContainer.Root.Registrations.Count());

            var idSource = DIContainer.Root.Resolve<ITestTarget>();

            Assert.AreEqual(idSource.IdentifySource(), "from app.config");

            DIContainer.Initialize();
            Assert.AreEqual(4, DIContainer.Root.Registrations.Count());
        }

        [TestMethod]
        public void InitializeFileNullFromAppConfigTest()
        {
            Utilities.ResetContainer();

            DIContainer.Initialize(null);
            DIContainer.Root.DebugDump();

            Assert.IsNotNull(DIContainer.Root);
            Assert.IsNotNull(ServiceLocator.Current);
            Assert.IsTrue(DIContainer.IsInitialized);
            Assert.AreEqual(5, DIContainer.Root.Registrations.Count());

            var idSource = DIContainer.Root.Resolve<ITestTarget>();

            Assert.AreEqual(idSource.IdentifySource(), "from app.config");

            DIContainer.Initialize();
            Assert.AreEqual(5, DIContainer.Root.Registrations.Count());
        }


        [TestMethod]
        public void InitializeDefaultFromTestConfigTest()
        {
            Utilities.ResetContainer();

            DIContainer.Initialize("test.config");

            Assert.IsNotNull(DIContainer.Root);
            Assert.IsNotNull(ServiceLocator.Current);
            Assert.IsTrue(DIContainer.IsInitialized);

            var idSource = DIContainer.Root.Resolve<ITestTarget>();

            Assert.AreEqual(idSource.IdentifySource(), "from test.config");
        }

        [TestMethod]
        public void InitializeDefaultFromMissingTestConfigTest()
        {
            Utilities.ResetContainer();

            DIContainer.Initialize("c:\\test.config");

            Assert.IsNotNull(DIContainer.Root);
            Assert.IsNotNull(ServiceLocator.Current);
            Assert.IsTrue(DIContainer.IsInitialized);

            var idSource = DIContainer.Root.Resolve<ITestTarget>();

            Assert.AreEqual(idSource.IdentifySource(), "from app.config");
        }

        [TestMethod]
        public void InitializeDefaultFromAppConfigBoxTest()
        {
            Utilities.ResetContainer();

            DIContainer.Initialize(null, "unity", "box");

            Assert.IsNotNull(DIContainer.Root);
            Assert.IsNotNull(ServiceLocator.Current);
            Assert.IsTrue(DIContainer.IsInitialized);

            var idSource = DIContainer.Root.Resolve<ITestTarget>();

            Assert.AreEqual(idSource.IdentifySource(), "from app.config/box");
        }

        [TestMethod]
        public void InitializeDefaultFromTestConfigBoxTest()
        {
            Utilities.ResetContainer();

            DIContainer.Initialize("test.config", "unity", "box");

            Assert.IsNotNull(DIContainer.Root);
            Assert.IsNotNull(ServiceLocator.Current);
            Assert.IsTrue(DIContainer.IsInitialized);

            var idSource = DIContainer.Root.Resolve<ITestTarget>();

            Assert.AreEqual(idSource.IdentifySource(), "from test.config/box");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InitializeDefaultFromTestWithErrorConfigBoxTest()
        {
            Utilities.ResetContainer();

            DIContainer.Initialize("testWithError.config", "unity", "box");
            DIContainer.Root.DebugDump();

            Assert.Fail("The initialization was expected to throw exception.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DumpTestNullWriter()
        {
            DIContainer.Root.Dump(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DumpTestNullContainer()
        {
            var writer = new StringWriter();
            UnityContainer unity = null;

            unity.Dump(writer);
        }

        [TestMethod]
        public void DumpTest()
        {
            string dump;

            using (var writer = new StringWriter())
            {
                DIContainer.Reset();
                DIContainer.Initialize("test.config", "unity", "box");

                DIContainer.Root.Dump(writer);

                dump = writer.GetStringBuilder().ToString();
            }

            TestContext.WriteLine("{0}", dump);
            Assert.AreEqual(
@"Container has 3 Registrations:
+ InjectionPolicy  'Microsoft.Practices.Unity.InterceptionExtension.AttributeDrivenPolicy, Microsoft.Practices.Unity.Interception, Version=4.0.0.0, Culture=neutral, PublicKeyToken=6d32ff45e0ccc69f'  ContainerControlled
+ ITestTarget -> TestTargetFromTestConfigBox  '[default]'  Transient
+ IUnityContainer  '[default]'  Container
", dump);
        }
    }
}
