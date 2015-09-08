using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Cache;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Summary description for ObjectPoolTests
    /// </summary>
    [TestClass]
    public class ObjectPoolTests
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

        class TestObject
        {
            public int Id { get; set; }
        }

        [TestMethod]
        public void Initialize1Test()
        {
            using (var target = new ObjectPool<TestObject>())
            {
                var i = 0;

                target.Initialize(16, () => new TestObject { Id = i++ }, true);

                Assert.IsTrue(target.IsInitialized);
                Assert.AreEqual(16, target.PoolSize);
                Assert.AreEqual(0, target.AvailableInstances);
            }
        }

        [TestMethod]
        public void Initialize2Test()
        {
            using (var target = new ObjectPool<TestObject>())
            {
                var i = 0;

                target.Initialize(16, () => new TestObject { Id = i++ }, false);

                Assert.IsTrue(target.IsInitialized);
                Assert.AreEqual(16, target.PoolSize);
                Assert.AreEqual(16, target.AvailableInstances);
            }
        }

        [TestMethod]
        public void Initialize3Test()
        {
            var i = 0;

            using (var target = new ObjectPool<TestObject>(16, () => new TestObject { Id = i++ }, false))
            {
                Assert.IsTrue(target.IsInitialized);
                Assert.AreEqual(16, target.PoolSize);
                Assert.AreEqual(16, target.AvailableInstances);
            }
        }

        [TestMethod]
        public void LendObject1Test()
        {
            var i = 0;

            using (var target = new ObjectPool<TestObject>(2, () => new TestObject { Id = i++ }))
            using (var test1 = target.LendObject())
            {
                Assert.IsNotNull(test1);
                Assert.IsNotNull(test1.Instance);
                Assert.IsNotNull(test1.Pool);
                Assert.AreEqual(test1.Pool, target);
            }
        }

        [TestMethod]
        public void LendObject2Test()
        {
            var i = 0;

            using (var target = new ObjectPool<TestObject>(2, () => new TestObject { Id = i++ }))
            using (var test1 = target.LendObject())
            {
                Assert.IsNotNull(test1);
                Assert.IsNotNull(test1.Instance);
                Assert.IsNotNull(test1.Pool);
                Assert.AreEqual(test1.Pool, target);

                using (var test2 = target.LendObject())
                {
                    Assert.IsNotNull(test1);
                    Assert.IsNotNull(test1.Instance);
                    Assert.IsNotNull(test1.Pool);
                    Assert.AreEqual(test1.Pool, target);

                    using (var test3 = target.LendObject(1))
                        Assert.IsNull(test3);
                }
            }
        }

        [TestMethod]
        public void LendObject3Test()
        {
            var i = 0;

            using (var target = new ObjectPool<TestObject>(2, () => new TestObject { Id = i++ }, false))
            using (var test1 = target.LendObject())
            {
                Assert.IsNotNull(test1);
                Assert.IsNotNull(test1.Instance);
                Assert.IsNotNull(test1.Pool);
                Assert.AreEqual(test1.Pool, target);

                using (var test2 = target.LendObject())
                {
                    Assert.IsNotNull(test1);
                    Assert.IsNotNull(test1.Instance);
                    Assert.IsNotNull(test1.Pool);
                    Assert.AreEqual(test1.Pool, target);

                    using (var test3 = target.LendObject(1))
                        Assert.IsNull(test3);
                }
            }
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReturnObjectTest()
        {
            var i = 0;

            using (var target = new ObjectPool<TestObject>(2, () => new TestObject { Id = i++ }, false))
            {
                var test = target.LendObject();

                using (var target1 = new ObjectPool<TestObject>(2, () => new TestObject { Id = i++ }, false))
                    target1.ReturnObject(test);
            }
        }

        [TestMethod]
        public void LendObjectAsync1Test()
        {
            var i = 0;

            using (var target = new ObjectPool<TestObject>(2, () => new TestObject { Id = i++ }))
            using (var test1 = target.LendObjectAsync().Result)
            {
                Assert.IsNotNull(test1);
                Assert.IsNotNull(test1.Instance);
                Assert.IsNotNull(test1.Pool);
                Assert.AreEqual(test1.Pool, target);
            }
        }

        [TestMethod]
        public void LendObjectAsync2Test()
        {
            var i = 0;

            using (var target = new ObjectPool<TestObject>(2, () => new TestObject { Id = i++ }))
            using (var test1 = target.LendObjectAsync().Result)
            {
                Assert.IsNotNull(test1);
                Assert.IsNotNull(test1.Instance);
                Assert.IsNotNull(test1.Pool);
                Assert.AreEqual(test1.Pool, target);

                using (var test2 = target.LendObjectAsync().Result)
                {
                    Assert.IsNotNull(test1);
                    Assert.IsNotNull(test1.Instance);
                    Assert.IsNotNull(test1.Pool);
                    Assert.AreEqual(test1.Pool, target);

                    using (var test3 = target.LendObjectAsync(1).Result)
                        Assert.IsNull(test3);
                }
            }
        }

        [TestMethod]
        public void LendObjectAsync3Test()
        {
            var i = 0;

            using (var target = new ObjectPool<TestObject>(2, () => new TestObject { Id = i++ }, false))
            using (var test1 = target.LendObjectAsync().Result)
            {
                Assert.IsNotNull(test1);
                Assert.IsNotNull(test1.Instance);
                Assert.IsNotNull(test1.Pool);
                Assert.AreEqual(test1.Pool, target);

                using (var test2 = target.LendObjectAsync().Result)
                {
                    Assert.IsNotNull(test1);
                    Assert.IsNotNull(test1.Instance);
                    Assert.IsNotNull(test1.Pool);
                    Assert.AreEqual(test1.Pool, target);

                    using (var test3 = target.LendObjectAsync(1).Result)
                        Assert.IsNull(test3);
                }
            }
        }
    }
}
