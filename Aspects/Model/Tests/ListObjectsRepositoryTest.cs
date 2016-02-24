using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Model.InMemory;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ListObjectsRepositoryTest : IRepositoryTest<ListObjectsRepository>
    {
        protected override IRepository GetRepository() => new ListObjectsRepository();

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void ClassInitialize(
            TestContext testContext)
        {
            ListObjectsRepository.Reset();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup]
        //public static void ClassCleanup()
        //{
        //}
        //
        // Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void TestInitialize()
        //{
        //}
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public override void DetachTest()
        {
            base.DetachTest();
        }

        [TestMethod]
        public override void DetachedEntitiesTest()
        {
            IRepository target;
            TestEntity entity1;
            TestEntity entity2;
            TestEntity entity3;

            using (target = GetInitializedRepository())
            {
                entity1 = target.CreateEntity<TestEntity>();
                entity1.Id = target.GetStoreId<TestEntity, long>();
                entity1.Name = "test01";
                target.Add(entity1);

                entity2 = target.CreateEntity<TestEntity1>();
                entity2.Id = target.GetStoreId<TestEntity1, long>();
                entity2.Name = "test02";
                target.Add(entity2);

                entity3 = target.CreateEntity<TestEntity2>();
                entity3.Id = target.GetStoreId<TestEntity2, long>();
                entity3.Name = "test03";
                target.Add(entity3);

                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                Assert.IsTrue(target.DetachedEntities<TestEntity>().Count() >= 3);
                Assert.AreEqual(entity1, target.DetachedEntities<TestEntity>().Where(o => o.Name == "test01").FirstOrDefault());
                Assert.AreEqual(entity2, target.DetachedEntities<TestEntity>().Where(o => o.Name == "test02").FirstOrDefault());
                Assert.AreEqual(entity3, target.DetachedEntities<TestEntity>().Where(o => o.Name == "test03").FirstOrDefault());
                Assert.IsNull(target.DetachedEntities<TestEntity>().Where(o => o.Name == "test04").FirstOrDefault());

                entity1 = target.DetachedEntities<TestEntity>().Where(o => o.Name == "test01").FirstOrDefault();
                entity1.StringProperty = "aha!";

                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                entity1 = target.DetachedEntities<TestEntity>().Where(o => o.Name == "test01").FirstOrDefault();

                // in-memory repositories do not support detached.
                Assert.AreEqual("aha!", entity1.StringProperty);
            }
        }
    }
}
