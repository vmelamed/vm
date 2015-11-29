using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class IHasherExtensions contains extension methods for <see cref="IHasher"/> objects.
    /// </summary>
    public static partial class IHasherExtensions
    {
        #region VerifyHash - throwing exception
        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="data" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <param name="hash">The hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
            this IHasher hasher,
            byte[] data,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            if (data != null && !hasher.TryVerifyHash(data, hash))
                throw new CryptographicException("Invalid hash.");
        }

        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="dataStream" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        public static void VerifyHash(
            this IHasher hasher,
            Stream dataStream,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");
            Contract.Requires<CryptographicException>(dataStream != null || hash == null, "Invalid hash.");

            if (dataStream != null && !hasher.TryVerifyHash(dataStream, hash))
                throw new CryptographicException("Invalid hash.");
        }
        #endregion

        #region HashText and base64 obsolete methods.
        /// <summary>
        /// Generates hash of the specified text.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="text">The text.</param>
        /// <returns>The generated hash.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hasher"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        [Obsolete("Use the overloaded hasher.Hash(string data) instead.")]
        public static byte[] HashText(
            this IHasher hasher,
            string text)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");

            if (text == null)
                return null;

            return hasher.Hash(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="text" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="text">The text.</param>
        /// <param name="hash">The hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [Obsolete("Use the overloaded cipher.VerifyHash(string data, byte[] hash) instead.")]
        public static void VerifyHashText(
            this IHasher hasher,
            string text,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");
            Contract.Requires<CryptographicException>(text != null || hash == null, "Invalid hash.");

            if (text != null && !hasher.TryVerifyHash(Encoding.UTF8.GetBytes(text), hash))
                throw new CryptographicException("Invalid hash.");
        }

        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="text" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="text">The text.</param>
        /// <param name="hash">The hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [Obsolete("Use the overloaded cipher.VerifyHash(string data, byte[] hash) instead.")]
        public static void VerifyTextHash(
            this IHasher hasher,
            string text,
            byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");
            Contract.Requires<CryptographicException>(text != null || hash == null, "Invalid hash.");

            VerifyHashText(hasher, text, hash);
        }

        /// <summary>
        /// Generates hash of the specified data and encode is in Base64.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The generated Base64 encoded hash.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hasher" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DataBase", Justification = "Follows naming convention and has nothing to do with databases.")]
        [Obsolete("Chain hasher.Hash(data).ToBase64String() instead.")]
        public static string HashDataBase64(
            this IHasher hasher,
            byte[] data)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");

            var hash = hasher.Hash(data);

            if (hash == null)
                return null;

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifies that the Base64 encoded <paramref name="hash64" /> of a <paramref name="data" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <param name="hash64">The Base64 encoded hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [Obsolete("Use hasher.VerifyHash(data, hash64.FromBase64String()) instead.")]
        public static void VerifyHashBase64(
            this IHasher hasher,
            byte[] data,
            string hash64)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");
            Contract.Requires<CryptographicException>(data != null || hash64 == null, "Invalid hash.");

            if (data == null)
                return;

            var hash = Convert.FromBase64String(hash64);

            if (!hasher.TryVerifyHash(data, hash))
                throw new CryptographicException("Invalid hash.");
        }

        /// <summary>
        /// Verifies that the Base64 encoded <paramref name="hash64" /> of a <paramref name="text" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="text">The text.</param>
        /// <param name="hash64">The Base64 encoded hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [Obsolete("Use hasher.VerifyHash(text, hash64.FromBase64String()) instead.")]
        public static void VerifyTextHashBase64(
            this IHasher hasher,
            string text,
            string hash64)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");
            Contract.Requires<CryptographicException>(text != null || hash64 == null, "Invalid hash.");

            if (text == null)
                return;

            var hash = Convert.FromBase64String(hash64);

            if (!hasher.TryVerifyHash(Encoding.UTF8.GetBytes(text), hash))
                throw new CryptographicException("Invalid hash.");
        }

        /// <summary>
        /// Verifies that the Base64 encoded <paramref name="hash64" /> of a <paramref name="dataStream" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash64">The hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [Obsolete("Use hasher.VerifyHash(dataStream, hash64.FromBase64String()) instead.")]
        public static void VerifyHashBase64(
            this IHasher hasher,
            Stream dataStream,
            string hash64)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");
            Contract.Requires<CryptographicException>(dataStream != null || hash64 == null, "Invalid hash.");

            if (dataStream == null)
                return;

            var hash = Convert.FromBase64String(hash64);

            if (!hasher.TryVerifyHash(dataStream, hash))
                throw new CryptographicException("Invalid hash.");
        }
        #endregion

        #region Hash strin-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            string data)
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
            string data,
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
              string data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<ArgumentNullException>(hash != null, nameof(hash));

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion

        #region Hash DateTime-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            DateTime data)
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
            DateTime data,
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
              DateTime data,
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
            DateTime[] data)
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
            DateTime[] data,
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
              DateTime[] data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion

        #region Hash Guid-s
        /// <summary>
        /// Hashes the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The hash value as a byte array.</returns>
        public static byte[] Hash(
            this IHasher hasher,
            Guid data)
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
            Guid data,
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
              Guid data,
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
            Guid[] data)
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
            Guid[] data,
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
              Guid[] data,
              byte[] hash)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, nameof(hasher));
            Contract.Requires<CryptographicException>(data != null || hash == null, "Invalid hash.");

            hasher.VerifyHash(ToByteArray.Convert(data), hash);
        }
        #endregion
    }
}
