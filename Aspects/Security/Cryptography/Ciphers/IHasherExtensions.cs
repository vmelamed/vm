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
    public static class IHasherExtensions
    {
        /// <summary>
        /// Generates hash of the specified text.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="text">The text.</param>
        /// <returns>The generated hash.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hasher"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [Obsolete("VerifyTextHash was renamed VerifyHashText")]
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
        /// Generates hash of the specified text and encode is in Base64.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="text">The text.</param>
        /// <returns>The generated Base64 encoded hash.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hasher" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static string HashTextBase64(
            this IHasher hasher,
            string text)
        {
            Contract.Requires<ArgumentNullException>(hasher != null, "hasher");

            if (text == null)
                return null;

            var hash = hasher.Hash(Encoding.UTF8.GetBytes(text));

            if (hash == null)
                return null;

            return Convert.ToBase64String(hash);
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
        /// Generates hash of the specified data and encode is in Base64.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The generated Base64 encoded hash.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hasher" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DataBase", Justification = "Follows naming convention and has nothing to do with databases.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
        /// Verifies that the <paramref name="hash" /> of a <paramref name="data" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="data">The data.</param>
        /// <param name="hash">The hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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

        /// <summary>
        /// Verifies that the Base64 encoded <paramref name="hash64" /> of a <paramref name="dataStream" /> is correct.
        /// If it is not, the method throws <see cref="T:System.Security.Cryptography.CryptographicException" />.
        /// </summary>
        /// <param name="hasher">The hasher.</param>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash64">The hash to verify with (optionally) appended the generated salt.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="hasher"/> is <see langword="null" />.</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the hash is not valid.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
    }
}
