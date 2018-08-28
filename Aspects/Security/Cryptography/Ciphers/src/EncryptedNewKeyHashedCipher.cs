using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using vm.Aspects.Security.Cryptography.Ciphers.DefaultServices;
using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class <c>EncryptedNewKeyHashedCipher</c> adds a cryptographic hash to the crypto package.
    /// <remarks>
    /// <para>
    /// The asymmetric key is stored in a key container with a name specified by the caller or by a certificate containing the public
    /// and possibly the private key.
    /// </para><para>
    /// By default the cipher uses the <see cref="T:System.Security.Cryptography.SHA256Cng"/> algorithm implementation for hashing.
    /// </para><para>
    /// By default the cipher uses the <see cref="T:System.Security.Cryptography.AesCryptoServiceProvider"/> with default parameters. 
    /// This can be changed by reconfiguring the Unity container and injecting any of the other symmetric ciphers. 
    /// Reconfiguration of the container allows also to change the parameters of the symmetric cipher in use.
    /// </para><para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item><description>Length of the encrypted hash (serialized Int32) - 4 bytes. Must be equal to the hash algorithm's hash size divided by 8.</description></item>
    ///         <item><description>The bytes of the encrypted hash.</description></item>
    ///         <item><description>Length of the encrypted symmetric key (serialized Int32) - 4 bytes.</description></item>
    ///         <item><description>The bytes of the encrypted symmetric key.</description></item>
    ///         <item><description>Length of the symmetric initialization vector (serialized Int32) - 4 bytes. Must be equal to the symmetric block size divided by 8.</description></item>
    ///         <item><description>The bytes of the initialization vector.</description></item>
    ///         <item><description>The bytes of the encrypted text.</description></item>
    ///     </list>
    /// </para><para>
    /// The cipher can also be used to encrypt elements of or entire XML documents, however it does not add hash to the encrypted XML document.
    /// There is no XML standard for hashing. Or in other words with regards to XML documents this cipher is equivalent to <see cref="EncryptedNewKeyCipher"/>.
    /// If integrity of the XML document is a requirement, please use <see cref="EncryptedNewKeySignedCipher"/>.
    /// </para>
    /// </remarks>
    /// </summary>
    public class EncryptedNewKeyHashedCipher : EncryptedNewKeyCipher
    {
        #region Fields
        /// <summary>
        /// Caches the hash algorithm factory
        /// </summary>
        IHashAlgorithmFactory _hashAlgorithmFactory;
        /// <summary>
        /// The hasher responsible for generating the hash - temporary, created before and disposed after each crypto-operation.
        /// </summary>
        HashAlgorithm _hasher;
        /// <summary>
        /// Temporarily stores the hash to validate.
        /// </summary>
        byte[] _hash;
        /// <summary>
        /// Indicates whether the cipher should encrypt the hash too.
        /// </summary>
        bool _shouldEncryptHash = true;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedNewKeyCipher" /> class.
        /// </summary>
        /// <param name="certificate">
        /// The certificate containing the public and optionally the private encryption keys. Cannot be <see langword="null" />.
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or
        /// <see langword="null" />, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default" />.
        /// </param>
        /// <param name="hashAlgorithmName">
        /// The name of the hash algorithm. By default the cipher will pick the algorithm from the <paramref name="certificate"/> but the caller
        /// may choose to use lower length signature key, e.g. the certificate may be for SHA256 but the caller may override that to SHA1.
        /// </param>
        /// <param name="hashAlgorithmFactory">
        /// The hash algorithm factory.
        /// If <see langword="null" /> the constructor will create an instance of the <see cref="DefaultServices.HashAlgorithmFactory" />,
        /// which uses the <see cref="HashAlgorithm.Create(string)" /> method from the .NET library.
        /// </param>
        /// <param name="symmetricAlgorithmFactory">
        /// The symmetric algorithm factory.
        /// If <see langword="null" /> the constructor will create an instance of the <see cref="DefaultServices.SymmetricAlgorithmFactory" />,
        /// which uses the <see cref="SymmetricAlgorithm.Create(string)" /> method from the .NET library.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="certificate" /> is <see langword="null" />.
        /// </exception>
        public EncryptedNewKeyHashedCipher(
            X509Certificate2 certificate = null,
            string hashAlgorithmName = Algorithms.Hash.Default,
            string symmetricAlgorithmName = Algorithms.Symmetric.Default,
            IHashAlgorithmFactory hashAlgorithmFactory = null,
            ISymmetricAlgorithmFactory symmetricAlgorithmFactory = null)
            : base(certificate, symmetricAlgorithmName, symmetricAlgorithmFactory)
        {
            InitializeHasher(hashAlgorithmName, hashAlgorithmFactory);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the hash algorithm.
        /// </summary>
        protected virtual string HashAlgorithmName => _hashAlgorithmFactory.HashAlgorithmName;

        /// <summary>
        /// Gets the hasher.
        /// </summary>
        protected HashAlgorithm Hasher => _hasher;

        /// <summary>
        /// Gets a value indicating whether the cipher should encrypt the hash too.
        /// </summary>
        public virtual bool ShouldEncryptHash
        {
            get { return _shouldEncryptHash; }
            set { _shouldEncryptHash = value; }
        }
        #endregion

        #region ICipher overrides
        /// <summary>
        /// Gets or sets a value indicating whether the encrypted texts are or should be Base64 encoded.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// If you try to set the property to <see langword="true"/> it will always throw the exception.
        /// This cipher does not support Base64 transformation as it requires the encrypted stream to be seek-able. 
        /// You can perform Base64 encoding on the output/input outside of the cipher.
        /// </exception>
        public override bool Base64Encoded
        {
            get => false;

            set
            {
                if (value)
                    throw new InvalidOperationException("This cipher does not support Base64 transformation because this requires the encrypted stream to be seek-able. "+
                                                        "You can perform Base64 encoding on the output/input outside of the cipher.");
            }
        }

        /// <summary>
        /// Reads the clear text from the <paramref name="dataStream" /> encrypts it and writes the result into the <paramref name="encryptedStream" />
        /// stream. This is the reverse method of <see cref="M:Decrypt(System.Stream, System.Stream)" />.
        /// </summary>
        /// <param name="dataStream">The unencrypted input stream.</param>
        /// <param name="encryptedStream">The output stream where to write the crypto package which will contain the encrypted data
        /// as well as some other crypto artifacts, e.g. initialization vector. This cipher requires that the encrypted stream is seek-able.</param>
        /// <exception cref="System.ArgumentException">The encrypted stream must be seek-able.</exception>
        public override void Encrypt(
            Stream dataStream,
            Stream encryptedStream)
        {
            if (dataStream == null)
                throw new ArgumentNullException(nameof(dataStream));
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));
            if (!encryptedStream.CanSeek)
                throw new ArgumentException(Resources.StreamNotSeekable, nameof(encryptedStream));

            base.Encrypt(dataStream, encryptedStream);
        }

        /// <summary>
        /// Reads and decrypts the <paramref name="encryptedStream" /> stream and writes the clear text into the <paramref name="dataStream" /> stream.
        /// This is the reverse method of <see cref="M:Encrypt(System.Stream, System.Stream)" />.
        /// </summary>
        /// <param name="encryptedStream">The input crypto package stream which contains the encrypted data
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc. This cipher requires that the encrypted stream is seek-able.</param>
        /// <param name="dataStream">The output stream where to put the unencrypted data.</param>
        /// <exception cref="System.ArgumentException">The encrypted stream must be seek-able.</exception>
        public override void Decrypt(
            Stream encryptedStream,
            Stream dataStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (dataStream == null)
                throw new ArgumentNullException(nameof(dataStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));
            if (!dataStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(dataStream));
            if (!encryptedStream.CanSeek)
                throw new ArgumentException(Resources.StreamNotSeekable, nameof(encryptedStream));

            base.Decrypt(encryptedStream, dataStream);
        }
        #endregion

        /// <summary>
        /// Initializes the hasher.
        /// </summary>
        /// <param name="hashAlgorithmName">The hash algorithm.</param>
        /// <param name="hashAlgorithmFactory">The hash algorithm factory.</param>
        protected void InitializeHasher(
            string hashAlgorithmName = Algorithms.Symmetric.Default,
            IHashAlgorithmFactory hashAlgorithmFactory = null)
        {
            _hashAlgorithmFactory = Resolver
                                        .GetInstanceOrDefault(hashAlgorithmFactory)
                                        .Initialize(hashAlgorithmName)
                                        ;
        }

        #region Overrides of the primitives called by the GoF template-methods
        /// <summary>
        /// Allows the inheritors to write some unencrypted information to the
        /// <paramref name="encryptedStream" />
        /// before the encrypted text, e.g. here the cipher reserves space in the beginning of the package for the hash.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the encrypted stream <paramref name="encryptedStream"/> cannot be written to.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void BeforeWriteEncrypted(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));
            if (!IsSymmetricKeyInitialized)
                throw new InvalidOperationException(Resources.UninitializedSymmetricKey);

            _hasher = _hashAlgorithmFactory.Create();
            ReserveSpaceForHash(encryptedStream);

            base.BeforeWriteEncrypted(encryptedStream);
        }

        /// <summary>
        /// Reserves the space for hash.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void ReserveSpaceForHash(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));
            if (!encryptedStream.CanSeek)
                throw new ArgumentException(Resources.StreamNotSeekable, nameof(encryptedStream));

            // reserve space in the encrypted stream for the hash
            var length = ShouldEncryptHash
                            ? PublicKey.KeySize / 8
                            : Hasher.HashSize / 8;
            var placeholderHash = new byte[length];

            encryptedStream.Write(BitConverter.GetBytes(placeholderHash.Length), 0, sizeof(int));
            encryptedStream.Write(placeholderHash, 0, placeholderHash.Length);
        }

        /// <summary>
        /// Creates the encrypting stream and gives an opportunity to the inheritors to modify the creation of the crypto-stream,
        /// e.g. here it wraps the crypto-stream created by the base class with another crypto-stream that computes the hash of the unencrypted document.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <returns>The created CryptoStream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream" /> cannot be written to.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override CryptoStream CreateEncryptingStream(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));

            Debug.Assert(_hasher != null);

            return new CryptoStream(
                            base.CreateEncryptingStream(encryptedStream),
                            _hasher,
                            CryptoStreamMode.Write);
        }

        /// <summary>
        /// Gives an opportunity to the inheritors to write more unencrypted information to the
        /// <paramref name="encryptedStream" />, e.g. here it adds the hash to the <paramref name="encryptedStream" />.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream" /> or <paramref name="cryptoStream" /> are <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream" /> cannot be written to.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void AfterWriteEncrypted(
            Stream encryptedStream,
            CryptoStream cryptoStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (cryptoStream == null)
                throw new ArgumentNullException(nameof(cryptoStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));
            if (!cryptoStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(cryptoStream));

            WriteHashInReservedSpace(
                encryptedStream,
                FinalizeHashAfterWrite(encryptedStream, cryptoStream));
        }

        /// <summary>
        /// Writes the hash in the reserved space.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="hash">The hash.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void WriteHashInReservedSpace(
            Stream encryptedStream,
            byte[] hash)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));
            if (!encryptedStream.CanSeek)
                throw new ArgumentException(Resources.StreamNotSeekable, nameof(encryptedStream));

            try
            {
                _hash = ShouldEncryptHash
                            ? PublicKey.Encrypt(hash, true)
                            : hash;

                Debug.Assert(_hash.Length == (ShouldEncryptHash
                                                    ? PublicKey.KeySize / 8
                                                    : Hasher.HashSize / 8));

                // write the hash into the reserved space
                encryptedStream.Seek(sizeof(int), SeekOrigin.Begin);
                encryptedStream.Write(_hash, 0, _hash.Length);
            }
            finally
            {
                _hash = null;
            }
        }

        /// <summary>
        /// Creates the decrypting stream and gives an opportunity to the inheritors to modify the creation of the crypto-stream,
        /// e.g. here it wraps the crypto-stream created in the base class with another crypto-stream that computes the hash of the unencrypted text.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <returns>The created CryptoStream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="encryptedStream" /> cannot be written to.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override CryptoStream CreateDecryptingStream(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));
            if (!IsSymmetricKeyInitialized)
                throw new InvalidOperationException(Resources.UninitializedSymmetricKey);

            Debug.Assert(_hasher != null);

            return new CryptoStream(
                            base.CreateDecryptingStream(encryptedStream),
                            _hasher,
                            CryptoStreamMode.Read);
        }

        /// <summary>
        /// Allows the inheritors to read some unencrypted information from the
        /// <paramref name="encryptedStream" />,
        /// e.g. here the cipher reads, and stores temporarily the hash.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        /// The input data does not represent a valid crypto package.
        /// </exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void BeforeReadDecrypted(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));

            _hasher = _hashAlgorithmFactory.Create();
            LoadHashToValidate(encryptedStream);
            base.BeforeReadDecrypted(encryptedStream);
        }

        /// <summary>
        /// Loads the hash to validate.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        /// <exception cref="System.ArgumentException">
        /// The input data does not represent a valid crypto package: could not read the length of the hash.
        /// or
        /// The input data does not represent a valid crypto package: could not read the hash.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void LoadHashToValidate(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));
            if (ShouldEncryptHash &&  PrivateKey == null)
                throw new InvalidOperationException("The certificate does not contain a private key for decryption.");

            //read the encrypted length and hash
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            if (encryptedStream.Read(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException(Resources.InvalidInputData+"length of the hash.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            _hash = new byte[length];

            if (encryptedStream.Read(_hash, 0, _hash.Length) != _hash.Length)
                throw new ArgumentException(Resources.InvalidInputData+"hash.", nameof(encryptedStream));

            // decrypt
            if (ShouldEncryptHash)
                _hash = PrivateKey.Decrypt(_hash, true);
        }

        /// <summary>
        /// Gives an opportunity to the inheritors to read more unencrypted information from the
        /// <paramref name="encryptedStream" /> or perform other activities, e.g. here it verifies the signature or the hash of the
        /// <paramref name="encryptedStream" />.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream" /> or <paramref name="cryptoStream" /> are <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="encryptedStream" /> cannot be read.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the original and the computed hashes do not match.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void AfterReadDecrypted(
            Stream encryptedStream,
            CryptoStream cryptoStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (cryptoStream == null)
                throw new ArgumentNullException(nameof(cryptoStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));
            if (!cryptoStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(cryptoStream));

            try
            {
                // compare the hashes
                if (!_hash.SequenceEqual(
                                FinalizeHashAfterRead(encryptedStream, cryptoStream)))
                    throw new CryptographicException("The original and computed hashes do not match.");
            }
            finally
            {
                _hash = null;
            }
        }

        /// <summary>
        /// Finalizes the hash after write.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <returns>System.Byte[].</returns>
        /// <exception cref="System.ArgumentNullException">
        /// encryptedStream
        /// or
        /// cryptoStream
        /// </exception>
        /// <exception cref="System.ArgumentException">The encrypted stream cannot be written.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual byte[] FinalizeHashAfterWrite(
            Stream encryptedStream,
            CryptoStream cryptoStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (cryptoStream == null)
                throw new ArgumentNullException(nameof(cryptoStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));
            if (!cryptoStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(cryptoStream));

            Debug.Assert(_hasher != null);

            base.AfterWriteEncrypted(encryptedStream, cryptoStream);
            if (!cryptoStream.HasFlushedFinalBlock)
                cryptoStream.FlushFinalBlock();

            var hash = _hasher.Hash;

            _hasher.Dispose();
            _hasher = null;

            return hash;
        }

        /// <summary>
        /// Finalizes the hash after read.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <returns>System.Byte[].</returns>
        /// <exception cref="System.ArgumentNullException">
        /// encryptedStream
        /// or
        /// cryptoStream
        /// </exception>
        /// <exception cref="System.ArgumentException">The encrypted stream cannot be read.;encryptedStream</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected byte[] FinalizeHashAfterRead(
            Stream encryptedStream,
            CryptoStream cryptoStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (cryptoStream == null)
                throw new ArgumentNullException(nameof(cryptoStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));
            if (!cryptoStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(cryptoStream));

            Debug.Assert(_hasher != null);

            base.AfterReadDecrypted(encryptedStream, cryptoStream);
            if (!cryptoStream.HasFlushedFinalBlock)
                cryptoStream.FlushFinalBlock();

            var hash = _hasher.Hash;

            _hasher.Dispose();
            _hasher = null;

            return hash;
        }
        #endregion

        /// <summary>
        /// Asynchronously reads the clear text from the <paramref name="dataStream" />, encrypts it and writes the result into the
        /// <paramref name="encryptedStream" /> stream. This is the reverse method of <see cref="M:DecryptAsync(System.Stream, System.Stream)" />.
        /// </summary>
        /// <param name="dataStream">The unencrypted input stream.</param>
        /// <param name="encryptedStream">The output stream where to write the crypto package which will contain the encrypted data
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc. This cipher requires that the encrypted stream is seek-able.</param>
        /// <returns>A <see cref="Task" /> object which represents the process of asynchronous encryption.</returns>
        /// <exception cref="System.ArgumentException">The encrypted stream must be seek-able.</exception>
        public override Task EncryptAsync(
            Stream dataStream,
            Stream encryptedStream)
        {
            if (dataStream == null)
                throw new ArgumentNullException(nameof(dataStream));
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));
            if (!encryptedStream.CanSeek)
                throw new ArgumentException(Resources.StreamNotSeekable, nameof(encryptedStream));

            return base.EncryptAsync(dataStream, encryptedStream);
        }

        /// <summary>
        /// Asynchronously reads and decrypts the <paramref name="encryptedStream" /> stream and writes the clear text into the
        /// <paramref name="dataStream" /> stream. This is the reverse method of <see cref="M:EncryptAsync(System.Stream, System.Stream)" />.
        /// </summary>
        /// <param name="encryptedStream">The input crypto package stream which contains the encrypted data
        /// as well as some other crypto artifacts, e.g. initialization vector, hash, etc. This cipher requires that the encrypted stream is seek-able.</param>
        /// <param name="dataStream">The output stream where to put the unencrypted data.</param>
        /// <returns>A <see cref="Task" /> object which represents the process of asynchronous decryption.</returns>
        /// <exception cref="System.ArgumentException">The encrypted stream must be seek-able.</exception>
        public override Task DecryptAsync(
            Stream encryptedStream,
            Stream dataStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (dataStream == null)
                throw new ArgumentNullException(nameof(dataStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));
            if (!dataStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(dataStream));
            if (!encryptedStream.CanSeek)
                throw new ArgumentException(Resources.StreamNotSeekable, nameof(encryptedStream));

            return base.DecryptAsync(encryptedStream, dataStream);
        }

        #region Overrides of the async primitives
        /// <summary>
        /// Allows the inheritors to write asynchronously some unencrypted information to the
        /// <paramref name="encryptedStream" />
        /// before the encrypted text, e.g. here the cipher reserves space in the beginning of the package for the hash.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the process.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the encrypted stream <paramref name="encryptedStream"/> cannot be written to.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override async Task BeforeWriteEncryptedAsync(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(encryptedStream));
            if (!IsSymmetricKeyInitialized)
                throw new InvalidOperationException(Resources.UninitializedSymmetricKey);

            _hasher = _hashAlgorithmFactory.Create();
            ReserveSpaceForHash(encryptedStream);

            await base.BeforeWriteEncryptedAsync(encryptedStream);
        }

        /// <summary>
        /// Allows the inheritors to read asynchronously some unencrypted information from the
        /// <paramref name="encryptedStream" />,
        /// e.g. here the cipher reads, and stores temporarily the hash.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="encryptedStream" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        /// The input data does not represent a valid crypto package.
        /// </exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override async Task BeforeReadDecryptedAsync(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));
            if (!IsSymmetricKeyInitialized)
                throw new InvalidOperationException(Resources.UninitializedSymmetricKey);

            _hasher = _hashAlgorithmFactory.Create();
            await LoadHashToValidateAsync(encryptedStream);
            await base.BeforeReadDecryptedAsync(encryptedStream);
        }

        /// <summary>
        /// Loads asynchronously the hash to validate.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        /// <exception cref="System.ArgumentException">
        /// The input data does not represent a valid crypto package: could not read the length of the hash.
        /// or
        /// The input data does not represent a valid crypto package: could not read the hash.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual async Task LoadHashToValidateAsync(
            Stream encryptedStream)
        {
            if (encryptedStream == null)
                throw new ArgumentNullException(nameof(encryptedStream));
            if (!encryptedStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(encryptedStream));
            if (!ShouldEncryptHash  &&  PrivateKey == null)
                throw new InvalidOperationException("The certificate does not contain a private key for decryption.");

            //read the encrypted length and hash
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            if (await encryptedStream.ReadAsync(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException(Resources.InvalidInputData+"length of the hash.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            var storedHash = new byte[length];

            if (await encryptedStream.ReadAsync(storedHash, 0, storedHash.Length) != storedHash.Length)
                throw new ArgumentException(Resources.InvalidInputData+"hash.", nameof(encryptedStream));

            // decrypt
            _hash = ShouldEncryptHash
                            ? PrivateKey.Decrypt(storedHash, true)
                            : storedHash;
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_hasher != null)
                    _hasher.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
