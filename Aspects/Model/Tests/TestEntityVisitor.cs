
using System;
namespace vm.Aspects.Model.Tests
{
    public class TestEntityVisitor
    {
        public int TestEntityVisitedCount { get; private set; }
        public int TestEntity1VisitedCount { get; private set; }
        public int TestEntity2VisitedCount { get; private set; }

        public void Visit(
            object visited)
        {
            if (visited == null)
                throw new ArgumentNullException(nameof(visited));

            throw new NotImplementedException($"The visitor {GetType().Name} does not implement IVisitor<{visited.GetType().Name}>.");
        }

        public void Visit(TestEntity obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            TestEntityVisitedCount++;
        }

        public void Visit(TestEntity1 obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            TestEntity1VisitedCount++;
        }

        public void Visit(TestEntity2 obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            TestEntity2VisitedCount++;
        }
    }
}
