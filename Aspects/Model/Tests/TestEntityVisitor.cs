
using System;
using vm.Aspects.Visitor;
namespace vm.Aspects.Model.Tests
{
    public class TestEntityVisitor : CatchallVisitor,
        IVisitor<TestEntity>,
        IVisitor<TestEntity1>,
        IVisitor<TestEntity2>
    {
        public int TestEntityVisitedCount { get; private set; }
        public int TestEntity1VisitedCount { get; private set; }
        public int TestEntity2VisitedCount { get; private set; }

        public void Visit(TestEntity obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            TestEntityVisitedCount++;
        }

        public void Visit(TestEntity1 obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            TestEntity1VisitedCount++;
        }

        public void Visit(TestEntity2 obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            TestEntity2VisitedCount++;
        }
    }
}
