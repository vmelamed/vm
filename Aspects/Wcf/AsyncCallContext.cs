using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using vm.Aspects.Threading;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Maintains a simple synchronized key-value collection to replace
    /// the one from the <see cref="CallContext"/> which is failing in asynchronous situations.
    /// </summary>
    public class AsyncCallContext
    {
        readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        IDictionary<string, object> _contextSlots = new Dictionary<string, object>();

        /// <summary>
        /// Gets the entry at the data slot with name <paramref name="slotName"/>.
        /// </summary>
        /// <param name="slotName">Name of the slot.</param>
        /// <returns>The object in the slot or <see langword="null"/>.</returns>
        public object GetData(string slotName)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(slotName), "The argument "+nameof(slotName)+" cannot be null, empty or consist of whitespace characters only.");

            object entry;

            using (_lock.ReaderLock())
                if (!_contextSlots.TryGetValue(slotName, out entry))
                    return null;

            return entry;
        }

        /// <summary>
        /// Sets the entry at the data slot with name <paramref name="slotName"/>.
        /// NOTE: if the slot is not empty and the current entry supports <see cref="IDisposable"/>, 
        /// it will be disposed before replacing it with the new <paramref name="entry"/>.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="entry">The entry.</param>
        public void SetData(string slotName, object entry)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(slotName), "The argument "+nameof(slotName)+" cannot be null, empty or consist of whitespace characters only.");

            using (_lock.WriterLock())
            {
                object oldEntry;

                if (_contextSlots.TryGetValue(slotName, out oldEntry))
                    oldEntry.Dispose();

                _contextSlots[slotName] = entry;
            }
        }

        /// <summary>
        /// Frees the data slot, if it exists. If the slot contains a disposable object - it will be disposed.
        /// </summary>
        /// <param name="slotName">Name of the slot.</param>
        public void FreeDataSlot(string slotName)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(slotName), "The argument "+nameof(slotName)+" cannot be null, empty or consist of whitespace characters only.");

            using (_lock.WriterLock())
            {
                object entry;

                if (_contextSlots.TryGetValue(slotName, out entry))
                    entry.Dispose();
            }
        }

        /// <summary>
        /// Removes all entries. The disposable ones will be disposed.
        /// </summary>
        public void Clear()
        {
            using (_lock.WriterLock())
            {
                foreach (var entry in _contextSlots.Values)
                    entry.Dispose();

                _contextSlots.Clear();
            }
        }

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        void ObjectInvariant()
        {
            Contract.Invariant(_contextSlots != null);
        }
    }
}
