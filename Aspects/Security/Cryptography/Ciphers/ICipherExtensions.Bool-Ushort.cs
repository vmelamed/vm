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

            return cipher.Encrypt(BitConverter.GetBytes(data));
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

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToBoolean(data, 0);
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

            if (data == null)
                return null;

            int elementSize = sizeof(bool);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return cipher.Encrypt(bytes);
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

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(bool);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of bool values.");

            bool[] data = new bool[bytes.Length / elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToBoolean(bytes, index);
                index += elementSize;
            }

            return data;
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

            return cipher.Encrypt(BitConverter.GetBytes(data));
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

            return BitConverter.ToChar(data, 0);
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

            if (data == null)
                return null;

            var dataString = new string(data);

            return cipher.Encrypt(dataString);
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

            if (encrypted == null)
                return null;

            return cipher.DecryptString(encrypted)
                         .ToCharArray();
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

            return cipher.Encrypt(BitConverter.GetBytes(data));
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

            var data = cipher.Decrypt(encrypted);

            if (data.Length != sizeof(short))
                throw new ArgumentException("The encrypted data does not represent a valid SByte value.", nameof(encrypted));

            return unchecked( (sbyte)BitConverter.ToInt16(data, 0) );
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

            if (data == null)
                return null;

            int elementSize = sizeof(sbyte);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return cipher.Encrypt(bytes);
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

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            var data = new sbyte[bytes.Length];

            for (var i = 0; i < data.Length; i++)
                data[i] = (sbyte)bytes[i];

            return data;
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

            return cipher.Encrypt(BitConverter.GetBytes(data));
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

            var data = cipher.Decrypt(encrypted);

            if (data.Length != 1)
                throw new ArgumentException("The encrypted data does not represent a valid SByte value.", nameof(encrypted));

            return data[0];
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

            return cipher.Encrypt(BitConverter.GetBytes(data));
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

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToInt16(data, 0);
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

            if (data == null)
                return null;

            int elementSize = sizeof(short);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return cipher.Encrypt(bytes);
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

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(short);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of short values.");

            var data = new short[bytes.Length / elementSize];
            var index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToInt16(bytes, index);
                index += elementSize;
            }

            return data;
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

            return cipher.Encrypt(BitConverter.GetBytes(data));
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

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToUInt16(data, 0);
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

            if (data == null)
                return null;

            int elementSize = sizeof(ushort);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return cipher.Encrypt(bytes);
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

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(ushort);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of ushort values.");

            var data = new ushort[bytes.Length / elementSize];
            var index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToUInt16(bytes, index);
                index += elementSize;
            }

            return data;
        }
        #endregion
    }
}
