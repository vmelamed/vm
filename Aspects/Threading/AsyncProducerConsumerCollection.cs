using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// <c>AsyncProducerConsumerCollection</c> coordinates between asynchronous activities: 
    /// producer (produces data) and consumer (consumes the data) which may be running in parallel.
    /// </summary>
    /// <typeparam name="T">The type of the items produced by the producer and consumed by the consumer.</typeparam>
    /// <example>
    /// <![CDATA[
    /// AsyncProducerConsumerCollection<int> data = new AsyncProducerConsumerCollection<int>();
    /// 
    /// // the consumer's activity:
    /// async Task ConsumerAsync()
    /// {
    ///     while (true)
    ///     {
    ///         int next = await data.Take();
    ///         
    ///         Process(next);
    ///     }
    /// }
    /// 
    /// // the producer's activity:
    /// void Produce()
    /// {
    ///     while (true)
    ///     {
    ///         int next = GetNext();
    ///         
    ///         data.Add(next);
    ///     }
    /// }
    /// ]]>
    /// </example>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class AsyncProducerConsumerCollection<T>
    {
        readonly Queue<T> _collection = new Queue<T>();
        readonly Queue<TaskCompletionSource<T>> _waiting = new Queue<TaskCompletionSource<T>>();

        /// <summary>
        /// The producer adds the specified item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            TaskCompletionSource<T> tcs = null;

            lock (_collection)
                if (_waiting.Count > 0)
                    tcs = _waiting.Dequeue();
                else
                    _collection.Enqueue(item);

            if (tcs != null)
                tcs.TrySetResult(item);
        }

        /// <summary>
        /// The consumer takes an instance from the collection.
        /// </summary>
        /// <returns>Task{T}.</returns>
        public Task<T> Take()
        {
            lock (_collection)
                if (_collection.Count > 0)
                    return Task.FromResult(_collection.Dequeue());
                else
                {
                    var tcs = new TaskCompletionSource<T>();

                    _waiting.Enqueue(tcs);
                    return tcs.Task;
                }
        }
    }
}
