﻿using System;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;
using vm.Aspects.Model.EFRepository;
using vm.Aspects.Model.InMemory;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.Tests
{
    /// <summary>
    /// Summary description for IRepositoryTest
    /// </summary>
    [TestClass]
    public abstract class IRepositoryTest<R> where R : class, IRepository
    {
        protected IRepositoryTest()
        {
            Container = DIContainer.Root;
        }

        protected virtual IUnityContainer Container { get; set; }

        public TestContext TestContext { get; set; }

        protected abstract IRepository GetRepository();

        protected IRepository GetInitializedRepository()
        {
            var repository = GetRepository();

            repository.Initialize();
            Assert.IsTrue(repository.IsInitialized);

            return repository;
        }

        [TestMethod]
        public virtual void InitializeTest()
        {
            try
            {
                var target = GetRepository();

                target.Initialize();

                Assert.IsTrue(target.IsInitialized);
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception x)
            {
                TestContext.WriteLine("{0}", x.DumpString());
                using (var writer = new StringWriter())
                {
                    Container.Dump(writer);
                    TestContext.WriteLine("{0}", writer.GetStringBuilder().ToString());
                }
                throw;
            }
        }

        [TestMethod]
        public virtual void GetGenericStoreIdTest()
        {
            var target = GetInitializedRepository();

            var id1 = target.GetStoreId<TestEntity, long>();

            Assert.AreNotEqual(0, id1);

            var id2 = target.GetStoreId<TestEntity, long>();

            Assert.AreNotEqual(id1, id2);

            var id3 = target.GetStoreId<TestEntity, long>();

            Assert.AreNotEqual(id1, id3);
            Assert.AreNotEqual(id2, id3);
        }

        [TestMethod]
        public virtual void CreateEntityTest()
        {
            var target = GetInitializedRepository();

            var test = target.CreateEntity<TestEntity>();

            Assert.IsNotNull(test);
            Assert.IsTrue(test.GetType() == typeof(TestEntity) || 
                          typeof(TestEntity).IsAssignableFrom(test.GetType()));
        }

        [TestMethod]
        public virtual void CreateValueTest()
        {
            var target = GetInitializedRepository();

            var test = target.CreateValue<TestValue>();

            Assert.IsNotNull(test);
            Assert.IsTrue(test.GetType() == typeof(TestValue) || 
                          typeof(TestValue).IsAssignableFrom(test.GetType()));
        }

        [TestMethod]
        public virtual void AddTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);
                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                var entityX = target.GetByStoreId<TestEntity, long>(id);

                Assert.AreEqual(entity, entityX);
                Assert.AreEqual(id, entityX.Id);
            }
        }

        [TestMethod]
        public virtual void AddDerivedTest()
        {
            long id, id1;
            IRepository target;
            TestEntity entity;
            TestEntity1 entity1;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);

                id1 = target.GetStoreId<TestEntity1, long>();
                entity1 = target.CreateEntity<TestEntity1>();

                entity1.Id = id1;
                entity1.Name = "test"+id1;

                target.Add(entity1);

                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                var entityX = target.GetByStoreId<TestEntity, long>(id);

                Assert.AreEqual(entity, entityX);
                Assert.AreEqual(id, entityX.Id);

                var entityX1 = target.GetByStoreId<TestEntity, long>(id1);

                Assert.AreEqual(entity1, entityX1);
                Assert.AreEqual(id1, entityX1.Id);
            }
        }

        [TestMethod]
        public virtual void AddBaseTest()
        {
            long id2, id1;
            IRepository target;
            TestEntity entity2;
            TestEntity1 entity1;

            using (target = GetInitializedRepository())
            {
                id2 = target.GetStoreId<TestEntity2, long>();
                entity2 = target.CreateEntity<TestEntity2>();

                entity2.Id = id2;
                entity2.Name = "test"+id2;

                target.Add(entity2);

                id1 = target.GetStoreId<TestEntity1, long>();
                entity1 = target.CreateEntity<TestEntity1>();

                entity1.Id = id1;
                entity1.Name = "test"+id1;

                target.Add(entity1);

                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                var entityX2 = target.GetByStoreId<TestEntity, long>(id2);

                Assert.AreEqual(entity2, entityX2);
                Assert.AreEqual(id2, entityX2.Id);

                var entityX1 = target.GetByStoreId<TestEntity, long>(id1);

                Assert.AreEqual(entity1, entityX1);
                Assert.AreEqual(id1, entityX1.Id);
            }
        }

        [TestMethod]
        public virtual void AttachTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);
                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                target.Attach(entity);
                target.CommitChanges();

                var entity1 = target.GetByStoreId<TestEntity, long>(id);

                Assert.IsTrue(ReferenceEquals(entity1, entity));
            }
        }

        [TestMethod]
        public virtual void AttachModifiedTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);
                target.CommitChanges();
            }

            entity.StringProperty = "testValue";

            using (target = GetInitializedRepository())
            {
                target.Attach(entity, EntityState.Modified, e => e.StringProperty);
                target.CommitChanges();

                var entity1 = target.GetByStoreId<TestEntity, long>(id);

                Assert.IsTrue(ReferenceEquals(entity1, entity));
            }
        }

        [TestMethod]
        public virtual void AttachAddedExistingTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);
                target.CommitChanges();
            }

            entity.StringProperty = "testValue";

            Type targetType = null;

            try
            {
                using (target = GetInitializedRepository())
                {
                    targetType = target.GetType();

                    target.Attach(entity, EntityState.Added);
                    target.CommitChanges();

                    Assert.IsTrue(false, "Expected some exception showing that attaching existing entity in Added state is not a valid operation.");

                    //var entity1 = target.GetByStoreId<TestEntity>(id);

                    //Assert.IsTrue(ReferenceEquals(entity1, entity));
                }
            }
            catch (InvalidOperationException x)
            {
                TestContext.WriteLine(x.DumpString());

                // swallow InvalidOperationException here - it is expected from the InMemory repositories
                Assert.IsTrue(typeof(ListObjectsRepository).IsAssignableFrom(targetType) ||
                              typeof(MapObjectsRepository).IsAssignableFrom(targetType));
            }
            // Add here more similar exception handlers for expected exceptions from different types of repositories.
            catch (Exception x)
            {
                TestContext.WriteLine(x.DumpString());
            }
        }

        [TestMethod]
        public virtual void AttachAddedNonExistingTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);
                target.CommitChanges();
            }

            entity.StringProperty = "testValue";

            var entity1 = new TestEntity
            {
                Name    = "test100",
                Created = Facility.Clock.UtcNow,
                Updated = Facility.Clock.UtcNow,
            };

            entity1.SetUpdated();

            using (target = GetInitializedRepository())
            {
                var id1 = target.GetStoreId<TestEntity, long>();

                entity1.Id = id1;

                target.Attach(entity1, EntityState.Added);
                target.CommitChanges();

                var entity2 = target.GetByStoreId<TestEntity, long>(id1);

                Assert.AreEqual(entity1, entity2);
                Assert.AreEqual(entity1.Id, entity2.Id);
                Assert.IsTrue(ReferenceEquals(entity1, entity2));
            }
        }

        [TestMethod]
        public virtual void AttachDeletedExistingTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);
                target.CommitChanges();

                Assert.IsNotNull(target.GetByStoreId<TestEntity, long>(id));
            }

            entity.StringProperty = "testValue";

            using (target = GetInitializedRepository())
            {
                target.Attach(entity, EntityState.Deleted);
                target.CommitChanges();

                Assert.IsNull(target.GetByStoreId<TestEntity, long>(id));
            }
        }

        [TestMethod]
        public virtual void AttachDeletedNonExistingTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);
                target.CommitChanges();
            }

            entity.StringProperty = "testValue";

            var entity1 = new TestEntity
            {
                Name    = "test101",
                Created = Facility.Clock.UtcNow,
                Updated = Facility.Clock.UtcNow,
            };

            entity1.SetUpdated();

            Type targetType = null;

            try
            {
                using (target = GetInitializedRepository())
                {
                    targetType = target.GetType();

                    var id1 = target.GetStoreId<TestEntity, long>();

                    entity1.Id = id1;

                    target.Attach(entity1, EntityState.Deleted);
                    target.CommitChanges();

                    Assert.IsNull(target.GetByStoreId<TestEntity, long>(id1));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                Assert.IsTrue(typeof(EFRepositoryBase).IsAssignableFrom(targetType));
            }
            // Add here more similar exception handlers for expected exceptions from different types of repositories.
            catch (Exception x)
            {
                TestContext.WriteLine(x.DumpString());
                throw;
            }
        }

        [TestMethod]
        public virtual void DetachTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;

                target.Add(entity);
                target.CommitChanges();

                target.Detach(entity);

                entity.StringProperty = "testValue";
                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                entity = target.GetByStoreId<TestEntity, long>(id);

                Assert.AreNotEqual("testValue", entity.StringProperty);
            }
        }

        [TestMethod]
        public virtual void GetByStoreIdExistingTest()
        {
            long id;
            string name;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                name = "test"+id;
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = name;
                entity.StringProperty = "testValue";

                target.Add(entity);
                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                entity = target.GetByStoreId<TestEntity, long>(id);

                Assert.IsNotNull(entity);
                Assert.AreEqual(name, entity.Name);
                Assert.AreEqual("testValue", entity.StringProperty);
            }
        }

        [TestMethod]
        public virtual void GetByStoreIdNotExistingTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>() + 1000;

                entity = target.GetByStoreId<TestEntity, long>(id);

                Assert.IsNull(entity);
            }
        }

        [TestMethod]
        public virtual void DeleteExistingFromContextTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;
                entity.StringProperty = "testValue";

                target.Add(entity);
                Assert.IsNotNull(target.GetByStoreId<TestEntity, long>(id));
                target.Delete(entity);
                Assert.IsNull(target.GetByStoreId<TestEntity, long>(id));

                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                Assert.IsNull(target.GetByStoreId<TestEntity, long>(id));
            }
        }

        [TestMethod]
        public virtual void DeleteExistingTest()
        {
            long id;
            string name;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                name = "test"+id;
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = name;                 
                entity.StringProperty = "testValue";

                target.Add(entity);
                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                entity = target.GetByStoreId<TestEntity, long>(id);

                Assert.IsNotNull(entity);
                Assert.AreEqual(name, entity.Name);
                Assert.AreEqual("testValue", entity.StringProperty);

                target.Delete(entity);
                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                Assert.IsNull(target.GetByStoreId<TestEntity, long>(id));
            }
        }

        [TestMethod]
        public virtual void DeleteNotExistingTest()
        {
            long id;
            IRepository target;
            TestEntity entity;

            using (target = GetInitializedRepository())
            {
                id = target.GetStoreId<TestEntity, long>();
                entity = target.CreateEntity<TestEntity>();

                entity.Id = id;
                entity.Name = "test"+id;
                entity.StringProperty = "testValue";

                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                Assert.IsNull(target.GetByStoreId<TestEntity, long>(id));

                target.Delete(entity);

                Assert.IsNull(target.GetByStoreId<TestEntity, long>(id));
            }
        }

        [TestMethod]
        public virtual void EntitiesTest()
        {
            IRepository target;
            TestEntity entity1;
            TestEntity entity2;
            TestEntity entity3;
            string name1, name2, name3;

            using (target = GetInitializedRepository())
            {
                entity1 = target.CreateEntity<TestEntity>();
                entity1.Id = target.GetStoreId<TestEntity, long>();
                entity1.Name = name1 = "test"+entity1.Id;
                target.Add(entity1);

                entity2 = target.CreateEntity<TestEntity1>();
                entity2.Id = target.GetStoreId<TestEntity1, long>();
                entity2.Name = name2 = "test"+entity2.Id;
                target.Add(entity2);

                entity3 = target.CreateEntity<TestEntity2>();
                entity3.Id = target.GetStoreId<TestEntity2, long>();
                entity3.Name = name3 = "test"+entity3.Id;
                target.Add(entity3);

                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                Assert.IsTrue(target.Entities<TestEntity>().Count() >= 3);

                var entity11 = target.Entities<TestEntity>()
                                                  .Where(o => o.Name == name1)
                                                  .FirstOrDefault();
                var entity12 = target.Entities<TestEntity>()
                                                  .Where(o => o.Name == name2)
                                                  .FirstOrDefault();
                var entity13 = target.Entities<TestEntity>()
                                                  .Where(o => o.Name == name3)
                                                  .FirstOrDefault();

                Assert.AreEqual(entity1.Id, entity11.Id);
                Assert.AreEqual(entity2.Id, entity12.Id);
                Assert.AreEqual(entity3.Id, entity13.Id);

                var entity14 = target.Entities<TestEntity>()
                                     .Where(o => o.Name == "test2424")
                                     .FirstOrDefault();

                Assert.IsNull(entity14);
            }
        }

        [TestMethod]
        public virtual void DetachedEntitiesTest()
        {
            IRepository target;
            TestEntity entity1;
            TestEntity entity2;
            TestEntity entity3;
            string name1, name2, name3;

            using (target = GetInitializedRepository())
            {
                entity1 = target.CreateEntity<TestEntity>();
                entity1.Id = target.GetStoreId<TestEntity, long>();
                entity1.Name = name1 = "test"+entity1.Id;
                target.Add(entity1);

                entity2 = target.CreateEntity<TestEntity1>();
                entity2.Id = target.GetStoreId<TestEntity1, long>();
                entity2.Name = name2 = "test"+entity2.Id;
                target.Add(entity2);

                entity3 = target.CreateEntity<TestEntity2>();
                entity3.Id = target.GetStoreId<TestEntity2, long>();
                entity3.Name = name3 = "test"+entity3.Id;
                target.Add(entity3);

                target.CommitChanges();
            }
            using (target = GetInitializedRepository())
            {
                Assert.IsTrue(target.DetachedEntities<TestEntity>().Count() >= 3);
                Assert.AreEqual(entity1, target.DetachedEntities<TestEntity>().Where(o => o.Name == name1).FirstOrDefault());
                Assert.AreEqual(entity2, target.DetachedEntities<TestEntity>().Where(o => o.Name == name2).FirstOrDefault());
                Assert.AreEqual(entity3, target.DetachedEntities<TestEntity>().Where(o => o.Name == name3).FirstOrDefault());
                Assert.IsNull(target.DetachedEntities<TestEntity>().Where(o => o.Name == "test2828").FirstOrDefault());

                entity1 = target.DetachedEntities<TestEntity>().Where(o => o.Name == name1).FirstOrDefault();
                entity1.StringProperty = "aha!";

                target.CommitChanges();
            }

            using (target = GetInitializedRepository())
            {
                entity1 = target.DetachedEntities<TestEntity>().Where(o => o.Name == name1).FirstOrDefault();
                Assert.AreNotEqual("aha!", entity1.StringProperty);
            }
        }
    }
}
