using System;
using System.Threading;

namespace vm.Aspects.Diagnostics.Implementation
{
    /// <summary>
    /// With the help of this class the developer can create a synchronized reader upgradeable to writer scope by utilizing the <c>using</c> statement.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// class Protected
    /// {
    ///     static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    ///     static Dictionary<string, string>; _protected = new Dictionary<string, string>();
    ///
    ///     public void Add(string key, string value)
    ///     {
    ///         using(new UpgradeableReaderSlimSync(_lock))
    ///         {
    ///             string v;
    ///             if (_protected.TryGetValue(key, out v))
    ///                 throw ArgumentException("The key already exists.", "key");
    ///
    ///             using(new WriterSlimSync(_lock))
    ///                 _protected.Add(key, value);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="ReaderSlimSync"/>, <seealso cref="WriterSlimSync"/>
    public sealed class UpgradeableReaderSlimSync : IDisposable
    {
        readonly ReaderWriterLockSlim _readerWriterLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriterSlimSync"/> class with the specified <paramref name="readerWriterLock"/> and
        /// waits indefinitely until it acquires the lock in upgradable reader mode.
        /// </summary>
        /// <param name="readerWriterLock">The reader writer lock.</param>
        public UpgradeableReaderSlimSync(
            ReaderWriterLockSlim readerWriterLock)
        {
            if (readerWriterLock == null)
                throw new ArgumentNullException(nameof(readerWriterLock));

            readerWriterLock.EnterUpgradeableReadLock();
            _readerWriterLock = readerWriterLock;
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set when the object gets disposed.
        /// </summary>
        /// <value>
        /// 0 - if the object is not disposed yet, any other value would mean that the object is already disposed.
        /// </value>
        int _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                _readerWriterLock.ExitUpgradeableReadLock();
        }
        #endregion
    }
}
