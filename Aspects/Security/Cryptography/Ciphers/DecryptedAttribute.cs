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
        public const string DefaultEncryptedProperty = "Encypted";

        /// <summary>
        /// Gets or sets the name of the property or field which holds the encrypted value.
        /// </summary>
        public string EncryptedIn { get; }

        /// <summary>
        /// Gets a string that can be used to resolve which cipher should be selected from the DI container.
        /// </summary>
        public string CipherResolver { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptedAttribute"/> class.
        /// </summary>
        /// <param name="encrypted">
        /// Sets the name of a property or field that holds the encrypted value.
        /// </param>
        public DecryptedAttribute(
            string encrypted = DefaultEncryptedProperty)
        {
            EncryptedIn = encrypted;
        }
    }
}
