using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;

namespace vm.Aspects.Tests.Facilities
{
    /// <summary>
    /// Summary description for ClockTests
    /// </summary>
    [TestClass]
    public abstract class GenericIClockTests
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

        protected abstract IClock GetClock();

        [TestMethod]
        public void InitializeTest()
        {
            IClock target = new Clock();

            Assert.IsTrue(target.Initialize());
        }

        [TestMethod]
        public void UtcNowTest()
        {
            IClock target = new Clock();
            var now1 = target.UtcNow;
            var now2 = target.UtcNow;

            Assert.IsTrue(now1<=now2);
        }
    }
}
