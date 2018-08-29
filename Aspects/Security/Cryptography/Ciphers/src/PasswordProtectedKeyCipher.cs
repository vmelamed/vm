using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// <para>
    /// Class <c>PasswordProtectedKeyCipher</c>. This cipher is suitable for encrypting data with symmetric keys derived from passwords.
    /// Internally the cipher uses the PBKDF2 method based on HMACSHA1 and specified in RFC2898 for deriving artifacts (e.g. keys) 
    /// from passwords.
    /// </para><para>
    /// Please, note that the derivation process is intentionally slow in order to thwart brute force, dictionary attacks.
    /// The key is initialized only once from the password and the randomly generated or read from the first document salt bytes.
    /// After the initialization, for security reasons the password is discarded and the cipher object keeps using the key until it is
    /// disposed of or garbage-collected.
    /// </para><para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item>Length of the unencrypted salt bytes (serialized Int32) - 4 bytes.</item>
    ///         <item>The salt bytes.</item>
    ///         <item>Length of the unencrypted symmetric cipher initialization vector (serialized Int32) - 4 bytes.</item>
    ///         <item>The bytes of the unencrypted initialization vector.</item>
    ///         <item>The bytes of the encrypted text.</item>
    ///     </list>
    /// </para>
    /// </summary>
    public class PasswordProtectedKeyCipher : EncryptedKeyCipher
    {
        #region Fields
        int _numberOfIterations;
        int _saltLength;
        string _password;
        byte[] _salt;
        bool _isSymmetricKeyInitializedInternal;
        #endregion

        #region Properties
        /// <summary>
        /// Flag indicating whether to encrypt the initialization vector. Always returns <see langword="false"/>
        /// </summary>
        public override bool ShouldEncryptIV
        {
            get => false;
            set { }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordProtectedKeyCipher" /> class.
        /// </summary>
        /// <param name="password">
        /// The password to derive the symmetric key off of.
        /// </param>
        /// <param name="numberOfIterations">
        /// The number of iterations, the default value is <see cref="PasswordDerivationConstants.DefaultNumberOfIterations" />.
        /// The greater the iterations the more secure is the generated symmetric key but is also slower.
        /// Should not be less than <see cref="PasswordDerivationConstants.DefaultNumberOfIterations" />.
        /// </param>
        /// <param name="saltLength">
        /// The length of the salt, the default value is <see cref="PasswordDerivationConstants.DefaultSaltLength" /> bytes.
        /// Must be at least <see cref="PasswordDerivationConstants.MinSaltLength" /> bytes.
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or
        /// <see langword="null" />, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default" />.
        /// </param>
        /// <param name="symmetricAlgorithmFactory">
        /// The symmetric algorithm factory. If <see langword="null" /> the constructor will create an instance of the <see cref="DefaultServices.SymmetricAlgorithmFactory" />,
        /// which uses the <see cref="SymmetricAlgorithm.Create(string)" /> method from the .NET library.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the <paramref name="password" /> is <see langword="null" />, empty or consist of whitespace characters only.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <list type="bullet">
        /// <item>the <paramref name="numberOfIterations" /> is less than <see cref="PasswordDerivationConstants.MinNumberOfIterations" /> bytes; or </item>
        /// <item>the <paramref name="saltLength" /> is less than <see cref="PasswordDerivationConstants.MinSaltLength" /> bytes.</item>
        /// </list>
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PasswordProtectedKeyCipher(
            string password,
            int numberOfIterations = PasswordDerivationConstants.DefaultNumberOfIterations,
            int saltLength = PasswordDerivationConstants.DefaultSaltLength,
            string symmetricAlgorithmName = Algorithms.Symmetric.Default,
            ISymmetricAlgorithmFactory symmetricAlgorithmFactory = null)
            : base(symmetricAlgorithmName, symmetricAlgorithmFactory)
        {
            if (password.IsNullOrWhiteSpace())
                throw new ArgumentException(Resources.NullOrEmptyArgument, nameof(password));
            if (numberOfIterations < PasswordDerivationConstants.MinNumberOfIterations)
                throw new ArgumentException($"The argument cannot be at less than {PasswordDerivationConstants.MinNumberOfIterations} bytes long.", nameof(numberOfIterations));
            if (saltLength < PasswordDerivationConstants.MinSaltLength)
                throw new ArgumentException($"The argument cannot be at less than {PasswordDerivationConstants.MinSaltLength} bytes long.", nameof(saltLength));

            _numberOfIterations = numberOfIterations;
            _saltLength         = saltLength;
            _password           = (string)password.Clone();
        }

        private PasswordProtectedKeyCipher(
            string symmetricAlgorithmName = Algorithms.Symmetric.Default,
            ISymmetricAlgorithmFactory symmetricAlgorithmFactory = null)
            : base(symmetricAlgorithmName, symmetricAlgorithmFactory)
        {
        }
        #endregion

        #region Disable the inherited IKeyManagement - the keys are stored in the crypto-package.
        /// <summary>
        /// Gets the determined store location name (e.g. path and filename).
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override string KeyLocation
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Imports the clear text of a symmetric key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void ImportSymmetricKey(
            byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Exports the clear text of the symmetric key.
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public override byte[] ExportSymmetricKey()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously imports the symmetric key as a clear text.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override Task ImportSymmetricKeyAsync(
            byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously exports the symmetric key as a clear text.
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public override Task<byte[]> ExportSymmetricKeyAsync()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Initialization of the symmetric key overrides
        /// <summary>
        /// Here it just sets the flag <see cref="SymmetricKeyCipherBase.IsSymmetricKeyInitialized"/> to satisfy the contract.
        /// The real initialization can occur only when the first crypto operation really starts. If encrypting
        /// the key and the salt need to be generated. If decrypting the salt needs to read from the crypto package
        /// and passed-on to the key generation method <see cref="InitializeSymmetricKeyInternal"/>.
        /// </summary>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected override void InitializeSymmetricKey()
        {
            IsSymmetricKeyInitialized = true;
        }

        /// <summary>
        /// If not yet initialized, the method initializes the symmetric key by deriving it from the password and generating new salt bytes.
        /// </summary>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected override Task InitializeSymmetricKeyAsync()
        {
            IsSymmetricKeyInitialized = true;
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// If not yet initialized, the method initializes the symmetric key by deriving it from the password and generating new salt bytes.
        /// </summary>
        /// <param name="generateSalt">if set to <see langword="true" /> the method will generate salt otherwise will reuse <see cref="_salt"/>.</param>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        void InitializeSymmetricKeyInternal(
            bool generateSalt)
        {
            if (_isSymmetricKeyInitializedInternal)
                return;

            if (generateSalt)
                _salt = new byte[_saltLength].FillRandom();

            using (var derivedBytes = new Rfc2898DeriveBytes(
                                            Encoding.UTF8.GetBytes(_password),
                                            _salt,
                                            _numberOfIterations))
                Symmetric.Key = derivedBytes.GetBytes(Symmetric.KeySize / 8);

            _isSymmetricKeyInitializedInternal = true;
        }

        /// <summary>
        /// Here it does nothing and always returns <see langword="null"/>.
        /// </summary>
        /// <returns><see langword="null"/></returns>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected override byte[] EncryptSymmetricKey()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Here it does nothing.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void DecryptSymmetricKey(
            byte[] encryptedKey)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Encrypting primitives
        /// <summary>
        /// Allows the inheritors to write some unencrypted information to the <paramref name="encryptedStream" />
        /// before the encrypted text, e.g. here the cipher writes the salt and the initialization vector.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        /// <exception cref="System.ArgumentException">The input stream cannot be written to.;encryptedStream</exception>
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

            InitializeSymmetricKeyInternal(true);

            // write the length and contents of the salt clear text in the memory stream
            encryptedStream.Write(BitConverter.GetBytes(_salt.Length), 0, sizeof(int));
            encryptedStream.Write(_salt, 0, _salt.Length);

            base.BeforeWriteEncrypted(encryptedStream);
        }
        #endregion

        #region Decrypting primitives
        /// <summary>
        /// Allows the inheritors to read some unencrypted information from the <paramref name="encryptedStream" />,
        /// e.g. here the cipher reads and sets the salt and the initialization vector in the symmetric cipher.
        /// Also if the key is not initialized yet - here it will generate the key from the password and the read salt.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        /// <exception cref="System.ArgumentException">
        /// The input stream cannot be read.;encryptedStream
        /// or
        /// The input data does not represent a valid crypto package: could not read the length of the salt.;encryptedStream
        /// or
        /// The input data does not represent a valid crypto package: could not read the salt.;encryptedStream
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

            // read the length of the salt and allocate an array for it
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            // read the length of the salt and allocate an array for it
            if (encryptedStream.Read(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException(Resources.InvalidInputData+"length of the salt.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            _salt = new byte[length];

            // read the salt from the memory stream
            if (encryptedStream.Read(_salt, 0, _salt.Length) != _salt.Length)
                throw new ArgumentException(Resources.InvalidInputData+"salt.", nameof(encryptedStream));

            InitializeSymmetricKeyInternal(false);

            base.BeforeReadDecrypted(encryptedStream);
        }
        #endregion

        #region Async encrypting primitives
        /// <summary>
        /// Allows the inheritors to write asynchronously some unencrypted information to the <paramref name="encryptedStream" />
        /// before the encrypted text, e.g. here the cipher writes the salt and the initialization vector.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        /// <exception cref="System.ArgumentException">The input stream cannot be written to.;encryptedStream</exception>
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

            InitializeSymmetricKeyInternal(true);

            // write the length and contents of the salt clear text in the memory stream
            await encryptedStream.WriteAsync(BitConverter.GetBytes(_salt.Length), 0, sizeof(int));
            await encryptedStream.WriteAsync(_salt, 0, _salt.Length);

            await base.BeforeWriteEncryptedAsync(encryptedStream);
        }
        #endregion

        #region Async decrypting primitives
        /// <summary>
        /// Allows the inheritors to read some unencrypted information from the <paramref name="encryptedStream" />,
        /// e.g. here the cipher reads and sets the salt and the initialization vector in the symmetric cipher.
        /// Also if the key is not initialized yet - here it will generate the key from the password and the read salt.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        /// <exception cref="System.ArgumentException">
        /// The input stream cannot be read.;encryptedStream
        /// or
        /// The input data does not represent a valid crypto package: could not read the length of the salt.;encryptedStream
        /// or
        /// The input data does not represent a valid crypto package: could not read the salt.;encryptedStream
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

            // read the length of the salt and allocate an array for it
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            // read the length of the salt and allocate an array for it
            if (await encryptedStream.ReadAsync(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException(Resources.InvalidInputData+"length of the salt.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            _salt = new byte[length];

            // read the salt from the memory stream
            if (await encryptedStream.ReadAsync(_salt, 0, _salt.Length) != _salt.Length)
                throw new ArgumentException(Resources.InvalidInputData+"salt.", nameof(encryptedStream));

            InitializeSymmetricKeyInternal(false);

            await base.BeforeReadDecryptedAsync(encryptedStream);
        }
        #endregion

        /// <summary>
        /// Copies certain characteristics of this instance to the <paramref name="cipher" /> parameter.
        /// The goal is to produce a cipher with the same encryption/decryption behavior but saving the key encryption and decryption ceremony and overhead if possible.
        /// </summary>
        /// <param name="cipher">The cipher that gets the identical symmetric algorithm object.</param>
        protected override void CopyTo(SymmetricKeyCipherBase cipher)
        {
            base.CopyTo(cipher);
            if (!(cipher is PasswordProtectedKeyCipher pwdCipher))
                return;

            _password           = (string)pwdCipher._password.Clone();
            _numberOfIterations = pwdCipher._numberOfIterations;
            _saltLength         = pwdCipher._saltLength;
        }

        #region ILightCipher
        /// <summary>
        /// Releases the asymmetric keys. By doing so the instance looses its <see cref="IKeyManagement" /> behavior but the memory footprint becomes much lighter.
        /// The asymmetric keys can be dropped only if the underlying symmetric algorithm instance is already initialized and
        /// the property <see cref="SymmetricKeyCipherBase.ShouldEncryptIV" /> is <see langword="false" />.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the underlying symmetric algorithm instance is not initialized yet or the property <see cref="SymmetricKeyCipherBase.ShouldEncryptIV" /> is <see langword="false" />.
        /// </exception>
        /// See also <seealso cref="CloneLightCipher"/>.
        public override ICipherTasks ReleaseCertificate()
        {
            InitializeSymmetricKey();

            return this;
        }

        /// <summary>
        /// Creates a new, lightweight <see cref="EncryptedKeyCipher"/> instance and copies certain characteristics of this instance to it.
        /// A duplicate can be created only if the underlying symmetric algorithm instance is already initialized and the property <see cref="SymmetricKeyCipherBase.ShouldEncryptIV"/> is <see langword="false"/>.
        /// The duplicate can be used only for encryption and decryption of data (the <see cref="ICipher"/> and <see cref="ICipherTasks"/> behavior). The <see cref="IKeyManagement"/> behavior is disabled and
        /// calling any of its members would throw <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <returns>The duplicate.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the underlying symmetric algorithm instance is not initialized yet or the property <see cref="SymmetricKeyCipherBase.ShouldEncryptIV" /> is <see langword="false" />.
        /// </exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The caller must dispose it.")]
        public override ICipherTasks CloneLightCipher()
        {
            InitializeSymmetricKey();

            var cipher = new PasswordProtectedKeyCipher(Symmetric.GetType().FullName, null);

            CopyTo(cipher);

            return cipher;
        }
        #endregion
    }
}
