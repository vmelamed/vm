using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Xml
{
    /// <summary>
    /// Class ProtectedKeyXmlCipher encrypts an XML document or selected elements of it with a symmetric key encryption.
    /// The symmetric key is protected with DPAPI into a file.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Nothing to dispose here.")]
    public class ProtectedKeyXmlCipher : EncryptedKeyXmlCipher
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:vm.Aspects.Security.Cryptography.Ciphers.ProtectedData.ProtectedKeyCipher"/> class.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// </param>
        /// <param name="symmetricKeyLocation">
        /// Seeding name of store location name of the encrypted symmetric key (e.g. relative or absolute path).
        /// Can be <see langword="null"/>, empty or whitespace characters only.
        /// The parameter will be passed to the <paramref name="symmetricKeyLocationStrategy"/> to determine the final store location name path (e.g. relative or absolute path).
        /// </param>
        /// <param name="symmetricKeyLocationStrategy">
        /// Object which implements the strategy for determining the store location name (e.g. path and filename) of the encrypted symmetric key.
        /// If <see langword="null"/> it defaults to a new instance of the class <see cref="DefaultServices.KeyFileLocationStrategy"/>.
        /// </param>
        /// <param name="keyStorage">
        /// Object which implements the storing and retrieving of the the encrypted symmetric key to and from the store with the determined location name.
        /// If <see langword="null"/> it defaults to a new instance of the class <see cref="DefaultServices.KeyFileStorage"/>.
        /// </param>
        public ProtectedKeyXmlCipher(
            string symmetricAlgorithmName = null,
            string symmetricKeyLocation = null,
            IKeyLocationStrategy symmetricKeyLocationStrategy = null,
            IKeyStorageTasks keyStorage = null)
            : this(symmetricAlgorithmName)
        {
            ResolveKeyStorage(symmetricKeyLocation, symmetricKeyLocationStrategy, keyStorage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:vm.Aspects.Security.Cryptography.Ciphers.ProtectedData.ProtectedKeyCipher"/> class for initialization by the constructors of the inheriting classes.
        /// </summary>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// </param>
        protected ProtectedKeyXmlCipher(
            string symmetricAlgorithmName)
            : base(symmetricAlgorithmName)
        {
        }
        #endregion

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
    }
}
