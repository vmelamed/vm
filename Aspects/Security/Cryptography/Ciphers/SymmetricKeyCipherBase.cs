using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class SymmetricKeyCipherBase is the base class for most of the ciphers and XML ciphers.
    /// Internally manages tasks associated with the symmetric key. Also
    /// implements <see cref="IKeyManagement"/> and the <see cref="IDisposable"/> pattern.
    /// </summary>
    public abstract class SymmetricKeyCipherBase : IKeyManagement, IDisposable
    {
        #region Fields
        /// <summary>
        /// The underlying .NET symmetric cipher.
        /// </summary>
        SymmetricAlgorithm _symmetric;
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
        ///             resolve the <see cref="Symmetric"/> from the common service locator, 
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
            CreateSymmetricKey(symmetricAlgorithmName);
        }
        #endregion

        /// <summary>
        /// Creates the symmetric key.
        /// </summary>
        /// <param name="symmetricAlgorithmName">Name of the symmetric algorithm.</param>
        protected void CreateSymmetricKey(
            string symmetricAlgorithmName)
        {
            var factory = ServiceLocatorWrapper.Default.GetInstance<ISymmetricAlgorithmFactory>();

            factory.Initialize(symmetricAlgorithmName);
            _symmetric = factory.Create();
        }

        #region Properties
        /// <summary>
        /// The object which is responsible for storing and retrieving the encrypted symmetric key 
        /// to and from the store with the determined store location name (e.g file I/O).
        /// </summary>
        protected IKeyStorageAsync KeyStorage { get; set; }

        /// <summary>
        /// Gets the underlying .NET symmetric cipher.
        /// </summary>
        protected SymmetricAlgorithm Symmetric
            => _symmetric;

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

            KeyStorage = keyStorage;
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
            KeyStorage?.PutKey(EncryptSymmetricKey(), KeyLocation);
            IsSymmetricKeyInitialized = true;
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
        /// A <see cref="Task"/> object representing the process of asynchronously importing the symmetric key.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual async Task ImportSymmetricKeyAsync(
            byte[] key)
        {
            Symmetric.Key = key;
            await KeyStorage?.PutKeyAsync(EncryptSymmetricKey(), KeyLocation);
        }

        /// <summary>
        /// Asynchronously exports the symmetric key as a clear text.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> object representing the process of asynchronously exporting the symmetric key including the result -
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
            if (IsSymmetricKeyInitialized)
                return;

            // get or create and put the file from/to its physical location
            if (KeyStorage.KeyLocationExists(KeyLocation))
                DecryptSymmetricKey(KeyStorage.GetKey(KeyLocation));
            else
            {
                Symmetric.GenerateKey();
                KeyStorage.PutKey(EncryptSymmetricKey(), KeyLocation);
            }

            IsSymmetricKeyInitialized = true;
        }

        /// <summary>
        /// Asynchronously initializes the symmetric key for encryption.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> object representing the process of asynchronous initialization.
        /// </returns>
        protected virtual async Task InitializeSymmetricKeyAsync()
        {
            if (IsSymmetricKeyInitialized)
                return;

            // get or create and put the file from/to its location
            if (KeyStorage.KeyLocationExists(KeyLocation))
                DecryptSymmetricKey(await KeyStorage.GetKeyAsync(KeyLocation));
            else
            {
                Symmetric.GenerateKey();
                await KeyStorage.PutKeyAsync(EncryptSymmetricKey(), KeyLocation);
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
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(true)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~SymmetricKeyCipherBase"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(
            bool disposing)
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing)
            {
                Symmetric.Dispose();

                var disposable = KeyStorage as IDisposable;

                if (disposable != null)
                    disposable.Dispose();
            }
        }
        #endregion

        /// <summary>
        /// Copies certain characteristics of this instance to the <paramref name="cipher"/> parameter.
        /// The goal is to produce a cipher with the same encryption/decryption behavior but saving the key encryption and decryption ceremony and overhead if possible.
        /// Here it creates a <see cref="SymmetricAlgorithm"/> object identical to the current <see cref="Symmetric"/> and assigns it to <paramref name="cipher"/>'s <see cref="Symmetric"/>.
        /// </summary>
        /// <param name="cipher">The cipher that gets the identical symmetric algorithm object.</param>
        protected virtual void CopyTo(
            SymmetricKeyCipherBase cipher)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (!IsSymmetricKeyInitialized)
                return;

            cipher._symmetric = SymmetricAlgorithm.Create(Symmetric.GetType().FullName);

            cipher.Symmetric.Mode            = Symmetric.Mode;
            cipher.Symmetric.Padding         = Symmetric.Padding;
            cipher.Symmetric.BlockSize       = Symmetric.BlockSize;
            cipher.Symmetric.FeedbackSize    = Symmetric.FeedbackSize;
            cipher.Symmetric.KeySize         = Symmetric.KeySize;
            cipher.Symmetric.Key             = (byte[])Symmetric.Key.Clone();
            cipher.Symmetric.IV              = (byte[])Symmetric.IV.Clone();

            cipher.IsSymmetricKeyInitialized = true;
            cipher.ShouldEncryptIV           = ShouldEncryptIV;
        }
    }
}
