using System;
using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ICipherExtensions contains extension methods for <see cref="ICipherTasks"/> objects.
    /// </summary>
    public static partial class ICipherExtensions
    {
        #region En/Decrypt bool-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            bool data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="bool"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static bool DecryptBoolean(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 1)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToBoolean(decrypted);
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
            bool[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="bool"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static bool[] DecryptBooleanArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            var decrypted = cipher.Decrypt(encrypted);

            return FromByteArray.ToBooleanArray(decrypted);
        }
        #endregion

        #region En/Decypt char-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            char data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="char"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static char DecryptChar(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 2)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToChar(decrypted);
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
            char[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="char"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static char[] DecryptCharArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToCharArray(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt sbyte-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static byte[] Encrypt(
            this ICipher cipher,
            sbyte data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="sbyte"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="encrypted"/> does not represent a valid encrypted <see cref="sbyte"/> value.</exception>
        [CLSCompliant(false)]
        public static sbyte DecryptSByte(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 1)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToSByte(decrypted);
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static byte[] Encrypt(
            this ICipher cipher,
            sbyte[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="sbyte"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static sbyte[] DecryptSByteArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToSByteArray(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt byte-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            byte data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="byte"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="encrypted"/> does not represent a valid encrypted <see cref="byte"/> value.</exception>
        public static byte DecryptByte(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 1)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToByte(decrypted);
        }
        #endregion

        #region En/Decrypt short-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            short data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="short"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static short DecryptInt16(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 2)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToInt16(decrypted);
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
            short[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="short"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static short[] DecryptInt16Array(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToInt16Array(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt ushort-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static byte[] Encrypt(
            this ICipher cipher,
            ushort data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="ushort"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static ushort DecryptUInt16(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 2)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToChar(decrypted);
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static byte[] Encrypt(
            this ICipher cipher,
            ushort[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="ushort"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static ushort[] DecryptUInt16Array(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToUInt16Array(cipher.Decrypt(encrypted));
        }
        #endregion
    }
}
