using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
#if NET45
using System.Threading.Tasks;
#endif

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The class <c>Hasher</c> computes and verifies the cryptographic hash of data for maintaining its integrity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item><description>If the salt length is greater than 0, the length of the salt (serialized Int32) - 4 bytes.</description></item>
    ///         <item><description>If the salt length is greater than 0, the bytes of the salt.</description></item>
    ///         <item><description>Length of the hash (serialized Int32) - 4 bytes.</description></item>
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
        /// Caches the hash algorithm factory
        /// </summary>
        readonly IHashAlgorithmFactory _hashAlgorithmFactory;
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
            Contract.Requires<ArgumentException>(saltLength==0 || saltLength>=DefaultSaltLength, "The salt length should be either 0 or not less than 8 bytes.");

            _hashAlgorithmFactory = ServiceLocatorWrapper.Default.GetInstance<IHashAlgorithmFactory>();
            _hashAlgorithmFactory.Initialize(hashAlgorithmName);

            _saltLength = saltLength;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether the hash should be salted.
        /// </summary>
        public bool ShouldSalt
        {
            get { return SaltLength > 0; }
        }

        /// <summary>
        /// Gets the name of the hash algorithm.
        /// </summary>
        /// <value>The name of the hash algorithm.</value>
        public string HashAlgorithmName
        {
            get { return _hashAlgorithmFactory.HashAlgorithmName; }
        }
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
                throw new ArgumentException("The data stream cannot be read.", "dataStream");

            using (var hashAlgorithm = _hashAlgorithmFactory.Create())
            using (var hashStream = CreateHashStream(hashAlgorithm))
            {
                var salt = WriteSalt(hashStream, null);

                dataStream.CopyTo(hashStream);
                return FinalizeHashing(hashStream, hashAlgorithm, salt);
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        public virtual bool TryVerifyHash(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream == null)
                return hash==null;

            // save the property value - it may change for this call only depending on the length of the hash
            var savedSaltLength = SaltLength;

            using (var hashAlgorithm = _hashAlgorithmFactory.Create())
                try
                {
                    // the parameter hash has the length of the expected product from this algorithm, i.e. there is no salt
                    if (hash.Length == hashAlgorithm.HashSize/8)
                        SaltLength = 0;
                    else
                        // the parameter hash has the length of the expected product from this algorithm + the length of the salt, i.e. there is salt in the parameter salt
                        if (hash.Length > hashAlgorithm.HashSize/8)
                            SaltLength = hash.Length - hashAlgorithm.HashSize/8;
                        else
                            // this is wrong...
                            return false;

                    using (var hashStream = CreateHashStream(hashAlgorithm))
                    {
                        WriteSalt(hashStream, hash);
                        dataStream.CopyTo(hashStream);

                        byte[] computedHash = FinalizeHashing(hashStream, hashAlgorithm, hash);

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

            using (var hashAlgorithm = _hashAlgorithmFactory.Create())
            using (var hashStream = CreateHashStream(hashAlgorithm))
            {
                var salt = WriteSalt(hashStream, null);

                hashStream.Write(data, 0, data.Length);
                return FinalizeHashing(hashStream, hashAlgorithm, salt);
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        public virtual bool TryVerifyHash(
            byte[] data,
            byte[] hash)
        {
            if (data == null)
                return hash==null;

            // save the property value - it may change for this call only
            var savedSaltLength = SaltLength;

            using (var hashAlgorithm = _hashAlgorithmFactory.Create())
                try
                {
                    // the parameter hash has the length of the expected product from this algorithm, i.e. there is no salt
                    if (hash.Length == hashAlgorithm.HashSize/8)
                        SaltLength = 0;
                    else
                        // the parameter hash has the length of the expected product from this algorithm + the length of the salt, i.e. there is salt in the parameter salt
                        if (hash.Length > hashAlgorithm.HashSize/8)
                            SaltLength = hash.Length - hashAlgorithm.HashSize/8;
                        else
                            // this is wrong...
                            return false;

                    using (var hashStream = CreateHashStream(hashAlgorithm))
                    {
                        WriteSalt(hashStream, hash);
                        hashStream.Write(data, 0, data.Length);

                        byte[] computedHash = FinalizeHashing(hashStream, hashAlgorithm, hash);

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

#if NET45
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

            using (var hashAlgorithm = _hashAlgorithmFactory.Create())
            using (var hashStream = CreateHashStream(hashAlgorithm))
            {
                var salt = await WriteSaltAsync(hashStream, null);

                await dataStream.CopyToAsync(hashStream);
                return FinalizeHashing(hashStream, hashAlgorithm, salt);
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

            // save the property value - it may change for this call only depending on the length of the hash
            var savedSaltLength = SaltLength;

            using (var hashAlgorithm = _hashAlgorithmFactory.Create())
                try
                {
                    // the hash has the same length as the length of the key - there is no salt
                    if (hash.Length == hashAlgorithm.HashSize/8)
                        SaltLength = 0;
                    else
                        // the hash has the same length as the length of the key + the length of the salt - there is salt in the parameter salt
                        if (hash.Length > hashAlgorithm.HashSize/8)
                            SaltLength = hash.Length - hashAlgorithm.HashSize/8;
                        else
                            // this is wrong...
                            return false;

                    using (var hashStream = CreateHashStream(hashAlgorithm))
                    {
                        await WriteSaltAsync(hashStream, hash);
                        await dataStream.CopyToAsync(hashStream);

                        byte[] computedHash = FinalizeHashing(hashStream, hashAlgorithm, hash);

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
#endif

        #region Primitives called by the GoF method templates.
        /// <summary>
        /// Creates the crypto stream.
        /// </summary>
        /// <returns>CryptoStream.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="It will be disposed by the calling code.")]
        protected virtual CryptoStream CreateHashStream(
            HashAlgorithm hashAlgorithm)
        {
            Contract.Requires<ArgumentNullException>(hashAlgorithm != null, "hashAlgorithm");

            return new CryptoStream(new NullStream(), hashAlgorithm, CryptoStreamMode.Write);
        }

        /// <summary>
        /// Writes the salt (if any) into the crypto stream.
        /// </summary>
        /// <param name="hashStream">The hash stream.</param>
        /// <param name="salt">The salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hashStream"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected virtual byte[] WriteSalt(
            CryptoStream hashStream,
            byte[] salt)
        {
            Contract.Requires<ArgumentNullException>(hashStream != null, "hashStream");
            Contract.Requires<ArgumentException>(hashStream.CanWrite, "The argument \"hashStream\" cannot be written to.");

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
        /// <param name="hashAlgorithm">The hash algorithm.</param>
        /// <param name="salt">The salt.</param>
        /// <returns>The hash.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hashStream" /> is <see langword="null" />.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="2", Justification="salt is conditionally validated.")]
        protected virtual byte[] FinalizeHashing(
            CryptoStream hashStream,
            HashAlgorithm hashAlgorithm,
            byte[] salt)
        {
            Contract.Requires<ArgumentNullException>(hashStream != null, "hashStream");
            Contract.Requires<ArgumentException>(hashStream.CanWrite, "The argument \"hashStream\" cannot be written to.");
            Contract.Requires<ArgumentNullException>(hashAlgorithm != null, "hashAlgorithm");
            Contract.Requires<ArgumentNullException>(!ShouldSalt || salt != null, "salt");

            if (!hashStream.HasFlushedFinalBlock)
                hashStream.FlushFinalBlock();

            var hash = new byte[SaltLength + hashAlgorithm.HashSize/8];

            if (SaltLength > 0)
                salt.CopyTo(hash, 0);

            hashAlgorithm.Hash.CopyTo(hash, SaltLength);

            return hash;
        }
        #endregion

#if NET45
        #region Async primitives called by the GoF method templates.
        /// <summary>
        /// Writes the salt (if any) into the crypto stream.
        /// </summary>
        /// <param name="hashStream">The hash stream.</param>
        /// <param name="salt">The salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hashStream"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected virtual async Task<byte[]> WriteSaltAsync(
            CryptoStream hashStream,
            byte[] salt)
        {
            Contract.Requires<ArgumentNullException>(hashStream != null, "hashStream");
            Contract.Requires<ArgumentException>(hashStream.CanWrite, "The argument \"hashStream\" cannot be written to.");

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
#endif

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

#if NET40
        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed
        {
            get { return _disposed != 0; }
        }
#else
        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed
        {
            get { return Volatile.Read(ref _disposed) != 0; }
        }
#endif
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(true)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification="It is correct.")]
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
        ~Hasher()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="M:Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~Hasher"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="M:Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="T:IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _hashAlgorithmFactory.Dispose();
        }
        #endregion

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(_hashAlgorithmFactory != null, "The hash algorithm factory cannot be null.");
            Contract.Invariant(_saltLength==0 || _saltLength>=DefaultSaltLength, "Invalid salt length.");
        }
    }
}
