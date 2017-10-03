using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Exceptions;
using vm.Aspects.Validation;
using vm.Aspects.Visitor;

namespace vm.Aspects.Model.Tests
{
    public class TestValueVisitor : CatchallVisitor,
        IVisitor<TestValue>
    {
        public int TestValueVisitedCount { get; private set; }

        public void Visit(TestValue obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            TestValueVisitedCount++;
        }
    }

    [TestClass]
    public class ValueTests
    {

        [TestMethod]
        public void AcceptedVisitorsTest()
        {
            var target = new TestValue { Name = "12345", };
            var visitor = new TestValueVisitor();

            target.Accept(visitor);

            Assert.AreEqual(1, visitor.TestValueVisitedCount);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void UnacceptedVisitorTest()
        {
            var target = new TestValue { Name = "12345", };
            var visitor = new TestEntityVisitor();

            target.Accept(visitor);
        }

        [TestMethod]
        public void ValidationTest()
        {
            var target = new TestValue
            {
                Name = "gogo",
            };

            Assert.IsTrue(target.IsValid());
            Assert.IsNotNull(target.ConfirmValid());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidObjectException))]
        public void InvalidTest()
        {
            var target = new TestValue
            {
                Name = "0123456789012345678901234567890",
            };

            Assert.IsFalse(target.IsValid());
            target.ConfirmValid();
        }
    }
}
