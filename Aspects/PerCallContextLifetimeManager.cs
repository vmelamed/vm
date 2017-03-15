using Microsoft.Practices.Unity;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Threading;
using vm.Aspects.Threading;

namespace vm.Aspects
{
    /// <summary>
    /// Class PerCallContextLifetimeManager. Used for objects which lifetime should end with the end of the current 
    /// .NET remoting or WCF call context. The objects are stored in the current <see cref="T:System.Runtime.Remoting.Messaging.CallContext"/>.
    /// </summary>
    [PermissionSet(SecurityAction.LinkDemand)]
    public class PerCallContextLifetimeManager : LifetimeManager, IDisposable
    {
        ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Gets the key of the object stored in the call context.
        /// </summary>
        public string Key { get; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Retrieve a value from the backing store associated with this Lifetime policy.
        /// </summary>
        /// <returns>the object desired, or null if no such object is currently stored.</returns>
        public override object GetValue()
        {
            object value;
            
            using (_sync.ReaderLock())
                value = CallContext.LogicalGetData(Key);

            return value;
        }

        /// <summary>
        /// Stores the given value into backing store for retrieval later.
        /// </summary>
        /// <param name="newValue">The object being stored.</param>
        public override void SetValue(object newValue)
        {
            if (newValue == null)
                RemoveValue();
            else
                using (_sync.WriterLock())
                    CallContext.LogicalSetData(Key, newValue);
        }

        /// <summary>
        /// Remove the given object from backing store.
        /// </summary>
        public override void RemoveValue()
        {
            object value;

            using (_sync.WriterLock())
            {
                value = CallContext.LogicalGetData(Key);
                CallContext.LogicalSetData(Key, null);
            }

            value.Dispose();
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
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // these will be called only if the instance is not disposed and is not in a process of disposing.
            _sync.Dispose();
        }
        #endregion
    }
}