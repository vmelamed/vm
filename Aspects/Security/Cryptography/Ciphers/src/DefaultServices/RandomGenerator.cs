using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Threading;

namespace vm.Aspects.Security.Cryptography.Ciphers.DefaultServices
{
    /// <summary>
    /// DefaultRandomGenerator generates cryptographically strong byte sequences.
    /// </summary>
    /// <seealso cref="IRandom" />
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public class RandomGenerator : IRandom, IDisposable
    {
        RandomNumberGenerator _random = new RNGCryptoServiceProvider();

        #region IRandom
        /// <summary>
        /// Fills an array of bytes with a cryptographically strong sequence of random values.
        /// </summary>
        /// <param name="data">The array to fills with a cryptographically strong sequence of random bytes.</param>
        public void GetBytes(byte[] data)
            => _random.GetBytes(data);
        #endregion

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
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_random")]
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // these will be called only if the instance is not disposed and is not in a process of disposing.
            (_random as IDisposable)?.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
