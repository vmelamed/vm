using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Exceptions;
using vm.Aspects.Facilities;
using vm.Aspects.Validation;

namespace vm.Aspects.Model.Tests
{
    /// <summary>
    /// Summary description for EntityTests
    /// </summary>
    [TestClass]
    public class EntityTests : IdentityTester<DomainEntity<long, string>>
    {
        public TestContext TestContext { get; set; }

        public EntityTests()
        {
            Initialize(
                new TestEntity() { Name = "vm", },
                new TestEntity() { Name = "vm", },
                new TestEntity() { Name = "vm", },
                new TestEntity() { Name = "vm1", },
                e => { });
        }

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

        [TestMethod]
        public void RegistrationLookupOperatorEqualsTest()
        {
            var target1 = new TestEntity() { Name = "vm", };
            var target2 = new TestEntity() { Name = "vm", };
            var target3 = new TestEntity() { Name = "vm", };
            var target4 = new TestEntity() { Name = "vm1", };

            Assert.IsTrue(!(target1==(TestEntity)null), "target1 must not be equal to null.");
            Assert.IsTrue(!((TestEntity)null==target1), "target1 must not be equal to obj1.");

            // reflexitivity
            var t = target1;

            Assert.IsTrue(target1==t, "The operator == must be reflexive.");
            Assert.IsFalse(target1!=t, "The operator == must be reflexive.");

            // symmetricity
            Assert.AreEqual(target1==target2, target2==target1, "The operator == must be symmetric.");
            Assert.AreEqual(target1!=target4, target4!=target1, "The operator != must be symmetric.");

            // transityvity
            Assert.IsTrue(target1==target2 && target2==target3 && target3==target1, "The operator == must be transitive.");
            Assert.IsTrue(target1==target2 && target1!=target4 && target2!=target4, "The operator != must be transitive.");
        }

        [TestMethod]
        public void AcceptedVisitorsTest()
        {
            var vm = new TestEntity() { Name = "vm", };
            var vm1 = new TestEntity1() { Name = "vm1", };
            var vm2 = new TestEntity2() { Name = "vm2", };

            var visitor = new TestEntityVisitor();

            vm.Accept(visitor);
            vm1.Accept(visitor);
            vm2.Accept(visitor);

            Assert.AreEqual(1, visitor.TestEntityVisitedCount);
            Assert.AreEqual(1, visitor.TestEntity1VisitedCount);
            Assert.AreEqual(1, visitor.TestEntity2VisitedCount);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void UnacceptedVisitorTest()
        {
            var target = new TestXEntity();
            var visitor = new TestEntityVisitor();

            target.Accept(visitor);
        }

        [TestMethod]
        public void ValidationTest()
        {
            var target = new TestXEntity
            {
                Id = 1,
                Name = "gogo",
                StringProperty = "",
                Created = Facility.Clock.UtcNow,
                Updated = Facility.Clock.UtcNow,
            };

            Assert.IsTrue(target.IsValid());
            Assert.IsNotNull(target.ConfirmValid());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidObjectException))]
        public void InvalidTest()
        {
            var target = new TestXEntity
            {
                Id = 1,
                Name = "",
                StringProperty = "0123456789012345678901234567890",
                //Created = Facility.Clock.UtcNow,
                //Updated = Facility.Clock.UtcNow,
            };

            try
            {
                Assert.IsFalse(target.IsValid());
                target.ConfirmValid();
            }
            catch (Exception x)
            {
                TestContext.WriteLine("{0}", target);
                TestContext.WriteLine("{0}", x.DumpString());
                throw;
            }
        }
    }
}
