using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tools.KeyFile
{
    /// <summary>
    /// Specifies a method for encrypting the symmetric encryption key in the key file.
    /// </summary>
    public enum KeyEncryptionMethod
    {
        /// <summary>
        /// Use DPAPI for encrypting symmetric encryption keys.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DPAPI")]
        DPAPI,

        /// <summary>
        /// Use a certificate for encrypting symmetric encryption keys.
        /// </summary>
        Certificate,

        /// <summary>
        /// Use a certificate for encrypting MAC keys.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MAC")]
        MAC,
    }
}
