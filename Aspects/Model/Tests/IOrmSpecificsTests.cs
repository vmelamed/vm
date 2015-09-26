using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.Tests
{
    /// <summary>
    /// Summary description for IOrmSpecificTests
    /// </summary>
    [TestClass]
    public abstract class IOrmSpecificsTests<R> where R : class, IRepository
    {
        public IOrmSpecificsTests()
        {
            Container = DIContainer.Root;
        }

        protected IUnityContainer Container { get; set; }

        public TestContext TestContext { get; set; }

        protected abstract IRepository GetRepository();

        protected IRepository GetInitializedRepository()
        {
            var repository = GetRepository();

            repository.Initialize();
            Assert.IsTrue(repository.IsInitialized);

            return repository;
        }

        protected abstract IOrmSpecifics GetSpecifics();

        [TestMethod]
        public void FetchTest()
        {
            IRepository target;
            TestEntity2 entity2;
            TestEntity2 entity21;
            string name1, name2, name3;

            using (target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                entity2 = target.CreateEntity<TestEntity2>();
                entity2.Id = target.GetStoreId<TestEntity2, long>();
                entity2.Name = name1 = "test" + entity2.Id;
                name2 = "test"+(entity2.Id+1);
                name3 = "test"+(entity2.Id+2);
                entity2.InternalValues.Add(
                    new TestValue
                    {
                        Id = target.GetStoreId<TestValue, long>(),
                        Name = name2,
                    });
                entity2.InternalValues.Add(
                    new TestValue
                    {
                        Id = target.GetStoreId<TestValue, long>(),
                        Name = name3,
                    });
                target.Add(entity2);

                target.CommitChanges();
            }

            using (target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                Assert.IsTrue(target.Entities<TestEntity2>().Count() >= 1);
                var seq = target.Entities<TestEntity2>()
                                .Where(o => o.Name == name1);

                entity21 = GetSpecifics()
                                .Fetch(seq, t => t.InternalValues)
                                .FirstOrDefault();

                Assert.AreEqual(entity2, entity21);
                Assert.AreEqual(2, entity21.InternalValues.Count());
                Assert.AreEqual(name2, entity21.InternalValues.ElementAt(0).Name);
                Assert.AreEqual(name3, entity21.InternalValues.ElementAt(1).Name);
            }
        }

        public abstract void EnlistInAmbientTransaction();

        public abstract void ObjectIsProxyTest();

        public abstract void ObjectIsLoadedTest();

        public abstract void ObjectIsChangeTrackingTest();

        public abstract void ExceptionIsOptimisticConcurrencyTest();

        public abstract void ExceptionIsConnectionRelatedTest();

        public abstract void ExceptionIsTransientTest();
    }
}
