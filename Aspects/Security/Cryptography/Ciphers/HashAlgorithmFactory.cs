using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class <c>HashAlgorithmFactory</c> encapsulates the strategy for determining and realizing the hash algorithm.
    /// </summary>
    public sealed class HashAlgorithmFactory : IHashAlgorithmFactory
    {
        /// <summary>
        /// The resolved hash algorithm name
        /// </summary>
        string _hashAlgorithmName;
        /// <summary>
        /// The generated hash factory
        /// </summary>
        Func<HashAlgorithm> _hashFactory;
        /// <summary>
        /// A temporary hash algorithm object.
        /// </summary>
        HashAlgorithm _hashAlgorithm;

        #region IHashAlgorithmFactory members
        /// <summary>
        /// Initializes a new instance of the <see cref="HashAlgorithmFactory"/> class with optional hash algorithm name.
        /// </summary>
        /// <param name="hashAlgorithmName">Name of the hash algorithm.</param>
        /// <exception cref="ActivationException">
        /// If the supplied hash algorithm name is not valid.
        /// </exception>
        /// <remarks>
        /// Implements the strategy for resolving the hash algorithm:
        /// <list type="number">
        ///     <item>
        ///         If the user passed hash algorithm name that is not <see langword="null"/>, empty or whitespace characters only, 
        ///         it is used in creating the <see cref="T:HashAlgorithm"/> object; otherwise
        ///     </item>
        ///     <item>
        ///         Try to resolve the <see cref="T:HashAlgorithm"/> object directly from CSL; otherwise 
        ///     </item>
        ///     <item>
        ///         Try to resolve the name of the hash algorithm from the CSL with resolve name <see cref="F:HashAlgorithmResolveName"/>; otherwise
        ///     </item>
        ///     <item>
        ///         Assume the default hash algorithm - <see cref="F:Algorithms.Hash.Default"/> - SHA256.
        ///     </item>
        /// </list>
        /// </remarks>
        public void Initialize(
            string hashAlgorithmName = null)
        {
            // 1. If the user passed hash algorithm name that is not null, empty or whitespace characters only, 
            //    it will be used in creating the <see cref="T:HashAlgorithm"/> object.
            if (!string.IsNullOrWhiteSpace(hashAlgorithmName))
                _hashAlgorithmName = hashAlgorithmName;
            else
            {
                try
                {
                    // 2. try to resolve the hash algorithm object directly from the CSL
                    _hashFactory = () => ServiceLocatorWrapper.Default.GetInstance<HashAlgorithm>();
                    _hashAlgorithm = _hashFactory();
                    // if we are here, we've got our factory.
                    return;
                }
                catch (ActivationException)
                {
                    // the hash algorithm is not registered in the CSL
                    _hashFactory = null;
                }

                try
                {
                    // 3. Try to resolve the name of the hash algorithm from the CSL with resolve name "DefaultHash" 
                    _hashAlgorithmName = ServiceLocatorWrapper.Default.GetInstance<string>(Algorithms.Hash.ResolveName);
                }
                catch (ActivationException)
                {
                }

                if (string.IsNullOrWhiteSpace(_hashAlgorithmName))
                    // 4. if the hash algorithm name has not been resolved so far, assume the default algorithm:
                    _hashAlgorithmName = Algorithms.Hash.Default;
            }

            // set the factory
            _hashFactory = () => HashAlgorithm.Create(_hashAlgorithmName);

            // try it
            _hashAlgorithm = _hashFactory();
            if (_hashAlgorithm == null)
                // if unsuccessful - throw an exception.
                throw new ActivationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The name \"{0}\" was not recognized as a valid hash algorithm.",
                                _hashAlgorithmName));
        }

        /// <summary>
        /// Creates a <see cref="T:HashAlgorithm"/> instance.
        /// </summary>
        /// <returns><see cref="T:HashAlgorithm"/> instance.</returns>
        /// <exception cref="T:InvalidOperationException">
        /// If the factory could not resolve the hash algorithm.
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
        /// Gets the name of the hash algorithm.
        /// </summary>
        /// <value>The name of the hash algorithm.</value>
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
        ~HashAlgorithmFactory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="M:Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~HashAlgorithmFactory"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="M:Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="T:IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        /*protected virtual*/
        void Dispose(bool disposing)
        {
            if (disposing && _hashAlgorithm != null)
                _hashAlgorithm.Dispose();
        }
        #endregion
    }
}
