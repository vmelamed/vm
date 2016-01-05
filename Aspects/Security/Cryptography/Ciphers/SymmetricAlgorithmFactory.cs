using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class <c>SymmetricAlgorithmFactory</c> encapsulates the strategy for determining and realizing the symmetric algorithm.
    /// </summary>
    public sealed class SymmetricAlgorithmFactory : ISymmetricAlgorithmFactory
    {
        /// <summary>
        /// The resolved symmetric algorithm name
        /// </summary>
        string _symmetricAlgorithmName;
        /// <summary>
        /// The generated symmetric factory
        /// </summary>
        Func<SymmetricAlgorithm> _symmetricAlgorithmFactory;
        /// <summary>
        /// A temporary symmetric algorithm object.
        /// </summary>
        SymmetricAlgorithm _symmetricAlgorithm;

        #region ISymmetricAlgorithmFactory Members

        /// <summary>
        /// Initializes the factory with an optional symmetric algorithm name.
        /// Possibly implements the resolution strategy and initializes the factory with the appropriate values.
        /// </summary>
        /// <param name="symmetricAlgorithmName">Name of the symmetric algorithm.</param>
        /// <exception cref="Microsoft.Practices.ServiceLocation.ActivationException"></exception>
        public void Initialize(
            string symmetricAlgorithmName = null)
        {
            // 1. If the user passed symmetric algorithm name that is not null, empty or whitespace characters only, 
            //    it will be used in creating the <see cref="T:Symmetric"/> object.
            if (!string.IsNullOrWhiteSpace(symmetricAlgorithmName))
                _symmetricAlgorithmName = symmetricAlgorithmName;
            else
            {
                try
                {
                    // 2. try to resolve the symmetric algorithm object directly from the CSL
                    _symmetricAlgorithmFactory = () => ServiceLocatorWrapper.Default.GetInstance<SymmetricAlgorithm>();
                    _symmetricAlgorithm = _symmetricAlgorithmFactory();
                    // if we are here, we've got our factory.
                    return;
                }
                catch (ActivationException)
                {
                    // the symmetric algorithm is not registered in the CSL
                    _symmetricAlgorithmFactory = null;
                }

                try
                {
                    // 3. Try to resolve the name of the symmetric algorithm from the CSL with resolve name "DefaultSymmetricAlgorithm" 
                    _symmetricAlgorithmName = ServiceLocatorWrapper.Default.GetInstance<string>(Algorithms.Symmetric.ResolveName);
                }
                catch (ActivationException)
                {
                }

                if (string.IsNullOrWhiteSpace(_symmetricAlgorithmName))
                    // 4. if the symmetric algorithm name has not been resolved so far, assume the default algorithm:
                    _symmetricAlgorithmName = Algorithms.Symmetric.Default;
            }

            // set the factory
            _symmetricAlgorithmFactory = () => SymmetricAlgorithm.Create(_symmetricAlgorithmName);

            // try it, if successful we'll store it and return it in the first call to Create()
            _symmetricAlgorithm = _symmetricAlgorithmFactory();

            if (_symmetricAlgorithm == null)
                // if unsuccessful - throw an exception.
                throw new ActivationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The name \"{0}\" was not recognized as a valid symmetric algorithm.",
                                _symmetricAlgorithmName));
        }

        /// <summary>
        /// Creates a <see cref="T:Symmetric" /> instance.
        /// </summary>
        /// <returns><see cref="T:Symmetric" /> instance.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public SymmetricAlgorithm Create()
        {
            if (_symmetricAlgorithmFactory == null)
                throw new InvalidOperationException("The factory was not initialized properly. Call Initialize first.");

            if (_symmetricAlgorithm == null)
                return _symmetricAlgorithmFactory();

            var symmetricAlgorithm = _symmetricAlgorithm;

            _symmetricAlgorithm = null;
            return symmetricAlgorithm;
        }

        /// <summary>
        /// Gets the name of the symmetric algorithm.
        /// </summary>
        /// <value>The name of the symmetric algorithm.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public string SymmetricAlgorithmName
        {
            get { return _symmetricAlgorithmName; }
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

            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows the object to attempt to free resources and perform other cleanup operations before it is reclaimed by garbage collection. 
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(false)"/>.</remarks>
        ~SymmetricAlgorithmFactory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="M:Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~SymmetricAlgorithmFactory"/>.
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
            if (disposing && _symmetricAlgorithm != null)
                _symmetricAlgorithm.Dispose();
        }
        #endregion
    }
}
