using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The class NullHasher is a development and test-friendly convenience class which implements trivially the <see cref="IHasherTasks"/> interface:
    /// generates an empty array for hash and always verifies the hash to be correct.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "N/A")]
    public class NullHasher : IHasher, IHasherTasks
    {
        #region IHasher Members
        /// <summary>
        /// Computes the hash of a specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <returns>
        /// If <paramref name="data" /> is <see langword="null" /> returns <see langword="null" />;
        /// otherwise always returns a 0-length byte array.
        /// </returns>
        public byte[] Hash(
            byte[] data)
        {
            if (data == null)
                return null;
            return new byte[8];
        }

        /// <summary>
        /// Computes the hash of a <paramref name="dataStream" /> stream.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns>
        /// If <paramref name="dataStream" /> is <see langword="null" /> returns <see langword="null" />;
        /// otherwise always returns a 0-length byte array.
        /// </returns>
        public byte[] Hash(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));
            return new byte[8];
        }

        /// <summary>
        /// Verifies the hash of the <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data which hash needs to be verified.</param>
        /// <param name="hash">The hash to be verified optionally prepended with salt.</param>
        /// <returns>Always <see langword="true" />.</returns>
        public bool TryVerifyHash(
            byte[] data,
            byte[] hash)
        {
            if (data == null)
                return hash==null;
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));
            return true;
        }

        /// <summary>
        /// Verifies the hash of the <paramref name="dataStream" />.
        /// </summary>
        /// <param name="dataStream">The data which hash needs to be verified.</param>
        /// <param name="hash">The hash to be verified optionally prepended with salt.</param>
        /// <returns>Always <see langword="true" />.</returns>
        public bool TryVerifyHash(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream == null)
                return hash==null;
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));
            return true;
        }

        int _saltLength;
        /// <summary>
        /// Gets or sets the length of the salt in bytes. If set to 0 salt will not be applied to the hash.
        /// </summary>
        public int SaltLength
        {
            get { return _saltLength; }
            set
            {
                if (value != 0  &&  value < 8)
                    throw new ArgumentException("The salt length should be either 0 or not less than 8 bytes.", nameof(value));

                _saltLength = value;
            }
        }
        #endregion

        #region IHasherAsync implementation
        /// <summary>
        /// hash as an asynchronous operation.
        /// </summary>
        /// <param name="dataStream">The data stream to compute the hash of.</param>
        /// <returns>A <see cref="T:Task{byte[]}" /> object representing the hashing process and the end result -
        /// a hash of the stream, optionally prepended with the generated salt.
        /// If <paramref name="dataStream" /> is <see langword="null" /> returns <see langword="null" />.</returns>
        public async Task<byte[]> HashAsync(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            return await Task.FromResult(new byte[8]);
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
        public async Task<bool> TryVerifyHashAsync(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream == null)
                return hash == null;
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            return await Task.FromResult(true);
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "N/A")]
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
