using System;
namespace vm.Aspects
{
    /// <summary>
    /// Supplies objects with conditional 'Dispose' method (if they implement <see cref="IDisposable"/>).
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Disposes the specified lazily instantiated object, if it supports <see cref="IDisposable"/>.
        /// </summary>
        /// <typeparam name="T">The type of the lazy instantiated object.</typeparam>
        /// <param name="lazy">The lazy instantiated object.</param>
        public static void Dispose<T>(this Lazy<T> lazy)
        {
            if (lazy == null)
                throw new ArgumentNullException(nameof(lazy));

            if (!lazy.IsValueCreated)
                return;

            if (lazy.Value is IDisposable disposable)
                disposable.Dispose();
        }

        /// <summary>
        /// Disposes the specified object instance if it supports <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="instance">The object.</param>
        public static void Dispose(this object instance)
        {
            if (instance is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
