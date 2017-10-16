using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The class <c>Hasher</c> computes and verifies the cryptographic hash of data for maintaining its integrity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item><description>If the salt length is greater than 0, the bytes of the salt.</description></item>
    ///         <item><description>The bytes of the hash.</description></item>
    ///     </list>
    /// </para>
    /// </remarks>
    public class Hasher : IHasherAsync
    {
        #region Constants
        /// <summary>
        /// The minimum salt length if not 0 - 8.
        /// </summary>
        public const int MinSaltLength = 8;
        /// <summary>
        /// The default salt length - 8.
        /// </summary>
        public const int DefaultSaltLength = MinSaltLength;
        #endregion

        #region Fields
        /// <summary>
        /// The underlying hash algorithm.
        /// </summary>
        readonly HashAlgorithm _hashAlgorithm;
        /// <summary>
        /// The salt length.
        /// </summary>
        int _saltLength;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Hasher" /> class.
        /// </summary>
        /// <param name="hashAlgorithmName">
        /// The hash algorithm name. You can use any of the constants from <see cref="Algorithms.Hash" /> or
        /// <see langword="null" />, empty or whitespace characters only - it will default to <see cref="Algorithms.Hash.Default" />.
        /// Also a string instance with name &quot;DefaultHash&quot; can be defined in a Common Service Locator compatible dependency injection container.
        /// </param>
        /// <param name="saltLength">
        /// The length of the salt. The default length is <see cref="DefaultSaltLength"/> bytes. Can be 0 or equal or greater than <see cref="DefaultSaltLength"/>.
        /// </param>
        public Hasher(
            string hashAlgorithmName = null,
            int saltLength = DefaultSaltLength)
        {
            if (saltLength != 0  &&  saltLength < DefaultSaltLength)
                throw new ArgumentException("The salt length should be either 0 or not less than 8 bytes.", nameof(saltLength));

            var hashAlgorithmFactory = ServiceLocatorWrapper.Default.GetInstance<IHashAlgorithmFactory>();

            hashAlgorithmFactory.Initialize(hashAlgorithmName);
            _hashAlgorithm = hashAlgorithmFactory.Create();
            _saltLength = saltLength;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the hash algorithm.
        /// </summary>
        public string HashAlgorithmName
        {
            get
            {


                return _hashAlgorithm.GetType().FullName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the hash should be salted.
        /// </summary>
        public bool ShouldSalt => SaltLength > 0;
        #endregion

        #region IHasher Members
        /// <summary>
        /// Gets or sets the length of the salt in bytes. If set to 0 salt will not be applied to the hash; otherwise the length must be equal or greater than <see cref="DefaultSaltLength"/>.
        /// </summary>
        /// <value>The length of the salt.</value>
        /// <exception cref="System.ArgumentException">The <paramref name="value"/> must be either 0 or at least <see cref="DefaultSaltLength"/> bytes.</exception>
        public virtual int SaltLength
        {
            get { return _saltLength; }
            set { _saltLength = value; }
        }

        /// <summary>
        /// Computes the hash of a <paramref name="dataStream" /> stream.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns>
        /// The hash of the stream optionally prepended with the generated salt or <see langword="null"/> if <paramref name="dataStream"/> is <see langword="null"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">The data stream cannot be read.</exception>
        public virtual byte[] Hash(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;

            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            _hashAlgorithm.Initialize();
            using (var hashStream = CreateHashStream())
            {
                var salt = WriteSalt(hashStream, null);

                dataStream.CopyTo(hashStream);
                return FinalizeHashing(hashStream, salt);
            }
        }

        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="dataStream" /> is correct.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The hash to verify, optionally prepended with salt.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="hash" /> is correct or <paramref name="hash" /> and <paramref name="dataStream"/> are both <see langword="null"/>, 
        /// otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hash"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the hash has an invalid size.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public virtual bool TryVerifyHash(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream != null  &&  hash == null)
                throw new ArgumentNullException(nameof(hash));

            if (dataStream == null)
                return hash == null;

            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            // save the property value - it may change for this call only depending on the length of the hash
            var savedSaltLength = SaltLength;

            try
            {
                _hashAlgorithm.Initialize();

                // the parameter hash has the length of the expected product from this algorithm, i.e. there is no salt
                if (hash.Length == _hashAlgorithm.HashSize/8)
                    SaltLength = 0;
                else
                {
                    // the parameter hash has the length of the expected product from this algorithm + the length of the salt, i.e. there is salt in the parameter salt
                    if (hash.Length > _hashAlgorithm.HashSize/8)
                        SaltLength = hash.Length - _hashAlgorithm.HashSize/8;
                    else
                        // this is wrong...
                        return false;
                }

                using (var hashStream = CreateHashStream())
                {
                    WriteSalt(hashStream, hash);
                    dataStream.CopyTo(hashStream);

                    byte[] computedHash = FinalizeHashing(hashStream, hash);

                    return computedHash.ConstantTimeEquals(hash);
                }
            }
            finally
            {
                // restore the value of the property
                SaltLength = savedSaltLength;
            }
        }

        /// <summary>
        /// Computes the hash of a specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <returns>The hash of the <paramref name="data" /> optionally prepended with the generated salt or <see langword="null" /> if <paramref name="data" /> is <see langword="null" />.
        /// </returns>
        public virtual byte[] Hash(
            byte[] data)
        {
            if (data == null)
                return null;

            _hashAlgorithm.Initialize();
            using (var hashStream = CreateHashStream())
            {
                var salt = WriteSalt(hashStream, null);

                hashStream.Write(data, 0, data.Length);
                return FinalizeHashing(hashStream, salt);
            }
        }

        /// <summary>
        /// Verifies the hash of the specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data which hash needs to be verified.</param>
        /// <param name="hash">The hash with optionally prepended salt to be verified.</param>
        /// <returns>
        /// <see langword="true" /> if the hash is correct or <paramref name="hash" /> and <paramref name="data"/> are both <see langword="null"/>, otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hash"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the hash is invalid.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public virtual bool TryVerifyHash(
            byte[] data,
            byte[] hash)
        {
            if (data != null  &&  hash == null)
                throw new ArgumentNullException(nameof(hash));

            if (data == null)
                return hash==null;

            if (hash == null)
                return false;

            // save the property value - it may change for this call only
            var savedSaltLength = SaltLength;

            try
            {
                _hashAlgorithm.Initialize();

                // the parameter hash has the length of the expected product from this algorithm, i.e. there is no salt
                if (hash.Length == _hashAlgorithm.HashSize/8)
                    SaltLength = 0;
                else
                {
                    // the parameter hash has the length of the expected product from this algorithm + the length of the salt, i.e. there is salt in the parameter salt
                    if (hash.Length > _hashAlgorithm.HashSize/8)
                        SaltLength = hash.Length - _hashAlgorithm.HashSize/8;
                    else
                        // this is wrong...
                        return false;
                }

                using (var hashStream = CreateHashStream())
                {
                    WriteSalt(hashStream, hash);
                    hashStream.Write(data, 0, data.Length);

                    byte[] computedHash = FinalizeHashing(hashStream, hash);

                    return computedHash.ConstantTimeEquals(hash);
                }
            }
            finally
            {
                // restore the property value
                SaltLength = savedSaltLength;
            }
        }
        #endregion

        #region IhasherAsync members
        /// <summary>
        /// Computes the hash of a <paramref name="dataStream" /> stream.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns>
        /// The hash of the stream optionally prepended with the generated salt or <see langword="null"/> if <paramref name="dataStream"/> is <see langword="null"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">The data stream cannot be read.</exception>
        public virtual async Task<byte[]> HashAsync(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            _hashAlgorithm.Initialize();
            using (var hashStream = CreateHashStream())
            {
                var salt = await WriteSaltAsync(hashStream, null);

                await dataStream.CopyToAsync(hashStream);
                return FinalizeHashing(hashStream, salt);
            }
        }

        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="dataStream" /> is correct.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The hash to verify, optionally prepended with salt.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="hash" /> is correct or <paramref name="hash" /> and <paramref name="dataStream"/> are both <see langword="null"/>, 
        /// otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hash"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the hash has an invalid size.</exception>
        public virtual async Task<bool> TryVerifyHashAsync(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream == null)
                return hash==null;
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            // save the property value - it may change for this call only depending on the length of the hash
            var savedSaltLength = SaltLength;

            try
            {
                _hashAlgorithm.Initialize();

                // the hash has the same length as the length of the key - there is no salt
                if (hash.Length == _hashAlgorithm.HashSize/8)
                    SaltLength = 0;
                else
                {
                    // the hash has the same length as the length of the key + the length of the salt - there is salt in the parameter salt
                    if (hash.Length > _hashAlgorithm.HashSize/8)
                        SaltLength = hash.Length - _hashAlgorithm.HashSize/8;
                    else
                        // this is wrong...
                        return false;
                }

                using (var hashStream = CreateHashStream())
                {
                    await WriteSaltAsync(hashStream, hash);
                    await dataStream.CopyToAsync(hashStream);

                    byte[] computedHash = FinalizeHashing(hashStream, hash);

                    return computedHash.ConstantTimeEquals(hash);
                }
            }
            finally
            {
                // restore the value of the property
                SaltLength = savedSaltLength;
            }
        }
        #endregion

        #region Primitives called by the GoF method templates.
        /// <summary>
        /// Creates the crypto stream.
        /// </summary>
        /// <returns>CryptoStream.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "It will be disposed by the calling code.")]
        protected virtual CryptoStream CreateHashStream() => new CryptoStream(new NullStream(), _hashAlgorithm, CryptoStreamMode.Write);

        /// <summary>
        /// Writes the salt (if any) into the crypto stream.
        /// </summary>
        /// <param name="hashStream">The hash stream.</param>
        /// <param name="salt">The salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hashStream"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual byte[] WriteSalt(
            CryptoStream hashStream,
            byte[] salt)
        {
            if (hashStream == null)
                throw new ArgumentNullException(nameof(hashStream));
            if (!hashStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(hashStream));

            if (!ShouldSalt)
                return null;

            if (salt == null)
            {
                salt = new byte[SaltLength];
                salt.FillRandom();
            }

            hashStream.Write(salt, 0, SaltLength);
            return salt;
        }

        /// <summary>
        /// Finalizes the hashing.
        /// </summary>
        /// <param name="hashStream">The hash stream.</param>
        /// <param name="salt">The salt.</param>
        /// <returns>The hash.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hashStream" /> is <see langword="null" />.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2", Justification = "salt is conditionally validated.")]
        protected virtual byte[] FinalizeHashing(
            CryptoStream hashStream,
            byte[] salt)
        {
            if (hashStream == null)
                throw new ArgumentNullException(nameof(hashStream));
            if (!hashStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(hashStream));
            if (ShouldSalt  &&  salt == null)
                throw new ArgumentNullException(nameof(salt));

            if (!hashStream.HasFlushedFinalBlock)
                hashStream.FlushFinalBlock();

            var hash = new byte[SaltLength + _hashAlgorithm.HashSize/8];

            if (SaltLength > 0)
                salt.CopyTo(hash, 0);

            _hashAlgorithm.Hash.CopyTo(hash, SaltLength);

            return hash;
        }
        #endregion

        #region Async primitives called by the GoF method templates.
        /// <summary>
        /// Writes the salt (if any) into the crypto stream.
        /// </summary>
        /// <param name="hashStream">The hash stream.</param>
        /// <param name="salt">The salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hashStream"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual async Task<byte[]> WriteSaltAsync(
            CryptoStream hashStream,
            byte[] salt)
        {
            if (hashStream == null)
                throw new ArgumentNullException(nameof(hashStream));
            if (!hashStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(hashStream));

            if (!ShouldSalt)
                return null;

            if (salt == null)
            {
                salt = new byte[SaltLength];
                salt.FillRandom();
            }

            await hashStream.WriteAsync(salt, 0, SaltLength);
            return salt;
        }
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
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~Hasher"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing)
                _hashAlgorithm.Dispose();
        }
        #endregion
    }
}
