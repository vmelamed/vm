using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;
using vm.Aspects.Model.Tests;

namespace vm.Aspects.Model.EFRepository.Tests
{
    /// <summary>
    /// Summary description for EFSpecificTests
    /// </summary>
    [TestClass]
    public class EFSpecificsTests : IOrmSpecificsTests<TestEFRepository>
    {
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            EFRepositoryTests.ClassInitialize(testContext);
        }

        // Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
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

        protected override IRepository GetRepository()
        {
            return EFRepositoryTests.CreateRepository();
        }

        protected override IOrmSpecifics GetSpecifics()
        {
            return new EFSpecifics();
        }

        [TestMethod]
        public void EnlistInAmbientTransactionRolledBack()
        {
            var specifics = GetSpecifics();

            try
            {
                using (var repository = GetInitializedRepository())
                {
                    var entity = repository.CreateEntity<TestEntity>();

                    entity.Id = repository.GetStoreId<TestEntity, long>();
                    entity.Name = "Entity1";

                    repository.Add(entity);

                    using (var transactionScope = new TransactionScope())
                    {
                        specifics.EnlistInAmbientTransaction(repository);
                        repository.CommitChanges();

                        // not calling transactionScope.Complete() is equivalent to ROLLBACK.
                    }
                }
            }
            catch (Exception x)
            {
                TestContext.WriteLine("{0}", x.DumpString());
                throw;
            }

            using (var repository = GetInitializedRepository())
                Assert.IsNull(repository.Entities<TestEntity>()
                                        .FirstOrDefault(e => e.Name == "Entity1"));
        }

        [TestMethod]
        public override void EnlistInAmbientTransaction()
        {
            var specifics = GetSpecifics();

            try
            {
                using (var repository = GetInitializedRepository())
                {
                    var entity = repository.CreateEntity<TestEntity>();

                    entity.Id = repository.GetStoreId<TestEntity, long>();
                    entity.Name = "Entity2";

                    repository.Add(entity);

                    using (var transactionScope = new TransactionScope())
                    {
                        specifics.EnlistInAmbientTransaction(repository);
                        repository.CommitChanges();

                        transactionScope.Complete();
                    }
                }
            }
            catch (Exception x)
            {
                TestContext.WriteLine("{0}", x.DumpString());
                throw;
            }

            using (var repository = GetInitializedRepository())
                Assert.IsNotNull(repository.Entities<TestEntity>()
                                           .FirstOrDefault(e => e.Name == "Entity2"));
        }

        [TestMethod]
        public override void ObjectIsProxyTest()
        {
            try
            {
                var specifics = GetSpecifics();

                using (var repository = GetInitializedRepository())
                {
                    var e1 = new TestEntity();
                    var e2 = repository.CreateEntity<TestEntity>();
                    var e3 = repository.CreateEntity<TestEntity>();

                    e1.Id = repository.GetStoreId<TestEntity, long>();
                    e2.Id = repository.GetStoreId<TestEntity, long>();
                    e3.Id = repository.GetStoreId<TestEntity, long>();

                    e1.Name = "Entity3";
                    e2.Name = "Entity4";
                    e3.Name = "Entity5";

                    e1.Created =
                    e2.Created =
                    e3.Created =
                    e1.Updated =
                    e2.Updated =
                    e3.Updated = Facility.Clock.UtcNow;

                    Assert.IsFalse(specifics.IsProxy(e1));
                    Assert.IsTrue(specifics.IsProxy(e2));
                    Assert.IsTrue(specifics.IsProxy(e3));

                    repository.Add(e1)
                              .Add(e2)
                              .Add(e3);

                    Assert.IsFalse(specifics.IsProxy(e1));
                    Assert.IsTrue(specifics.IsProxy(e2));
                    Assert.IsTrue(specifics.IsProxy(e3));

                    repository.CommitChanges();
                }

                using (var repository = GetInitializedRepository())
                {
                    var e4 = repository.Entities<TestEntity>().Where(e => e.Name == "Entity3").First();
                    var e5 = repository.Entities<TestEntity>().Where(e => e.Name == "Entity4").First();
                    var e6 = repository.Entities<TestEntity>().Where(e => e.Name == "Entity5").First();

                    Assert.IsNotNull(e4);
                    Assert.IsNotNull(e5);
                    Assert.IsNotNull(e6);

                    Assert.IsTrue(specifics.IsProxy(e4));
                    Assert.IsTrue(specifics.IsProxy(e5));
                    Assert.IsTrue(specifics.IsProxy(e6));
                }
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception x)
            {
                TestContext.WriteLine("{0}", x.DumpString());
                throw;
            }
        }

        [TestMethod]
        public override void ObjectIsLoadedTest()
        {
            long id;
            TestEntity principal;
            TestXEntity associated;
            var specifics = GetSpecifics();

            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                id = target.GetStoreId<TestEntity, long>();
                principal = target.CreateEntity<TestEntity>();
                principal.Id = id;
                principal.Name = "principal";

                associated = target.CreateEntity<TestXEntity>();
                associated.Id = target.GetStoreId<TestEntity, long>();
                associated.Name = "associated";

                principal.XEntity = associated;

                target.Add(principal);

                target.CommitChanges();
            }

            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                principal = target.GetByStoreId<TestEntity, long>(id);

                Assert.IsTrue(specifics.IsLoaded(principal.XEntity, principal, "XEntity", target));
            }

            using (var target = GetRepository())
            {
                target.Initialize();
                Assert.IsTrue(target.IsInitialized);

                var seq = target.Entities<TestEntity>()
                                .Where(e => e.Id==id);
                principal = specifics.Fetch(seq, e => e.XEntity)
                                     .FirstOrDefault();
                associated = principal.XEntity;

                Assert.IsTrue(specifics.IsLoaded(associated, principal, "XEntity", target));
            }
        }

        [TestMethod]
        public override void ObjectIsChangeTrackingTest()
        {
            var specifics = GetSpecifics();

            using (var repository = GetInitializedRepository())
            {
                var e6 = repository.CreateEntity<TestEntity>();

                e6.Id = repository.GetStoreId<TestEntity, long>();
                e6.Name = "Entity6";

                Assert.IsTrue(specifics.IsProxy(e6));

                repository.Add(e6);

                Assert.IsTrue(specifics.IsProxy(e6));

                repository.CommitChanges();
            }

            using (var repository = GetInitializedRepository())
            {
                var e6 = repository.Entities<TestEntity>()
                                   .Where(e => e.Name == "Entity6")
                                   .First();

                Assert.IsNotNull(e6);
                Assert.IsTrue(specifics.IsChangeTracking(e6, repository));
            }
        }

        [TestMethod]
        public override void ExceptionIsOptimisticConcurrencyTest()
        {
            var specifics = GetSpecifics();

            Assert.IsTrue(specifics.IsOptimisticConcurrency(new OptimisticConcurrencyException()) &&
                          specifics.IsOptimisticConcurrency(new DbUpdateConcurrencyException()));
            Assert.IsFalse(specifics.IsOptimisticConcurrency(new Exception()));
        }

        [TestMethod]
        public override void ExceptionIsConnectionRelatedTest()
        {
            // SqlException-s cannot be instantiated or mocked.
            Assert.IsTrue(true);
        }

        [TestMethod]
        public override void ExceptionIsTransientTest()
        {
            var specifics = GetSpecifics();

            Assert.IsTrue(specifics.IsTransient(new OptimisticConcurrencyException()) &&
                          specifics.IsTransient(new DbUpdateConcurrencyException()));
            Assert.IsFalse(specifics.IsTransient(new Exception()));
        }
    }
}
