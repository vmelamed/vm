using System;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.ServiceLocation;
using Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;

namespace vm.Aspects.Tests.Facilities
{
    /// <summary>
    /// Summary description for FacilityRegistrarTests
    /// </summary>
    [TestClass]
    public class FacilityRegistrarIntegrationTests
    {
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
        public void AllIsRegisteredTest()
        {
            DIContainer.Reset();

            // here we need the CSL first
            DIContainer.Initialize();

            Facility.Registrar.Reset();

            lock (DIContainer.Root)
            {
                var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                DIContainer.Root
                           .UnsafeRegister(Facility.Registrar, registrations)
                           ;
            }

            DIContainer.Root.DebugDump();

            Assert.IsTrue(DIContainer.Root.IsRegistered<IExceptionPolicyProvider>(ExceptionPolicyProvider.RegistrationName));
            Assert.IsTrue(DIContainer.Root.IsRegistered<LoggingConfiguration>());
            Assert.IsFalse(DIContainer.Root.IsRegistered<LoggingConfiguration>(LogConfigProvider.TestLogConfigurationResolveName));

            Assert.IsTrue(DIContainer.Root.IsRegistered<IClock>());
            Assert.IsTrue(DIContainer.Root.IsRegistered<IGuidGenerator>());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ValidatorFactory>());
            Assert.IsTrue(DIContainer.Root.IsRegistered<LogWriter>());
            Assert.IsTrue(DIContainer.Root.IsRegistered<ExceptionManager>());

            var clock = DIContainer.Root.Resolve<IClock>();

            Assert.IsTrue(clock is Clock);
            Assert.AreEqual(clock, Facility.Clock);

            var guidGen = ServiceLocator.Current.GetInstance<IGuidGenerator>();

            Assert.IsTrue(guidGen is GuidGenerator);
            Assert.AreEqual(guidGen, Facility.GuidGenerator);

            var vFactory = DIContainer.Root.Resolve<ValidatorFactory>();

            Assert.IsTrue(vFactory is ValidatorFactory);
            Assert.AreEqual(vFactory, Facility.ValidatorFactory);

            Assert.IsNotNull(Facility.LogWriter);
            Assert.IsNotNull(Facility.ExceptionManager);

            DIContainer.Reset();
        }
    }
}
