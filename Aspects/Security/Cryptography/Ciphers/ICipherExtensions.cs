using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ICipherExtensions contains extension methods for <see cref="ICipherAsync"/> objects.
    /// </summary>
    public static class ICipherExtensions
    {
        /// <summary>
        /// Encrypts the <paramref name="text"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="text">The text.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public static byte[] EncryptText(
            this ICipher cipher,
            string text)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, "cipher");
            Contract.Requires<ArgumentNullException>(text != null, "text");

            return cipher.Encrypt(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Encrypts the <paramref name="text"/> and encodes the result with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="text">The text.</param>
        /// <returns>The encrypted text encoded Base64.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public static string EncryptText64(
            this ICipher cipher,
            string text)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, "cipher");
            Contract.Requires<ArgumentNullException>(text != null, "text");

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var encryptedText = Convert.ToBase64String(cipher.EncryptText(text));

            cipher.Base64Encoded = base64;

            return encryptedText;
        }

        /// <summary>
        /// Encrypts the array of bytes <paramref name="data"/> and encodes the result with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text encoded Base64.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public static string EncryptData64(
            this ICipher cipher,
            byte[] data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, "cipher");
            Contract.Requires<ArgumentNullException>(data != null, "buffer");

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var encryptedText = Convert.ToBase64String(cipher.Encrypt(data));

            cipher.Base64Encoded = base64;

            return encryptedText;
        }

        /// <summary>
        /// Decrypts the <paramref name="encryptedText"/> to a string, provided the original string was encoded with <see cref="T:System.Text.Encoding.UTF8"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedText">The crypto text.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public static string DecryptText(
            this ICipher cipher,
            byte[] encryptedText)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, "cipher");
            Contract.Ensures(!(encryptedText==null ^ Contract.Result<string>()==null), "The returned value is invalid.");

            if (encryptedText == null)
                return null;

            var decryptedData = cipher.Decrypt(encryptedText);

            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// Decrypts the <paramref name="encryptedText64"/> to a string, provided the crypto text was encoded with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedText64">The crypto text encoded Base64.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public static string DecryptText64(
            this ICipher cipher,
            string encryptedText64)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, "cipher");
            Contract.Ensures(!(encryptedText64==null ^ Contract.Result<string>()==null), "The returned value is invalid.");

            if (encryptedText64 == null)
                return null;

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var decryptedData = cipher.DecryptText(Convert.FromBase64String(encryptedText64));

            cipher.Base64Encoded = base64;

            return decryptedData;
        }

        /// <summary>
        /// Decrypts the string <paramref name="encryptedData64"/> to an array of bytes, provided the crypto text was encoded with Byte64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedData64">The crypto text encoded Base64.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public static byte[] DecryptData64(
            this ICipher cipher,
            string encryptedData64)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, "cipher");
            Contract.Ensures(!(encryptedData64==null ^ Contract.Result<byte[]>()==null), "The returned value is invalid.");

            if (encryptedData64 == null)
                return null;

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var decryptedData = cipher.Decrypt(Convert.FromBase64String(encryptedData64));

            cipher.Base64Encoded = base64;

            return decryptedData;
        }
    }
}
