using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;

namespace vm.Aspects.Tests.Facilities
{
    /// <summary>
    /// Summary description for GenericIGuidGeneratorTests
    /// </summary>
    [TestClass]
    public abstract class GenericIGuidGeneratorTests
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

        protected abstract IGuidGenerator GetGenerator();

        [TestMethod]
        public void NextGuidTest()
        {
            var target = GetGenerator();

            var guid1 = target.NewGuid();
            var guid2 = target.NewGuid();

            Assert.AreNotEqual(guid1, guid2);
        }
    }
}
