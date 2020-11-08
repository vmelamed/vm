using System.Threading;

namespace vm.Aspects.Diagnostics.Implementation
{
    /// <summary>
    /// Class ReaderWriterLockSlimExtensions. Utility class for better management of the lifetime of the scope of <see cref="ReaderWriterLockSlim"/>
    /// </summary>
    internal static class ReaderWriterLockSlimExtensions
    {
        /// <summary>
        /// Gets the upgradable reader slim sync. Merely a shortcut to <c>new UpgradeableReaderSlimLock(readerWriterLock)</c>.
        /// </summary>
        /// <param name="readerWriterLock">
        /// The reader writer lock.
        /// </param>
        /// <returns>
        /// <see cref="UpgradeableReaderSlimSync"/> object.</returns>
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
        ///         using(_lock.UpgradableReaderLock())
        ///         {
        ///             string v;
        ///             if (_protected.TryGetValue(key, out v))
        ///                 throw ArgumentException("The key already exists.", "key");
        ///
        ///             using(_lock.WriterLock())
        ///                 _protected.Add(key, value);
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static UpgradeableReaderSlimSync UpgradableReaderLock(this ReaderWriterLockSlim readerWriterLock) =>
            new UpgradeableReaderSlimSync(readerWriterLock);

        /// <summary>
        /// Gets the reader slim sync. Mere of a shortcut to <c>new ReaderSlimSync(readerWriterLock)</c> however shows nicely in intellisense.
        /// </summary>
        /// <param name="readerWriterLock">The reader writer lock.</param>
        /// <returns><see cref="ReaderSlimSync"/> object.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// class Protected
        /// {
        ///     static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        ///     static Dictionary<string, string> _protected = new Dictionary<string, string>();
        ///
        ///     public string Get(string key)
        ///     {
        ///         using(_lock.ReaderLock())
        ///             return _protected.GetValue[key];
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static ReaderSlimSync ReaderLock(this ReaderWriterLockSlim readerWriterLock) =>
            new ReaderSlimSync(readerWriterLock);

        /// <summary>
        /// Gets the reader slim sync. Mere of a shortcut to <c>new WriterSlimSync(readerWriterLock)</c> however shows nicely in intellisense.
        /// </summary>
        /// <param name="readerWriterLock">The reader writer lock.</param>
        /// <returns><see cref="WriterSlimSync"/> object.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// class Protected
        /// {
        ///     static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        ///     static Dictionary<string, string> _protected = new Dictionary<string, string>();
        ///
        ///     public string Get(string key)
        ///     {
        ///         using(_lock.WriterLock())
        ///             return _protected.GetValue[key];
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static WriterSlimSync WriterLock(this ReaderWriterLockSlim readerWriterLock) =>
            new WriterSlimSync(readerWriterLock);
    }
}
