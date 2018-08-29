using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ByteArrayExtensions contains extension methods for array of bytes type of objects (<c>byte[]</c>).
    /// </summary>
    public static class ByteArrayExtensionsProtected
    {
        /// <summary>
        /// Encrypts the data in a specified byte array and returns a byte array that contains the encrypted data.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="entropy">An optional additional byte array used to increase the complexity of the encryption, or <see langword="null"/> for no additional complexity.</param>
        /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
        /// <returns>A byte array that contains the encrypted data or <see langword="null"/> if <paramref name="array"/> is <see langword="null"/>.</returns>
        /// <exception cref="CryptographicException">The encryption failed.</exception>
        public static byte[] Protect(
            this byte[] array,
            byte[] entropy = null,
            DataProtectionScope scope = DataProtectionScope.LocalMachine)
        {
            if (array == null)
                return null;

            return ProtectedData.Protect(array, entropy, scope);
        }

        /// <summary>
        /// Encrypts the data in a specified byte array and returns a byte array that contains the encrypted data.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="entropy">An optional additional byte array used to increase the complexity of the encryption, or <see langword="null"/> for no additional complexity.</param>
        /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
        /// <returns>A byte array that contains the decrypted data or <see langword="null"/> if <paramref name="array"/> is <see langword="null"/>.</returns>
        /// <exception cref="CryptographicException">The encryption failed.</exception>
        public static byte[] Unprotect(
            this byte[] array,
            byte[] entropy = null,
            DataProtectionScope scope = DataProtectionScope.LocalMachine)
        {
            if (array == null)
                return null;

            return ProtectedData.Unprotect(array, entropy, scope);
        }
    }
}
