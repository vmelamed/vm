using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// Can be used as a one time action (e.g. initialization), thread-safe indicator that does not use the if-lock-if-again pattern.
    /// </summary>
    /// <example>
    /// The following double-checking pattern:
    /// <![CDATA[
    ///     bool _isInitialized;
    ///     object _sync = new object();
    ///     ...
    ///     if (_isInitialized == 0)
    ///         lock(_syncObject)
    ///         {
    ///             if (_isInitialized == 0)
    ///             {
    ///                 _isInitialized = 1;
    ///                 Initialize();
    ///             }
    ///         }
    /// ]]>
    /// can be replaced by:
    /// <![CDATA[
    ///     Latch _latch = new Latch();
    ///     ...
    ///     if (_latch.Latched)
    ///         Initialize();
    /// ]]>
    /// </example>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not appropriate here.")]
    public sealed class Latch
    {
        int _latch;

        /// <summary>
        /// Gets a value indicating whether the latch has been locked already.
        /// </summary>
        public bool IsLatched => _latch != 0;

        /// <summary>
        /// Checks if the latch is already latched and if it is not - latches it, in a thread safe manner.
        /// </summary>
        /// <returns><c>true</c> if the latch latched on, <c>false</c> otherwise.</returns>
        public bool Latched() => Interlocked.CompareExchange(ref _latch, 1, 0) == 0;

        /// <summary>
        /// Resets the latch back to unlocked state.
        /// </summary>
        public void Reset() => Interlocked.Exchange(ref _latch, 0);
    }
}
