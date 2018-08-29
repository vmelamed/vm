using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// ProtectedKeyCipher is a symmetric cipher. The symmetric key is encrypted using DPAPI and so stored in a file.
    /// This class also defines a set of crypto-operations (virtual protected methods) 
    /// for the descending classes which implement the various steps of the methods (GoF template pattern) 
    /// that compose and decompose the crypto-packages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The cipher can be used to protect data at rest, e.g. data stored in a database, file, etc.
    /// </para><para>
    /// By default the cipher uses the <see cref="AesCryptoServiceProvider"/> with default parameters. 
    /// </para><para>
    /// The encrypted symmetric key is stored in a file. The class determines the path and name of the file from an <see cref="IKeyLocationStrategy"/> object.
    /// If the key file does not exist a new key is generated, encrypted and saved in a storage (<see cref="IKeyStorage"/>) in the location determined by the 
    /// <see cref="IKeyLocationStrategy"/> object.
    /// </para><para>
    /// DPAPI uses different keys for different machines and users. Therefore to enable your application to exchange and persist encrypted data with 
    /// other machines and applications (data in motion), you would need to export the clear text of the key and then import it to the target machines.
    /// The command line utility <c>ProtectedKey</c> can be used for this purpose.  More elaborate management can be implemented by using the 
    /// <see cref="IKeyManagement"/>'s methods <see cref="IKeyManagement.ExportSymmetricKey"/> and 
    /// <see cref="IKeyManagement.ImportSymmetricKey"/>. Please, follow the best security practices while handling the clear text of the key.
    /// </para><para>
    /// The symmetric key is generated and encrypted into a file once and read and decrypted from a file once per the life time of the cipher object.
    /// If the symmetric key is compromised, all documents encrypted with it will be compromised too.
    /// If higher level of security is required, consider using the <see cref="EncryptedKeyCipher"/> which generates the symmetric key and 
    /// encrypts it with asymmetric key, e.g. from a certificate, instead of using DPAPI. In this case, if all participating machines share the same certificate,
    /// the file can be shared as is - without exporting and importing of the key in clear text. A third option is to use <see cref="EncryptedNewKeyCipher"/> 
    /// which generates a new key for each document. The latter encrypts the symmetric key with an asymmetric key, e.g. from a certificate, 
    /// and stores it in the crypto package together with the initialization vector and the crypto text. No need for key management whatsoever, however the performance is lowered.
    /// </para><para>
    /// <b>Note that the DPAPI encryption of the key is done in the scope of the local machine. This means that anyone logged-on locally to the machine with read-access to the file could 
    /// obtain the clear text of the symmetric key. Best practice is to protect the key file from access by other users. Therefore the key file is created with allowed access 
    /// (full control) only to the current user, the SYSTEM account and the Administrators group.</b>
    /// While it is possible to change the protection scope to the current user, usually this is highly impractical: the key management must be done under the same security
    /// context as the application's. Especially for web applications, services, etc. which usually run under accounts with very limited rights - 
    /// this is almost impossible scenario which might pose other security risks.
    /// </para><para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item>Length of the encrypted symmetric cipher initialization vector (serialized Int32) - 4 bytes.</item>
    ///         <item>The bytes of the encrypted initialization vector.</item>
    ///         <item>The bytes of the encrypted text.</item>
    ///     </list>
    /// </para><para>
    /// The cipher can also be used to encrypt elements of or entire XML documents.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is implemented correctly.")]
    public class ProtectedKeyCipher : EncryptedKeyCipher
    {
        #region Properties
        /// <summary>
        /// Flag indicating whether to encrypt the initialization vector.
        /// </summary>
        public override bool ShouldEncryptIV { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedKeyCipher" /> class.
        /// </summary>
        /// <param name="symmetricKeyLocation">
        /// Seeding name of store location name of the encrypted symmetric key (e.g. relative or absolute path). Can be <see langword="null" />, 
        /// empty or whitespace characters only. The parameter will be passed to the <paramref name="symmetricKeyLocationStrategy" /> to determine the final 
        /// store location name path (e.g. relative or absolute path).
        /// </param>
        /// <param name="symmetricKeyLocationStrategy">
        /// Object which implements the strategy for determining the store location name (e.g. path and filename) of the encrypted symmetric key.
        /// If <see langword="null" /> it defaults to a new instance of the class <see cref="DefaultServices.KeyFileLocationStrategy" />.
        /// Alternatively an implementation type can be registered in a common service locator compatible DI container.</param>
        /// <param name="keyStorage">
        /// Object which implements the storing and retrieving of the the encrypted symmetric key to and from the store with the determined location name.
        /// If <see langword="null" /> it defaults to a new instance of the class <see cref="DefaultServices.KeyFileStorage" />.
        /// Alternatively an implementation type can be registered in a common service locator compatible DI container.
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or even
        /// <see langword="null" />, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default" />.
        /// </param>
        /// <param name="symmetricAlgorithmFactory">
        /// The symmetric algorithm factory. If <see langword="null" /> the constructor will create an instance of the <see cref="DefaultServices.SymmetricAlgorithmFactory" />,
        /// which uses the <see cref="SymmetricAlgorithm.Create(string)" /> method from the .NET library.
        /// </param>
        public ProtectedKeyCipher(
            string symmetricKeyLocation = null,
            IKeyLocationStrategy symmetricKeyLocationStrategy = null,
            IKeyStorageTasks keyStorage = null,
            string symmetricAlgorithmName = Algorithms.Symmetric.Default,
            ISymmetricAlgorithmFactory symmetricAlgorithmFactory = null)
            : base(symmetricAlgorithmName, symmetricAlgorithmFactory)
        {
            ResolveKeyStorage(symmetricKeyLocation, symmetricKeyLocationStrategy, keyStorage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedKeyCipher" /> class for initialization by the constructors of the inheriting classes.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or
        /// <see langword="null" />, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default" />.
        /// </param>
        /// <param name="symmetricAlgorithmFactory">
        /// The symmetric algorithm factory. If <see langword="null" /> the constructor will create an instance of the default <see cref="DefaultServices.SymmetricAlgorithmFactory" />,
        /// which uses the <see cref="SymmetricAlgorithm.Create(string)" /> method from the .NET library.
        /// </param>
        protected ProtectedKeyCipher(
            string symmetricAlgorithmName,
            ISymmetricAlgorithmFactory symmetricAlgorithmFactory = null)
            : base(symmetricAlgorithmName, symmetricAlgorithmFactory)
        {
        }
        #endregion

        #region Initialization of the symmetric key overrides
        /// <summary>
        /// Encrypts the symmetric key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <returns>System.Byte[].</returns>
        protected override byte[] EncryptSymmetricKey()
            => ProtectedData.Protect(Symmetric.Key, null, DataProtectionScope.LocalMachine);

        /// <summary>
        /// Decrypts the symmetric key.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <param name="encryptedKey">The encrypted key.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void DecryptSymmetricKey(
            byte[] encryptedKey)
        {
            if (encryptedKey == null)
                throw new ArgumentNullException(nameof(encryptedKey));
            if (encryptedKey.Length == 0)
                throw new ArgumentException(Resources.InvalidArgument, nameof(encryptedKey));

            Symmetric.Key = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.LocalMachine);
        }
        #endregion

        #region Encrypting primitives
        /// <summary>
        /// Encrypts the symmetric cipher's initialization vector.
        /// </summary>
        /// <remarks>
        /// The method is called by the GoF template-methods.
        /// </remarks>
        /// <returns>System.Byte[].</returns>
        protected override byte[] EncryptIV()
        {
            if (!IsSymmetricKeyInitialized)
                throw new InvalidOperationException(Resources.UninitializedSymmetricKey);

            return ShouldEncryptIV
                        ? ProtectedData.Protect(Symmetric.IV, null, DataProtectionScope.LocalMachine)
                        : Symmetric.IV;
        }
        #endregion

        #region Decrypting primitives
        /// <summary>
        /// Decrypts the symmetric cipher's initialization vector.
        /// </summary>
        /// <param name="encryptedIV">The encrypted initialization vector.</param>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        protected override void DecryptIV(
            byte[] encryptedIV)
        {
            if (!IsSymmetricKeyInitialized)
                throw new InvalidOperationException(Resources.UninitializedSymmetricKey);

            Symmetric.IV = ShouldEncryptIV
                                ? ProtectedData.Unprotect(encryptedIV, null, DataProtectionScope.LocalMachine)
                                : encryptedIV;
        }
        #endregion

        /// <summary>
        /// Copies certain characteristics of this instance to the <paramref name="cipher"/> parameter.
        /// The goal is to produce a cipher with the same encryption/decryption behavior but saving the key encryption and decryption ceremony and overhead if possible.
        /// </summary>
        /// <param name="cipher">The cipher that gets the identical symmetric algorithm object.</param>
        protected override void CopyTo(
            SymmetricKeyCipherBase cipher)
        {
            base.CopyTo(cipher);

            if (cipher is ProtectedKeyCipher c)
                c.Base64Encoded = Base64Encoded;
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
            if (ShouldEncryptIV)
                throw new InvalidOperationException("This object cannot create light clones because the property "+nameof(ShouldEncryptIV)+" is true.");

            InitializeSymmetricKey();

            var cipher = new ProtectedKeyCipher(Symmetric.GetType().FullName, null);

            CopyTo(cipher);
            cipher.KeyStorage = null;

            return cipher;
        }
        #endregion
    }
}
