using System;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// DecryptedAttribute marks a property or field as one that must be first decrypted before accessing its actual value.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [AttributeUsage(
        AttributeTargets.Field |
        AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = true)]
    public sealed class DecryptedAttribute : Attribute
    {
        /// <summary>
        /// The default name of a property or field that holds the encrypted value.
        /// </summary>
        public const string DefaultEncryptedProperty = "Encrypted";

        /// <summary>
        /// Gets or sets the name of the property or field which holds the encrypted value.
        /// The type of the specified property or field must be array of bytes.
        /// </summary>
        public string EncryptedIn { get; }

        /// <summary>
        /// Gets a string that can be used to resolve which cipher should be selected from the DI container.
        /// </summary>
        public string CipherResolver { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptedAttribute"/> class.
        /// </summary>
        /// <param name="encryptedIn">
        /// Sets the name of a property or field that holds the encrypted value.
        /// The type of the specified property or field must be array of bytes.
        /// </param>
        public DecryptedAttribute(
            string encryptedIn = DefaultEncryptedProperty)
        {
            EncryptedIn = encryptedIn;
        }
    }
}
