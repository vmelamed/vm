using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// With the help of this class the developer can create a synchronized writer scope by utilizing the <c>using</c> statement.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// class Protected
    /// {
    ///     static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    ///     static Dictionary<string, string> _protected = new Dictionary<string, string>();
    ///     
    ///     public void Add(string key, string value)
    ///     {
    ///         using(new WriterSlimSync(_loc))
    ///             _protected[key] = value;
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="T:ReaderSlimSync"/>, <seealso cref="T:WriterSlimSync"/>
    public sealed class WriterSlimSync : IDisposable
    {
        readonly ReaderWriterLockSlim _readerWriterLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriterSlimSync"/> class with the specified <paramref name="readerWriterLock"/> and
        /// waits indefinitely till it acquires the lock in writer mode.
        /// </summary>
        /// <param name="readerWriterLock">The reader-writer lock.</param>
        public WriterSlimSync(
            ReaderWriterLockSlim readerWriterLock)
        {
            Contract.Requires<ArgumentNullException>(readerWriterLock != null, nameof(readerWriterLock));

            readerWriterLock.EnterWriteLock();
            _readerWriterLock = readerWriterLock;
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag is being set when the object gets disposed.
        /// </summary>
        /// <value>
        /// 0 - if the object is not disposed yet, any other value would mean that the object is already disposed.
        /// </value>
        int _disposed;

        /// <summary>
        /// Returns <see langword="true"/> if the object has already been disposed, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                _readerWriterLock.ExitWriteLock();
        }
        #endregion
    }
}
