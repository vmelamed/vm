using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using vm.Aspects.Threading;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Maintains a simple synchronized key-value collection to replace
    /// the one from the <see cref="CallContext"/> which is failing in asynchronous situations.
    /// </summary>
    public class AsyncCallContext : IDisposable, IIsDisposed
    {
        readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly IDictionary<string, object> _contextSlots = new Dictionary<string, object>();

        /// <summary>
        /// Gets the current asynchronous call context.
        /// </summary>
        public static AsyncCallContext Current => CallContext.LogicalGetData(nameof(AsyncCallContext)) as AsyncCallContext;

        /// <summary>
        /// Gets the entry at the data slot with name <paramref name="slotName"/>.
        /// </summary>
        /// <param name="slotName">Name of the slot.</param>
        /// <returns>The object in the slot or <see langword="null"/>.</returns>
        public object GetData(
            string slotName)
        {
            Contract.Requires<ArgumentNullException>(slotName!=null, nameof(slotName));
            Contract.Requires<ArgumentException>(slotName.Length > 0, "The argument "+nameof(slotName)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(slotName.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(slotName)+" cannot be empty or consist of whitespace characters only.");

            object entry;

            using (_lock.UpgradableReaderLock())
            {
                if (!_contextSlots.TryGetValue(slotName, out entry))
                    return null;

                var disposed = entry as IIsDisposed;

                if (disposed != null  &&  disposed.IsDisposed)
                {
                    using (_lock.WriterLock())
                        _contextSlots.Remove(slotName);
                    return null;
                }
            }

            return entry;
        }

        /// <summary>
        /// Sets the entry at the data slot with name <paramref name="slotName"/>.
        /// NOTE: if the slot is not empty and the current entry supports <see cref="IDisposable"/>, 
        /// it will be disposed before replacing it with the new <paramref name="entry"/>.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="entry">The entry.</param>
        public void SetData(
            string slotName,
            object entry)
        {
            Contract.Requires<ArgumentNullException>(slotName!=null, nameof(slotName));
            Contract.Requires<ArgumentException>(slotName.Length > 0, "The argument "+nameof(slotName)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(slotName.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(slotName)+" cannot be empty or consist of whitespace characters only.");

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
        public void FreeDataSlot(
            string slotName)
        {
            Contract.Requires<ArgumentNullException>(slotName!=null, nameof(slotName));
            Contract.Requires<ArgumentException>(slotName.Length > 0, "The argument "+nameof(slotName)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(slotName.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(slotName)+" cannot be empty or consist of whitespace characters only.");

            object entry;

            using (_lock.WriterLock())
                if (_contextSlots.TryGetValue(slotName, out entry))
                    _contextSlots.Remove(slotName);

            if (entry != null)
                entry.Dispose();
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

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <see langword="true"/> if the object has already been disposed, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows the object to attempt to free resources and perform other cleanup operations before it is reclaimed by garbage collection. 
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/> with parameter <see langword="false"/>.</remarks>
        ~AsyncCallContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the finalizer.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/>==<see langword="true"/>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <see langword="false"/> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _lock.Dispose();
        }
        #endregion
    }
}
