using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ICipherExtensions contains extension methods for <see cref="ICipherAsync"/> objects.
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToInt32(data, 0);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            int elementSize = sizeof(int);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(int);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of int values.");

            var data = new int[bytes.Length / elementSize];
            var index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToInt32(bytes, index);
                index += elementSize;
            }

            return data;
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            return cipher.Encrypt(BitConverter.GetBytes(data));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToUInt32(data, 0);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            int elementSize = sizeof(uint);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(uint);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of int values.");

            var data = new uint[bytes.Length / elementSize];
            var index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToUInt32(bytes, index);
                index += elementSize;
            }

            return data;
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            return cipher.Encrypt(BitConverter.GetBytes(data));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToInt64(data, 0);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            int elementSize = sizeof(long);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(long);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of long values.");

            var data = new long[bytes.Length / elementSize];
            var index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToInt64(bytes, index);
                index += elementSize;
            }

            return data;
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            return cipher.Encrypt(BitConverter.GetBytes(data));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToUInt64(data, 0);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            int elementSize = sizeof(ulong);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(ulong);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of ulong values.");

            var data = new ulong[bytes.Length / elementSize];
            var index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToUInt64(bytes, index);
                index += elementSize;
            }

            return data;
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            return cipher.Encrypt(BitConverter.GetBytes(data));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToSingle(data, 0);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            int elementSize = sizeof(float);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(float);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of float values.");

            var data = new float[bytes.Length / elementSize];
            var index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToSingle(bytes, index);
                index += elementSize;
            }

            return data;
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            return cipher.Encrypt(BitConverter.GetBytes(data));
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));
            Contract.Requires<ArgumentNullException>(encrypted != null, nameof(encrypted));

            var data = cipher.Decrypt(encrypted);

            return BitConverter.ToDouble(data, 0);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (data == null)
                return null;

            int elementSize = sizeof(double);
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
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            if (encrypted == null)
                return null;

            var bytes = cipher.Decrypt(encrypted);
            int elementSize = sizeof(double);

            if (bytes.Length % elementSize != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array of double values.");

            var data = new double[bytes.Length / elementSize];
            var index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToDouble(bytes, index);
                index += elementSize;
            }

            return data;
        }
        #endregion
    }
}