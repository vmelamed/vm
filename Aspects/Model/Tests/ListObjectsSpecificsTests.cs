using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Model.InMemory;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.Tests
{
    [TestClass]
    public class ListObjectsSpecificsTests : ObjectsSpecificTests<ListObjectsRepository>
    {
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize]
        //public static void ClassInitialize(
        //    TestContext testContext)
        //{
        //}
        //
        // Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup]
        //public static void ClassCleanup()
        //{
        //}
        //
        // Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void TestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void TestCleanup() { }
        //
        #endregion

        protected override IRepository GetRepository()
        {
            return new ListObjectsRepository();
        }
    }
}
