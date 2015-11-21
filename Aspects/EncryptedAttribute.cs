using System;

namespace vm.Aspects
{
    /// <summary>
    /// EncryptedAttribute marks a property, field, return value or a parameter as being encrypted.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [AttributeUsage(
        AttributeTargets.Field |
        AttributeTargets.Property |
        AttributeTargets.ReturnValue |
        AttributeTargets.Parameter,
        AllowMultiple = false,
        Inherited = true)]
    public sealed class EncryptedAttribute : Attribute
    {
        /// <summary>
        /// Gets a string that can be used to resolve which cipher should be selected from the DI container.
        /// </summary>
        public string CipherResolver { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedAttribute"/> class.
        /// </summary>
        /// <param name="cipherResolver">An optional string that can be used to resolve which cipher should be selected from the DI container.</param>
        public EncryptedAttribute(string cipherResolver = null)
        {
            CipherResolver = cipherResolver;
        }
    }
}
