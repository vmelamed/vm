using System.Linq;
using Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;

namespace vm.Aspects.Policies.Tests
{
    /// <summary>
    /// Summary description for CallTraceCallHandler
    /// </summary>
    [TestClass]
    public class TestCallTraceCallHandler
    {
        public TestCallTraceCallHandler()
        {
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            DIContainer.Root
                .RegisterTypeIfNot<ITestCalls, TestCalls>()
                ;
        }

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
        public void Test1()
        {
            var testObject = DIContainer.Root.Resolve<ITestCalls>();

            testObject.Test1();

            Assert.AreEqual(
                @"",
                TestTraceListener.Messages.FirstOrDefault());
        }
    }
}
