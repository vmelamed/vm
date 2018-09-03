using System;
using System.Threading;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// With the help of this class the developer can create a synchronized multiple readers/single writer scope by utilizing the <c>using</c> statement.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// class Protected
    /// {
    ///     static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    ///     static Dictionary<string, string>; _protected = new Dictionary<string, string>();
    ///     
    ///     public void Get(string key)
    ///     {
    ///         using(new ReaderSlimSync(_lock))
    ///             return _protected[key];
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="T:System.Threading.WriterSlimSync"/>, <seealso cref="UpgradeableReaderSlimSync"/>, <seealso cref="WriterSlimSync"/>.
    sealed class ReaderSlimSync : IDisposable
    {
        readonly ReaderWriterLockSlim _readerWriterLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderSlimSync"/> class with the specified <paramref name="readerWriterLock"/> and
        /// waits indefinitely till it acquires the lock in reader mode.
        /// </summary>
        /// <param name="readerWriterLock">The reader writer lock.</param>
        public ReaderSlimSync(
            ReaderWriterLockSlim readerWriterLock)
        {
            if (readerWriterLock == null)
                throw new ArgumentNullException(nameof(readerWriterLock));

            readerWriterLock.EnterReadLock();
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
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                _readerWriterLock.ExitReadLock();
        }
        #endregion
    }
}
