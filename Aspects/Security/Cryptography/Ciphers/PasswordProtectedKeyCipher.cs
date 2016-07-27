using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
    /// After the initialization, for security reasons the password is disposed and the cipher object keeps using the key until it is
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
    public class PasswordProtectedKeyCipher : ProtectedKeyCipher
    {
        #region Fields
        readonly int _numberOfIterations;
        readonly int _saltLength;
        SecureString _password;
        byte[] _salt;
        bool _isSymmetricKeyInitializedInternal;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordProtectedKeyCipher"/> class.
        /// </summary>
        /// <param name="password">The password to derive the symmetric key off of.</param>
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
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the <paramref name="password"/> is <see langword="null"/>, empty or consist of whitespace characters only.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if 
        /// <list type="bullet">
        /// <item>the <paramref name="numberOfIterations" /> is less than <see cref="PasswordDerivationConstants.MinNumberOfIterations" /> bytes; or</item>
        /// <item>the <paramref name="saltLength" /> is less than <see cref="PasswordDerivationConstants.MinSaltLength" /> bytes.</item>
        /// </list>
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PasswordProtectedKeyCipher(
            string password,
            int numberOfIterations = PasswordDerivationConstants.DefaultNumberOfIterations,
            int saltLength = PasswordDerivationConstants.DefaultSaltLength,
            string symmetricAlgorithmName = null)
            : base(symmetricAlgorithmName)
        {
            Contract.Requires<ArgumentNullException>(password != null, nameof(password));
            Contract.Requires<ArgumentNullException>(password!=null, nameof(password));
            Contract.Requires<ArgumentException>(password.Length > 0, "The argument "+nameof(password)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(password.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(password)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(numberOfIterations >= PasswordDerivationConstants.MinNumberOfIterations, "The "+nameof(numberOfIterations)+" cannot be at less than \"PasswordDerivationConstants.MinNumberOfIterations\" bytes long.");
            Contract.Requires<ArgumentException>(saltLength >= PasswordDerivationConstants.MinSaltLength, "The "+nameof(saltLength)+" cannot be at less than \"PasswordDerivationConstants.MinSaltLength\" bytes long.");
            Contract.Ensures(_password != null && _password.Length==password.Length);

            _numberOfIterations = numberOfIterations;
            _saltLength         = saltLength;
            _password           = new SecureString();

            foreach (var c in password)
                _password.AppendChar(c);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordProtectedKeyCipher"/> class.
        /// </summary>
        /// <param name="password">The password to derive the symmetric key off of.</param>
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
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the <paramref name="password"/> is <see langword="null"/>, empty or consist of whitespace characters only.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if 
        /// <list type="bullet">
        /// <item>the <paramref name="numberOfIterations" /> is less than <see cref="PasswordDerivationConstants.MinNumberOfIterations" /> bytes; or</item>
        /// <item>the <paramref name="saltLength" /> is less than <see cref="PasswordDerivationConstants.MinSaltLength" /> bytes.</item>
        /// </list>
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PasswordProtectedKeyCipher(
            SecureString password,
            int numberOfIterations = PasswordDerivationConstants.DefaultNumberOfIterations,
            int saltLength = PasswordDerivationConstants.DefaultSaltLength,
            string symmetricAlgorithmName = null)
            : base(symmetricAlgorithmName)
        {
            Contract.Requires<ArgumentNullException>(password != null, nameof(password));
            Contract.Requires<ArgumentException>(password.Length > 0, "The argument "+nameof(password)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(numberOfIterations >= PasswordDerivationConstants.MinNumberOfIterations, "The "+nameof(numberOfIterations)+" cannot be at less than \"PasswordDerivationConstants.MinNumberOfIterations\" bytes long.");
            Contract.Requires<ArgumentException>(saltLength >= PasswordDerivationConstants.MinSaltLength, "The "+nameof(saltLength)+" cannot be at less than \"PasswordDerivationConstants.MinSaltLength\" bytes long.");
            Contract.Ensures(_password != null && _password.Length==password.Length);

            _password           = password.Copy();
            _numberOfIterations = numberOfIterations;
            _saltLength         = saltLength;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Flag indicating whether to encrypt the initialization vector. Always returns <see langword="false"/>
        /// </summary>
        public override bool ShouldEncryptIV
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == false);

                return false;
            }
            set { }
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
        /// Here it just sets the flag <see cref="P:IsSymmetricKeyInitialized"/> to satisfy the contract.
        /// The real initialization can occur only when the first crypto operation really starts. If encrypting
        /// the key and the salt need to be generated. If decrypting the salt needs to read from the crypto package
        /// and passed-on to the key generation method <see cref="M:InitializeSymmetricKeyInternal"/>.
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
        /// <param name="generateSalt">if set to <see langword="true" /> the method will generate salt otherwise will reuse <see cref="F:_salt"/>.</param>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        void InitializeSymmetricKeyInternal(
            bool generateSalt)
        {
            if (_isSymmetricKeyInitializedInternal)
                return;

            if (generateSalt)
                _salt = new byte[_saltLength].FillRandom();

            using (_password)
            using (var derivedBytes = new Rfc2898DeriveBytes(
                                            Encoding.UTF8.GetBytes(_password.ToString()),
                                            _salt,
                                            _numberOfIterations))
                Symmetric.Key = derivedBytes.GetBytes(Symmetric.KeySize / 8);

            _password = null;
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
            // read the length of the salt and allocate an array for it
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            // read the length of the salt and allocate an array for it
            if (encryptedStream.Read(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the length of the salt.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            _salt = new byte[length];

            // read the salt from the memory stream
            if (encryptedStream.Read(_salt, 0, _salt.Length) != _salt.Length)
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the salt.", nameof(encryptedStream));

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
            // read the length of the salt and allocate an array for it
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            // read the length of the salt and allocate an array for it
            if (await encryptedStream.ReadAsync(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the length of the salt.", nameof(encryptedStream));
            length = BitConverter.ToInt32(lengthBuffer, 0);

            _salt = new byte[length];

            // read the salt from the memory stream
            if (await encryptedStream.ReadAsync(_salt, 0, _salt.Length) != _salt.Length)
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the salt.", nameof(encryptedStream));

            InitializeSymmetricKeyInternal(false);

            await base.BeforeReadDecryptedAsync(encryptedStream);
        }
        #endregion

        /// <summary>
        /// Performs the actual job of disposing the object. Here it disposes the password if it is not disposed yet.
        /// </summary>
        /// <param name="disposing">Passes the information whether this method is called by <see cref="M:Dispose()" /> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~SymmetricCipher()" />.</param>
        /// <remarks>If the method is called with <paramref name="disposing" /><c>==true</c>, i.e. from <see cref="M:Dispose()" />, it will try to release all managed resources
        /// (usually aggregated objects which implement <see cref="IDisposable" /> as well) and then it will release all unmanaged resources if any.
        /// If the parameter is <c>false</c> then the method will only try to release the unmanaged resources.</remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _password != null)
                _password.Dispose();
            base.Dispose(disposing);
        }

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(_password != null || _isSymmetricKeyInitializedInternal);
            Contract.Invariant(_salt != null || !_isSymmetricKeyInitializedInternal);
        }
    }
}
