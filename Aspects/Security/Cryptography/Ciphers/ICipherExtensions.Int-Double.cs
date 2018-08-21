using System;
using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ICipherExtensions contains extension methods for <see cref="ICipherTasks"/> objects.
    /// </summary>
    public static partial class ICipherExtensions
    {
        #region En/Decrypt int-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            int data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(BitConverter.GetBytes(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="int"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static int DecryptInt32(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 4)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToInt32(decrypted);
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
            int[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="int"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static int[] DecryptInt32Array(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToInt32Array(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt uint-s
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
            uint data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="uint"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static uint DecryptUInt32(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 4)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToUInt32(decrypted);
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
            uint[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="uint"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static uint[] DecryptUInt32Array(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToUInt32Array(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt long-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            long data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="long"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static long DecryptInt64(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 8)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToInt64(decrypted);
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
            long[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="long"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static long[] DecryptInt64Array(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToInt64Array(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt ulong-s
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
            ulong data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="ulong"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static ulong DecryptUInt64(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 8)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToUInt64(decrypted);
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
            ulong[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="ulong"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [CLSCompliant(false)]
        public static ulong[] DecryptUInt64Array(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToUInt64Array(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt float-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            float data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="float"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static float DecryptSingle(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 4)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToSingle(decrypted);
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
            float[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="float"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static float[] DecryptSingleArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToSingleArray(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt double-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            double data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="double"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static double DecryptDouble(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 8)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToDouble(decrypted);
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
            double[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="double"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static double[] DecryptDoubleArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToDoubleArray(cipher.Decrypt(encrypted));
        }
        #endregion
    }
}