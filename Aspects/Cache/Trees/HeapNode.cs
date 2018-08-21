using System;

namespace vm.Aspects.Cache.Trees
{
    class HeapNode<T> where T : IComparable<T>
    {
        public HeapNode(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value stored in this node.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the parent of this node.
        /// </summary>
        public HeapNode<T> Parent { get; set; }

        /// <summary>
        /// Gets or sets the left sub-heap.
        /// </summary>
        HeapNode<T> Left { get; set; }

        /// <summary>
        /// Gets or sets the right sub-heap.
        /// </summary>
        HeapNode<T> Right { get; set; }

        /// <summary>
        /// Gets the weight of the sub-heap starting here.
        /// </summary>
        public int Weight { get; private set; }

        public void Add(HeapNode<T> newNode)
        {
            if (Left == null  ||  Right == null)
            {
                newNode.Parent = this;
                if (Left == null)
                    Left = newNode;
                else
                    Right = newNode;
            }
            else
            {
            }
            Weight++;
        }
    }
}
