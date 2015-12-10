using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Security.Cryptography.Ciphers.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class SymmetricKeyCipherBase is the base class for most of the ciphers and XML ciphers.
    /// Internally manages tasks associated with the symmetric key. Also
    /// implements <see cref="IKeyManagement"/> and the <see cref="IDisposable"/> pattern.
    /// </summary>
    [ContractClass(typeof(SymmetricKeyCipherBaseContract))]
    public abstract class SymmetricKeyCipherBase : IKeyManagement, IDisposable
    {
        #region Fields
        /// <summary>
        /// The object which is responsible for storing and retrieving the encrypted symmetric key 
        /// to and from the store with the determined store location name (e.g file I/O).
        /// </summary>
        IKeyStorageAsync _keyStorage;
        /// <summary>
        /// The underlying .NET symmetric cipher.
        /// </summary>
        readonly SymmetricAlgorithm _symmetric;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricKeyCipherBase"/> class with symmetric algorithm provider
        /// derived from the <paramref name="symmetricAlgorithmName"/>.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// Name of the symmetric algorithm. If the value is <see langword="null"/>, empty or consist of all whitespace characters,
        /// the class will try to:
        ///     <list type="number">
        ///         <item>
        ///             resolve the <see cref="T:Symmetric"/> from the common service locator, 
        ///             which gives the caller opportunity to plug-in their algorithm of choice, customized with their own parameters like key-length, mode, etc.;
        ///             and if not resolved will try to
        ///         </item>
        ///         <item>
        ///             resolve the name of the symmetric algorithm from the common service locator with a resolve name &quot;DefaultSymmetricEncryption&quot;;
        ///             and if not resolved 
        ///         </item>
        ///         <item>
        ///             will default to AES with key length of 256 bits.
        ///         </item>
        ///     </list>
        /// </param>
        protected SymmetricKeyCipherBase(
            string symmetricAlgorithmName)
        {
            var factory = ServiceLocatorWrapper.Default.GetInstance<ISymmetricAlgorithmFactory>();

            factory.Initialize(symmetricAlgorithmName);
            _symmetric = factory.Create();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the underlying .NET symmetric cipher.
        /// </summary>
        protected SymmetricAlgorithm Symmetric
        {
            get { return _symmetric; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance's symmetric key is initialized.
        /// </summary>
        protected bool IsSymmetricKeyInitialized { get; set; }

        /// <summary>
        /// Flag indicating whether to encrypt the initialization vector.
        /// </summary>
        public virtual bool ShouldEncryptIV { get; set; }
        #endregion

        /// <summary>
        /// Initializes the key storage by executing the key location strategy.
        /// </summary>
        /// <param name="symmetricKeyLocation">The name of the symmetric key location.</param>
        /// <param name="symmetricKeyLocationStrategy">The symmetric key location strategy.</param>
        /// <param name="keyStorage">The key storage.</param>
        protected void ResolveKeyStorage(
            string symmetricKeyLocation,
            IKeyLocationStrategy symmetricKeyLocationStrategy,
            IKeyStorageAsync keyStorage)
        {
            Contract.Ensures(KeyLocation != null, "Could not determine the key's physical location.");
            Contract.Ensures(_keyStorage != null, "Could not resolve the IKeyStorageAsync object.");

            try
            {
                if (symmetricKeyLocationStrategy == null)
                    symmetricKeyLocationStrategy = ServiceLocatorWrapper.Default.GetInstance<IKeyLocationStrategy>();
            }
            catch (ActivationException)
            {
                symmetricKeyLocationStrategy = new KeyLocationStrategy();
            }

            KeyLocation = symmetricKeyLocationStrategy.GetKeyLocation(symmetricKeyLocation);

            try
            {
                if (keyStorage == null)
                    keyStorage = ServiceLocatorWrapper.Default.GetInstance<IKeyStorageAsync>();
            }
            catch (ActivationException)
            {
                keyStorage = new KeyFile();
            }

            _keyStorage = keyStorage;
        }

        #region IKeyManagement Members
        /// <summary>
        /// Gets the physical storage location name of a symmetric key, e.g. the path and filename of a file.
        /// </summary>
        public virtual string KeyLocation { get; protected set; }

        /// <summary>
        /// Imports the symmetric key as a clear text.
        /// </summary>
        /// <param name="key">The key.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual void ImportSymmetricKey(
            byte[] key)
        {
            Symmetric.Key = key;

            var encryptedKey = EncryptSymmetricKey();

            _keyStorage.PutKey(encryptedKey, KeyLocation);
        }

        /// <summary>
        /// Exports the symmetric key as a clear text.
        /// </summary>
        /// <returns>Array of bytes of the symmetric key or <see langword="null"/> if the cipher does not have a symmetric key.</returns>
        public virtual byte[] ExportSymmetricKey()
        {
            InitializeSymmetricKey();
            return Symmetric.Key;
        }

        /// <summary>
        /// Asynchronously imports the symmetric key as a clear text.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// A <see cref="T:Task"/> object representing the process of asynchronously importing the symmetric key.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual async Task ImportSymmetricKeyAsync(
            byte[] key)
        {
            Symmetric.Key = key;
            await _keyStorage.PutKeyAsync(EncryptSymmetricKey(), KeyLocation);
        }

        /// <summary>
        /// Asynchronously exports the symmetric key as a clear text.
        /// </summary>
        /// <returns>
        /// A <see cref="T:Task"/> object representing the process of asynchronously exporting the symmetric key including the result -
        /// array of bytes of the symmetric key or <see langword="null"/> if the cipher does not have a symmetric key.
        /// </returns>
        public virtual async Task<byte[]> ExportSymmetricKeyAsync()
        {
            await InitializeSymmetricKeyAsync();
            return Symmetric.Key;
        }
        #endregion

        #region Initialization of the symmetric key
        /// <summary>
        /// Initializes the symmetric key for encryption.
        /// </summary>
        protected virtual void InitializeSymmetricKey()
        {
            Contract.Ensures(IsSymmetricKeyInitialized, "The key was not initialized properly.");

            if (IsSymmetricKeyInitialized)
                return;

            // get or create and put the file from/to its physical location
            if (_keyStorage.KeyLocationExists(KeyLocation))
                DecryptSymmetricKey(_keyStorage.GetKey(KeyLocation));
            else
            {
                Symmetric.GenerateKey();
                _keyStorage.PutKey(EncryptSymmetricKey(), KeyLocation);
            }

            IsSymmetricKeyInitialized = true;
        }

        /// <summary>
        /// Asynchronously initializes the symmetric key for encryption.
        /// </summary>
        /// <returns>
        /// A <see cref="T:Task"/> object representing the process of asynchronous initialization.
        /// </returns>
        protected virtual async Task InitializeSymmetricKeyAsync()
        {
            if (IsSymmetricKeyInitialized)
                return;

            // get or create and put the file from/to its location
            if (_keyStorage.KeyLocationExists(KeyLocation))
                DecryptSymmetricKey(await _keyStorage.GetKeyAsync(KeyLocation));
            else
            {
                Symmetric.GenerateKey();
                await _keyStorage.PutKeyAsync(EncryptSymmetricKey(), KeyLocation);
            }

            IsSymmetricKeyInitialized = true;
        }

        /// <summary>
        /// Encrypts the symmetric key in preparation to put it in the crypto-package.
        /// </summary>
        /// <returns>
        /// The bytes of the encrypted key.
        /// </returns>
        protected abstract byte[] EncryptSymmetricKey();

        /// <summary>
        /// Decrypts the symmetric key after retrieving it from the crypto-package.
        /// </summary>
        /// <param name="encryptedKey">
        /// The encrypted key.
        /// </param>
        protected abstract void DecryptSymmetricKey(
            byte[] encryptedKey);
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
            Contract.Ensures(_disposed!=0, "The object was not disposed successfully.");

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
        ~SymmetricKeyCipherBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="M:Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~SymmetricKeyCipherBase"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="M:Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                Symmetric.Dispose();

                var disposable = _keyStorage as IDisposable;

                if (disposable != null)
                    disposable.Dispose();
            }
        }
        #endregion

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(Symmetric != null, "The symmetric key was not instantiated.");
            Contract.Invariant(!IsDisposed, "The object was disposed.");
        }
    }
}
