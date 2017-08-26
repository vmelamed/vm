using System;

namespace vm.Aspects
{
    /// <summary>
    /// Supplements the <see cref="IDisposable"/> interface with <c>IsDisposed</c> property.
    /// </summary>
    public interface IIsDisposed
    {
        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        bool IsDisposed { get; }
    }
}
