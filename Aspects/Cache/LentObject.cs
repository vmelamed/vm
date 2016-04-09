using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;

namespace vm.Aspects.Cache
{
    /// <summary>
    /// Class LentObject wraps an object lent from an object pool. When the object is lent the pool returns an instance of this class.
    /// The actual object can be accessed from the property <see cref="Instance"/>. The easiest way to return the object to the pool
    /// is to dispose the instance of <c>LentObject</c>.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the object pool.</typeparam>
    public class LentObject<T> : IDisposable where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LentObject{T}"/> class.
        /// </summary>
        /// <param name="instance">The object.</param>
        /// <param name="pool">The pool from which the object is lent.</param>
        internal LentObject(
            T instance,
            ObjectPool<T> pool)
        {
            Contract.Requires<ArgumentNullException>(instance != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(pool != null, nameof(pool));

            Instance = instance;
            Pool     = pool;
        }

        /// <summary>
        /// Gets the lent object.
        /// </summary>
        public T Instance { get; }

        /// <summary>
        /// Gets the object pool from which the object was lent.
        /// </summary>
        internal ObjectPool<T> Pool { get; }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(bool)"/>.</remarks>
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
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(bool)"/>.</remarks>
        ~LentObject()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="M:Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~LentObject"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="M:Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="T:IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(
            bool disposing)
        {
            if (disposing)
                Pool.ReturnObject(this);
        }
        #endregion

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(Instance != null, "The lent object is null.");
            Contract.Invariant(Pool != null, "The reference to the object pool is null.");
        }
    }
}
