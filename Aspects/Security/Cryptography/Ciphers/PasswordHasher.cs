using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The class PasswordHasher is appropriate for hashing passwords because it is intentionally time consuming operation (with default number of iterations x100ms),
    /// making it impossible to attack with brute force methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The hasher uses the the PBKDF2/HMAC/SHA1 in accordance to RFC 2898.
    /// </para>
    /// Crypto package:
    /// <list type="number">
    ///     <item>The number of iterations (serialized Int32) - 4 bytes.</item>
    ///     <item>The length of the salt (serialized Int32) - 4 bytes.</item>
    ///     <item>The bytes of the salt.</item>
    ///     <item>The length of the hash (serialized Int32) - 4 bytes.</item>
    ///     <item>The bytes of the hash.</item>
    /// </list>
    /// </remarks>
    public class PasswordHasher : IHasherAsync
    {
        #region fields
        readonly int _hashLength;
        readonly int _numberOfIterations;
        int _saltLength;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordHasher" /> class.
        /// </summary>
        /// <param name="numberOfIterations">
        /// The number of iterations, the default value is <see cref="PasswordDerivationConstants.DefaultNumberOfIterations" />.
        /// The greater the iterations the more secure is the hash but is also slower. 
        /// Should not be less than <see cref="PasswordDerivationConstants.DefaultNumberOfIterations" />.
        /// </param>
        /// <param name="hashLength">
        /// The length of the hash in bytes. The minimum and default length is <see cref="PasswordDerivationConstants.DefaultHashLength"/>.
        /// </param>
        /// <param name="saltLength">
        /// The length of the salt, the default value is <see cref="PasswordDerivationConstants.DefaultSaltLength" /> bytes. 
        /// Must be at least <see cref="PasswordDerivationConstants.MinSaltLength" /> bytes.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if 
        /// <list type="bullet">
        /// <item>the <paramref name="numberOfIterations" /> is less than <see cref="PasswordDerivationConstants.MinNumberOfIterations" /> bytes; or</item>
        /// <item>the <paramref name="hashLength" /> is less than <see cref="PasswordDerivationConstants.DefaultHashLength" /> bytes; or</item>
        /// <item>the <paramref name="saltLength" /> is less than <see cref="PasswordDerivationConstants.MinSaltLength" /> bytes.</item>
        /// </list>
        /// </exception>
        public PasswordHasher(
            int numberOfIterations = PasswordDerivationConstants.DefaultNumberOfIterations,
            int hashLength = PasswordDerivationConstants.DefaultHashLength,
            int saltLength = PasswordDerivationConstants.DefaultSaltLength)
        {
            if (numberOfIterations < PasswordDerivationConstants.MinNumberOfIterations)
                throw new ArgumentException(
                            $"The number of iterations should be at least {PasswordDerivationConstants.MinNumberOfIterations} bytes long.",
                            nameof(numberOfIterations));

            if (hashLength < PasswordDerivationConstants.MinHashLength)
                throw new ArgumentException(
                            $"The hash should be at least {PasswordDerivationConstants.MinHashLength} bytes long.",
                            nameof(hashLength));

            if (saltLength < PasswordDerivationConstants.MinSaltLength)
                throw new ArgumentException(
                            $"Password hashes must always be salted with at least {PasswordDerivationConstants.MinSaltLength} bytes.",
                            nameof(saltLength));

            _saltLength         = saltLength;
            _hashLength         = hashLength;
            _numberOfIterations = numberOfIterations;
        }

        #region IHasher Members

        /// <summary>
        /// Computes the hash of a <paramref name="dataStream" /> stream.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns>
        /// The crypto package containing the hash. If <paramref name="dataStream" /> is <see langword="null" /> returns <see langword="null" />.
        /// </returns>
        /// <exception cref="System.IO.IOException">Unexpected number of bytes read from the data stream.</exception>
        public byte[] Hash(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;
            if (!dataStream.CanRead)
                throw new ArgumentException("The stream cannot be read.");

            var data = new byte[dataStream.Length];

            if (dataStream.Read(data, 0, data.Length) != data.Length)
                throw new IOException("Unexpected number of bytes read from the data stream.");

            return Hash(data);
        }

        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="dataStream" /> is correct.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The crypto package containing the hash to verify.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="hash" /> is correct or if both <paramref name="dataStream" /> and <paramref name="hash" /> are <see langword="null" />, 
        /// otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="System.IO.IOException">Unexpected number of bytes read from the data stream.</exception>
        public bool TryVerifyHash(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream == null)
                return hash == null;
            if (!dataStream.CanRead)
                throw new ArgumentException("The stream cannot be read.");

            var data = new byte[dataStream.Length];

            if (dataStream.Read(data, 0, data.Length) != data.Length)
                throw new IOException("Unexpected number of bytes read from the data stream.");

            return TryVerifyHash(data, hash);
        }

        /// <summary>
        /// hash as an asynchronous operation.
        /// </summary>
        /// <param name="dataStream">The data stream to compute the hash of.</param>
        /// <returns>A <see cref="T:Task{byte[]}" /> object representing the hashing process and the end result -
        /// a hash of the stream, optionally prepended with the generated salt.
        /// If <paramref name="dataStream" /> is <see langword="null" /> returns <see langword="null" />.</returns>
        /// <exception cref="System.IO.IOException">Unexpected number of bytes read from the data stream.</exception>
        public async Task<byte[]> HashAsync(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;
            if (!dataStream.CanRead)
                throw new ArgumentException("The stream cannot be read.");

            var data = new byte[dataStream.Length];

            if (await dataStream.ReadAsync(data, 0, data.Length) != data.Length)
                throw new IOException("Unexpected number of bytes read from the data stream.");

            return await Task.Run(() => Hash(data));
        }

        /// <summary>
        /// try verify hash as an asynchronous operation.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The hash to verify, optionally prepended with the salt.</param>
        /// <returns>A <see cref="T:Task{bool}" /> object representing the process and the verification result:
        /// <see langword="true" />
        /// if <paramref name="hash" /> is correct or if both <paramref name="dataStream" /> and <paramref name="hash" /> are <see langword="null" />,
        /// otherwise <see langword="false" />.</returns>
        /// <exception cref="System.IO.IOException">Unexpected number of bytes read from the data stream.</exception>
        public async Task<bool> TryVerifyHashAsync(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream == null)
                return hash == null;
            if (!dataStream.CanRead)
                throw new ArgumentException("The stream cannot be read.");

            var data = new byte[dataStream.Length];

            if (await dataStream.ReadAsync(data, 0, data.Length) != data.Length)
                throw new IOException("Unexpected number of bytes read from the data stream.");

            return await Task.Run(() => TryVerifyHash(data, hash));
        }

        /// <summary>
        /// Computes the hash of a specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <returns>The hash of the <paramref name="data" /> with optionally prepended generated salt or
        /// <see langword="null" /> if <paramref name="data" /> is <see langword="null" />.</returns>
        public byte[] Hash(
            byte[] data)
        {
            if (data == null)
                return null;

            var salt = new byte[_saltLength].FillRandom();

            using (var stream = new MemoryStream())
            {
                stream.Write(BitConverter.GetBytes(_numberOfIterations), 0, sizeof(int));
                stream.Write(BitConverter.GetBytes(_saltLength), 0, sizeof(int));
                stream.Write(salt, 0, _saltLength);
                stream.Write(BitConverter.GetBytes(_hashLength), 0, sizeof(int));
                stream.Write(GetHash(data, salt, _numberOfIterations, _hashLength), 0, _hashLength);
                stream.Close();

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Verifies the hash of the specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data which hash needs to be verified.</param>
        /// <param name="hash">The hash to be verified optionally prepended with salt.</param>
        /// <returns>
        /// <see langword="true" /> if the hash is correct or if both <paramref name="data" /> and <paramref name="hash" /> are <see langword="null" />, 
        /// otherwise <see langword="false" />.
        /// </returns>
        public bool TryVerifyHash(
            byte[] data,
            byte[] hash)
        {
            if (data == null)
                return hash==null;
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));

            using (var stream = new MemoryStream(hash))
            {
                var intBuffer = new byte[sizeof(int)];

                stream.Read(intBuffer, 0, sizeof(int));

                var iterations = BitConverter.ToInt32(intBuffer, 0);

                stream.Read(intBuffer, 0, sizeof(int));

                var saltLength = BitConverter.ToInt32(intBuffer, 0);

                if (saltLength < PasswordDerivationConstants.MinSaltLength)
                    return false;

                var salt = new byte[saltLength];

                stream.Read(salt, 0, saltLength);
                stream.Read(intBuffer, 0, sizeof(int));

                var hashLength = BitConverter.ToInt32(intBuffer, 0);

                if (hashLength == 0)
                    return false;

                var nakedHash = new byte[hashLength];

                stream.Read(nakedHash, 0, hashLength);

                return GetHash(data, salt, iterations, hashLength)
                            .ConstantTimeEquals(nakedHash);
            }
        }

        /// <summary>
        /// Gets or sets the length of the salt in bytes.
        /// </summary>
        /// <exception cref="System.ArgumentException">value</exception>
        public int SaltLength
        {
            get { return _saltLength; }
            set
            {
                if (value < PasswordDerivationConstants.MinSaltLength)
                    throw new ArgumentException(
                                $"Password hashes must always be salted with at least {PasswordDerivationConstants.MinSaltLength} bytes.",
                                "value");
                _saltLength = value;
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether this instance should "salt" the data before hashing it.
        /// This hasher ignores this property and always salts the hash.
        /// </summary>
        /// <value><c>true</c> if [should salt]; otherwise, <c>false</c>.</value>
        public bool ShouldSalt => SaltLength > 0;

        static byte[] GetHash(
            byte[] data,
            byte[] salt,
            int numberOfIterations,
            int hashLength)
        {
            using (var derivedBytes = new Rfc2898DeriveBytes(data, salt, numberOfIterations))
                return derivedBytes.GetBytes(hashLength);
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag is being set when the object gets disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="M:Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(true)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="M:Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~PasswordHasher()"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="M:Dispose()"/>, it will try to release all managed resources 
        /// (usually aggregated objects which implement <see cref="IDisposable"/> as well) and then it will release all unmanaged resources if any.
        /// If the parameter is <c>false</c> then the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // nothing to dispose here.
        }
        #endregion
    }
}
