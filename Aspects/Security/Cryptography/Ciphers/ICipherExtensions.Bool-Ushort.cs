using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ICipherExtensions contains extension methods for <see cref="ICipherAsync"/> objects.
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            return FromByteArray.ToBoolean(cipher.Decrypt(encrypted));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encrypted == null ^ Contract.Result<bool[]>() == null));

            if (encrypted == null)
                return null;

            return FromByteArray.ToBooleanArray(cipher.Decrypt(encrypted));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            var data = cipher.Decrypt(encrypted);

            return FromByteArray.ToChar(data);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encrypted == null ^ Contract.Result<char[]>() == null));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            return FromByteArray.ToSByte(cipher.Decrypt(encrypted));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encrypted == null ^ Contract.Result<sbyte[]>() == null));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            return FromByteArray.ToByte(cipher.Decrypt(encrypted));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            return FromByteArray.ToInt16(cipher.Decrypt(encrypted));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encrypted == null ^ Contract.Result<short[]>() == null));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            return FromByteArray.ToUInt16(cipher.Decrypt(encrypted));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Ensures(!(encrypted == null ^ Contract.Result<ushort[]>() == null));

            if (encrypted == null)
                return null;

            return FromByteArray.ToUInt16Array(cipher.Decrypt(encrypted));
        }
        #endregion
    }
}
