using System;

namespace vm.Aspects.Cache.Trees
{
    public class Heap<T> where T : IComparable<T>
    {
        HeapNode<T> Head { get; set; }

        public void Add(T value)
        {
            var newNode = new HeapNode<T>(value);

            if (Head == null)
                Head = newNode;
            else
                Head.Add(newNode);
        }
    }
}
