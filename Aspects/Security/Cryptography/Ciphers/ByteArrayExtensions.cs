using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ByteArrayExtensions contains extension methods for array of bytes type of objects (<c>byte[]</c>).
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Fills the array with cryptographically strong random data.
        /// </summary>
        /// <param name="array">The array to fill.</param>
        /// <returns>The filled array.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/></exception>
        /// <exception cref="CryptographicException">The encryption failed.</exception>
        public static byte[] FillRandom(this byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            using (var generator = new RNGCryptoServiceProvider())
                generator.GetBytes(array);

            return array;
        }

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

        /// <summary>
        /// Securely compares two arrays for equality in time which depends only on the length of <paramref name="array"/>.
        /// The security of this compare is based on the fact that regardless of where the first unequal bytes are,
        /// the method compares stubbornly all bytes to the end, thus producing the result in time depending only on the length of the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">The first array to compare.</param>
        /// <param name="other">The second array to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the arrays are equal, otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// array
        /// or
        /// other
        /// </exception>
        public static bool ConstantTimeEquals(
            this byte[] array,
            byte[] other)
        {
            Contract.Requires<ArgumentNullException>(array != null, nameof(array));
            Contract.Requires<ArgumentNullException>(other != null, nameof(other));

            bool equal = array.Length == other.Length;

            for (var i = 0; i < array.Length; i++)
            {
                byte thisByte  = array[i];
                byte otherByte = unchecked(other.Length > i ? other[i] : (byte)~thisByte);

                equal &= (thisByte==otherByte);
            }

            return equal;
        }

        /// <summary>
        /// Converts the <c>byte[]</c> parameter to base64 string.
        /// </summary>
        /// <param name="array">The array to be converted.</param>
        /// <param name="options">The base64 conversion options.</param>
        /// <returns>The base64 representation of the array.</returns>
        /// <remarks>
        /// Note that the returned string contains only the following characters: a-z, A-Z, 0-9, +, / and =.
        /// If stored, for saving space, consider storing the result as an ASCII string instead of Unicode string.
        /// </remarks>
        public static string ToBase64String(
            this byte[] array,
            Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            Contract.Requires<ArgumentNullException>(array != null, nameof(array));

            return Convert.ToBase64String(array, options);
        }

        /// <summary>
        /// Coverts base64 encoded string to the original byte array.
        /// </summary>
        /// <param name="base64">The base64 string to convert.</param>
        /// <param name="offset">The offset in the string to start the conversion from.</param>
        /// <param name="length">The length of the string to convert over, if negative - converts the string to the end.</param>
        /// <returns>The original byte array.</returns>
        public static byte[] FromBase64String(
            this string base64,
            int offset = 0,
            int length = -1)
        {
            Contract.Requires<ArgumentNullException>(base64 != null, nameof(base64));

            return Convert.FromBase64CharArray(base64.ToCharArray(), offset, length < 0 ? base64.Length : length);
        }
    }
}
