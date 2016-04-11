using System.Linq;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Model.InMemory;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.Tests
{
    [TestClass]
    public abstract class ObjectsSpecificTests<R> : IOrmSpecificsTests<R> where R : class, IRepository
    {
        protected override IOrmSpecifics GetSpecifics() => new ObjectsRepositorySpecifics();

        [TestMethod]
        public override void EnlistInAmbientTransaction()
        {
            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                using (var tx = new TransactionScope())
                    Assert.AreEqual(target, GetSpecifics().EnlistInAmbientTransaction(target));
            }
        }

        [TestMethod]
        public override void ObjectIsProxyTest()
        {
            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                var e1 = target.CreateEntity<TestEntity>();

                e1.Id = target.GetStoreId<TestEntity, long>();

                Assert.IsFalse(GetSpecifics().IsProxy(e1));

                var e2 = new TestEntity();

                e2.Id = target.GetStoreId<TestEntity, long>();

                Assert.IsFalse(GetSpecifics().IsProxy(e2));
            }
        }

        [TestMethod]
        public override void ObjectIsLoadedTest()
        {
            long id;
            TestEntity principal;
            TestXEntity associated;

            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                id = target.GetStoreId<TestEntity, long>();
                principal = target.CreateEntity<TestEntity>();
                principal.Id = id;
                principal.Name = "principal"+id;

                associated = target.CreateEntity<TestXEntity>();
                associated.Id = target.GetStoreId<TestEntity, long>();

                principal.XEntity = associated;

                target.Add(principal);

                target.CommitChanges();
            }

            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                principal = target.GetByStoreId<TestEntity, long>(id);

                Assert.IsTrue(GetSpecifics().IsLoaded(associated, principal, "XEntity", target));
            }

            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                var seq = target.Entities<TestEntity>()
                                .Where(e => e.Id==id);
                principal = GetSpecifics().FetchAlso(seq, e => e.XEntity)
                                          .FirstOrDefault();
                associated = principal.XEntity;

                Assert.IsTrue(GetSpecifics().IsLoaded(associated, principal, "XEntity", target));
            }
        }

        [TestMethod]
        public override void ObjectIsChangeTrackingTest()
        {
            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                var e = target.CreateEntity<TestEntity>();

                Assert.IsTrue(GetSpecifics().IsChangeTracking(e, target));

                var f = new TestEntity();

                Assert.IsTrue(GetSpecifics().IsChangeTracking(f, target));
            }
        }

        public override void ExceptionIsOptimisticConcurrencyTest()
        {
            throw new System.NotImplementedException();
        }

        public override void ExceptionIsConnectionRelatedTest()
        {
            throw new System.NotImplementedException();
        }

        public override void ExceptionIsTransientTest()
        {
            throw new System.NotImplementedException();
        }
    }
}
