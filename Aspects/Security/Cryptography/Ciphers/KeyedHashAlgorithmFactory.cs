using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class <c>KeyedHashAlgorithmFactory</c> encapsulates the strategy for determining and realizing the keyed hash algorithm.
    /// </summary>
    public sealed class KeyedHashAlgorithmFactory : IHashAlgorithmFactory
    {
        /// <summary>
        /// The resolved keyed hash algorithm name
        /// </summary>
        string _hashAlgorithmName;
        /// <summary>
        /// The generated keyed hash factory
        /// </summary>
        Func<KeyedHashAlgorithm> _hashFactory;
        /// <summary>
        /// A temporary keyed hash algorithm object.
        /// </summary>
        KeyedHashAlgorithm _hashAlgorithm;

        #region IHashAlgorithmFactory members
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedHashAlgorithmFactory"/> class with optional keyed hash algorithm name.
        /// </summary>
        /// <param name="keyedHashAlgorithmName">Name of the keyed hash algorithm.</param>
        /// <exception cref="ActivationException">
        /// If the supplied keyed hash algorithm name is not valid.
        /// </exception>
        /// <remarks>
        /// Implements the strategy for resolving the keyed hash algorithm:
        /// <list type="number">
        ///     <item>
        ///         If the user passed keyed hash algorithm name that is not <see langword="null"/>, empty or whitespace characters only, 
        ///         it is used in creating the <see cref="KeyedHashAlgorithm"/> object; otherwise
        ///     </item>
        ///     <item>
        ///         Try to resolve the <see cref="KeyedHashAlgorithm"/> object directly from CSL; otherwise 
        ///     </item>
        ///     <item>
        ///         Try to resolve the name of the keyed hash algorithm from the CSL with resolve name <see cref="Algorithms.KeyedHash.ResolveName"/>; otherwise
        ///     </item>
        ///     <item>
        ///         Assume the default keyed hash algorithm - <see cref="Algorithms.KeyedHash.Default"/> - SHA256.
        ///     </item>
        /// </list>
        /// </remarks>
        public void Initialize(
            string keyedHashAlgorithmName = null)
        {
            // 1. If the user passed keyed hash algorithm name that is not null, empty or whitespace characters only, 
            //    it will be used in creating the <see cref="KeyedHashAlgorithm"/> object.
            if (!string.IsNullOrWhiteSpace(keyedHashAlgorithmName))
                _hashAlgorithmName = keyedHashAlgorithmName;
            else
            {
                try
                {
                    // 2. try to resolve the keyed hash algorithm object directly from the CSL
                    _hashFactory = () => ServiceLocatorWrapper.Default.GetInstance<KeyedHashAlgorithm>();
                    _hashAlgorithm = _hashFactory();
                    // if we are here, we've got our factory.
                    return;
                }
                catch (ActivationException)
                {
                    // the keyed hash algorithm is not registered in the CSL
                    _hashFactory = null;
                }

                try
                {
                    // 3. Try to resolve the name of the keyed hash algorithm from the CSL with resolve name "DefaultHash" 
                    _hashAlgorithmName = ServiceLocatorWrapper.Default.GetInstance<string>(Algorithms.KeyedHash.ResolveName);
                }
                catch (ActivationException)
                {
                }

                if (string.IsNullOrWhiteSpace(_hashAlgorithmName))
                    // 4. if the keyed hash algorithm name has not been resolved so far, assume the default algorithm:
                    _hashAlgorithmName = Algorithms.Hash.Default;
            }

            // set the factory
            _hashFactory = () => KeyedHashAlgorithm.Create(_hashAlgorithmName);

            // try it
            _hashAlgorithm = _hashFactory();
            if (_hashAlgorithm == null)
                // if unsuccessful - throw an exception.
                throw new ActivationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The name \"{0}\" was not recognized as a valid keyed hash algorithm.",
                                _hashAlgorithmName));
        }

        /// <summary>
        /// Creates a <see cref="KeyedHashAlgorithm"/> instance.
        /// </summary>
        /// <returns><see cref="HashAlgorithm"/> instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the factory could not resolve the keyed hash algorithm.
        /// </exception>
        public HashAlgorithm Create()
        {
            if (_hashFactory == null)
                throw new InvalidOperationException("The factory was not initialized properly. Call Initialize first.");

            if (_hashAlgorithm == null)
                return _hashFactory();

            var hashAlgorithm = _hashAlgorithm;

            _hashAlgorithm = null;
            return hashAlgorithm;
        }

        /// <summary>
        /// Gets the name of the keyed hash algorithm.
        /// </summary>
        /// <value>The name of the keyed hash algorithm.</value>
        public string HashAlgorithmName
        {
            get { return _hashAlgorithmName; }
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="M:Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed
        {
            get { return Volatile.Read(ref _disposed) != 0; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(true)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // these will be called only if the instance is not disposed yet or is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows the object to attempt to free resources and perform other cleanup operations before it is reclaimed by garbage collection. 
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(false)"/>.</remarks>
        ~KeyedHashAlgorithmFactory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~KeyedHashAlgorithmFactory"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        void Dispose(bool disposing)
        {
            if (disposing && _hashAlgorithm != null)
                _hashAlgorithm.Dispose();
        }
        #endregion
    }
}
