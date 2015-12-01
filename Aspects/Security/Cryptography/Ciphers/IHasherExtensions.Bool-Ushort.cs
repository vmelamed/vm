using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class IHasherExtensions contains extension methods for <see cref="IHasher"/> objects.
    /// </summary>
    public static partial class IHasherExtensions
    {
        #region Hash bool-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            bool data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        public static bool TryVerifyHash(
            this IHasher hasher,
            bool data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
              this IHasher hasher,
              bool data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            bool[] data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(!(data != null ^ Contract.Result<byte[]>() != null));

            if (data == null)
                return null;

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        public static bool TryVerifyHash(
            this IHasher hasher,
            bool[] data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
              this IHasher hasher,
              bool[] data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion

        #region Hash char-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            char data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        public static bool TryVerifyHash(
            this IHasher hasher,
            char data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
              this IHasher hasher,
              char data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            char[] data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(!(data != null ^ Contract.Result<byte[]>() != null));

            if (data == null)
                return null;

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        public static bool TryVerifyHash(
            this IHasher hasher,
            char[] data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
              this IHasher hasher,
              char[] data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion

        #region Hash byte-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            byte data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        public static bool TryVerifyHash(
            this IHasher hasher,
            byte data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
            this IHasher hasher,
            byte data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion

        #region Hash sbyte-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        [CLSCompliant(false)]
        public static byte[] Hash(
            this IHasher hasher,
            sbyte data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        [CLSCompliant(false)]
        public static bool TryVerifyHash(
            this IHasher hasher,
            sbyte data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [CLSCompliant(false)]
        public static void VerifyHash(
              this IHasher hasher,
              sbyte data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        [CLSCompliant(false)]
        public static byte[] Hash(
            this IHasher hasher,
            sbyte[] data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(!(data != null ^ Contract.Result<byte[]>() != null));

            if (data == null)
                return null;

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        [CLSCompliant(false)]
        public static bool TryVerifyHash(
            this IHasher hasher,
            sbyte[] data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [CLSCompliant(false)]
        public static void VerifyHash(
              this IHasher hasher,
              sbyte[] data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion

        #region Hash short-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            short data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        public static bool TryVerifyHash(
            this IHasher hasher,
            short data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
              this IHasher hasher,
              short data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            short[] data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(!(data != null ^ Contract.Result<byte[]>() != null));

            if (data == null)
                return null;

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        public static bool TryVerifyHash(
            this IHasher hasher,
            short[] data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
              this IHasher hasher,
              short[] data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion

        #region Hash ushort-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        [CLSCompliant(false)]
        public static byte[] Hash(
            this IHasher hasher,
            ushort data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        [CLSCompliant(false)]
        public static bool TryVerifyHash(
            this IHasher hasher,
            ushort data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [CLSCompliant(false)]
        public static void VerifyHash(
              this IHasher hasher,
              ushort data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        [CLSCompliant(false)]
        public static byte[] Hash(
            this IHasher hasher,
            ushort[] data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Ensures(!(data != null ^ Contract.Result<byte[]>() != null));

            if (data == null)
                return null;

            return hasher.Hash(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <returns><c>true</c> if the hash has a value that was produced rom the <paramref name="data"/>; <c>false</c> otherwise.</returns>
        [CLSCompliant(false)]
        public static bool TryVerifyHash(
            this IHasher hasher,
            ushort[] data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            return hasher.TryVerifyHash(ToByteArray.Convert(data), hash);
        }

        /// <summary>
        /// Verifies that the passed <paramref name="hash"/> was produced from the same <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data, which hash must be verified.</param>
        /// <param name="hash">The hash to be tested.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [CLSCompliant(false)]
        public static void VerifyHash(
              this IHasher hasher,
              ushort[] data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion
    }
}
