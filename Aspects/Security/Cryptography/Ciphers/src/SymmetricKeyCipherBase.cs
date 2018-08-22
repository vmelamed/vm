using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class SymmetricKeyCipherBase is the base class for most of the ciphers and XML ciphers.
    /// Internally manages tasks associated with the symmetric key. Also
    /// implements <see cref="IKeyManagement"/> and the <see cref="IDisposable"/> pattern.
    /// </summary>
    public abstract class SymmetricKeyCipherBase : IKeyManagement, IKeyManagementTasks, IDisposable
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricKeyCipherBase" /> class by instantiating a symmetric algorithm provider
        /// derived from the <paramref name="symmetricAlgorithmName" />.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// If <see langword="null" /> the algorithm will default to <see cref="Algorithms.Symmetric.Default"/> (AESManaged).
        /// Hint: use the constants in the <see cref="Algorithms.Symmetric" /> static class.
        /// </param>
        /// <param name="symmetricAlgorithmFactory">
        /// The symmetric algorithm factory.
        /// If <see langword="null"/> the constructor will create an instance of the default <see cref="DefaultServices.SymmetricAlgorithmFactory"/>,
        /// which uses the <see cref="SymmetricAlgorithm.Create(string)"/> method from the .NET library.
        /// </param>
        protected SymmetricKeyCipherBase(
            string symmetricAlgorithmName = Algorithms.Symmetric.Default,
            ISymmetricAlgorithmFactory symmetricAlgorithmFactory = null)
        {
            Symmetric = DefaultServices
                            .Resolver
                            .GetInstanceOrDefault(symmetricAlgorithmFactory)
                            .Initialize(symmetricAlgorithmName)
                            .Create();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The underlying .NET symmetric cipher.
        /// </summary>
        protected SymmetricAlgorithm Symmetric { get; private set; }

        /// <summary>
        /// The object which is responsible for storing and retrieving the encrypted symmetric key 
        /// to and from the store with the resolved store specific location name (e.g file name for file storages.)
        /// </summary>
        protected IKeyStorageTasks KeyStorage { get; set; }

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
        /// <param name="symmetricKeyLocation">
        /// The name of the symmetric key location which must be relevant to the chosen <see cref="IKeyLocationStrategy"/>.
        /// </param>
        /// <param name="symmetricKeyLocationStrategy">
        /// Translates the <paramref name="symmetricKeyLocation"/> to the chosen concrete key store relevant specific key location.
        /// </param>
        /// <param name="keyStorage">
        /// The key storage.
        /// </param>
        protected void ResolveKeyStorage(
            string symmetricKeyLocation,
            IKeyLocationStrategy symmetricKeyLocationStrategy,
            IKeyStorageTasks keyStorage)
        {
            KeyLocation = DefaultServices.Resolver.GetInstanceOrDefault(symmetricKeyLocationStrategy)
                                .GetKeyLocation(symmetricKeyLocation);
            KeyStorage  = DefaultServices.Resolver.GetInstanceOrDefault(keyStorage);
        }

        #region IKeyManagement Members
        /// <summary>
        /// Gets the physical storage location name of a symmetric key, e.g. the path and filename of a file.
        /// </summary>
        public virtual string KeyLocation { get; protected set; }

        /// <summary>
        /// Imports the symmetric key as a clear text into the current ciphers and stores the new key into the key storage.
        /// </summary>
        /// <param name="key">The key.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual void ImportSymmetricKey(
            byte[] key)
        {
            if (key == null  ||  key.Length == 0)
                throw new ArgumentException("The imported key cannot be null or empty array.", nameof(key));

            Symmetric.Key             = key;
            IsSymmetricKeyInitialized = true;

            KeyStorage?.PutKey(EncryptSymmetricKey(), KeyLocation);
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
            if (key == null  ||  key.Length == 0)
                throw new ArgumentException("The imported key cannot be null or empty array.", nameof(key));

            Symmetric.Key             = key;
            IsSymmetricKeyInitialized = true;

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

                if (KeyStorage is IDisposable disposable)
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

            cipher.Symmetric                 = SymmetricAlgorithm.Create(Symmetric.GetType().FullName);
            CopyToSymetricAlgorithm(cipher.Symmetric);
            cipher.IsSymmetricKeyInitialized = true;
            cipher.ShouldEncryptIV           = ShouldEncryptIV;
        }

        void CopyToSymetricAlgorithm(
            SymmetricAlgorithm symmetric)
        {
            symmetric.Mode            = Symmetric.Mode;
            symmetric.Padding         = Symmetric.Padding;
            symmetric.BlockSize       = Symmetric.BlockSize;
            symmetric.FeedbackSize    = Symmetric.FeedbackSize;
            symmetric.KeySize         = Symmetric.KeySize;
            symmetric.Key             = (byte[])Symmetric.Key.Clone();
            symmetric.IV              = (byte[])Symmetric.IV.Clone();
        }
    }
}
