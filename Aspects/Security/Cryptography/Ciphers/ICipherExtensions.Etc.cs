using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ICipherExtensions contains extension methods for <see cref="ICipherAsync"/> objects.
    /// </summary>
    public static partial class ICipherExtensions
    {
        #region En/Decrypt DateTime-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            DateTime data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static DateTime DecryptDateTime(
            this ICipher cipher,
            byte[] encrypted)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            return FromByteArray.ToDateTime(cipher.Decrypt(encrypted));
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            DateTime[] data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="DateTime"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static DateTime[] DecryptDateTimeArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToDateTimeArray(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt decimal-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            decimal data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static decimal DecryptDecimal(
            this ICipher cipher,
            byte[] encrypted)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            return FromByteArray.ToDecimal(cipher.Decrypt(encrypted));
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            decimal[] data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="decimal"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static decimal[] DecryptDecimalArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToDecimalArray(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt string-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            string data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encryptedText"/> to a string, provided the original string was encoded with <see cref="T:System.Text.Encoding.UTF8"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedText">The crypto text.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static string DecryptString(
            this ICipher cipher,
            byte[] encryptedText)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encryptedText == null ^ Contract.Result<string>() == null), "The returned value is invalid.");

            if (encryptedText == null)
                return null;

            return FromByteArray.ToString(cipher.Decrypt(encryptedText));
        }

        /// <summary>
        /// Encrypts the <paramref name="text"/> and encodes the result with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="text">The text.</param>
        /// <returns>The encrypted text encoded Base64.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static string EncryptText64(
            this ICipher cipher,
            string text)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(text != null, "text");

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var encryptedText = Convert.ToBase64String(cipher.Encrypt(text));

            cipher.Base64Encoded = base64;

            return encryptedText;
        }

        /// <summary>
        /// Decrypts the <paramref name="encryptedText64"/> to a string, provided the crypto text was encoded with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedText64">The crypto text encoded Base64.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static string DecryptText64(
            this ICipher cipher,
            string encryptedText64)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encryptedText64 == null ^ Contract.Result<string>() == null), "The returned value is invalid.");

            if (encryptedText64 == null)
                return null;

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var decryptedData = cipher.DecryptString(Convert.FromBase64String(encryptedText64));

            cipher.Base64Encoded = base64;

            return decryptedData;
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The text.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [Obsolete("Use the overloaded cipher.Encrypt(string data) instead.")]
        public static byte[] EncryptText(
            this ICipher cipher,
            string data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(data != null, "text");

            return cipher.Encrypt(data);
        }

        /// <summary>
        /// Decrypts the <paramref name="encryptedText"/> to a string, provided the original string was encoded with <see cref="T:System.Text.Encoding.UTF8"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedText">The crypto text.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [Obsolete("Use DecryptString instead.")]
        public static string DecryptText(
            this ICipher cipher,
            byte[] encryptedText)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encryptedText == null ^ Contract.Result<string>() == null), "The returned value is invalid.");

            if (encryptedText == null)
                return null;

            return cipher.DecryptString(encryptedText);
        }
        #endregion

        #region En/Decrypt decimal-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            Guid data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static Guid DecryptGuid(
            this ICipher cipher,
            byte[] encrypted)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            return FromByteArray.ToGuid(cipher.Decrypt(encrypted));
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            Guid[] data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="Guid"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static Guid[] DecryptGuidArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToGuidArray(cipher.Decrypt(encrypted));
        }
        #endregion

        /// <summary>
        /// Encrypts the array of bytes <paramref name="data"/> and encodes the result with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text encoded Base64.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static string EncryptData64(
            this ICipher cipher,
            byte[] data)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(data != null, "buffer");

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var encryptedText = Convert.ToBase64String(cipher.Encrypt(data));

            cipher.Base64Encoded = base64;

            return encryptedText;
        }

        /// <summary>
        /// Decrypts the string <paramref name="encryptedData64"/> to an array of bytes, provided the crypto text was encoded with Byte64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedData64">The crypto text encoded Base64.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] DecryptData64(
            this ICipher cipher,
            string encryptedData64)
        {
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encryptedData64 == null ^ Contract.Result<byte[]>() == null), "The returned value is invalid.");

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