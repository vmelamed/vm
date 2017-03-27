using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Interface <c>IHasherAsync</c> extends <see cref="IHasher"/> with 
    /// asynchronous versions of its <see cref="Stream"/> related methods.
    /// </summary>
    [ContractClass(typeof(IHasherAsyncContract))]
    public interface IHasherAsync : IHasher
    {
        /// <summary>
        /// Computes the hash of the <paramref name="dataStream" /> asynchronously.
        /// </summary>
        /// <param name="dataStream">
        /// The data stream to compute the hash of.
        /// </param>
        /// <returns>
        /// A <see cref="T:Task{byte[]}"/> object representing the hashing process and the end result -
        /// a hash of the stream, optionally prepended with the generated salt.
        /// If <paramref name="dataStream" /> is <see langword="null" /> returns <see langword="null" />.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// Thrown when <paramref name="dataStream"/> cannot be read.
        /// </exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        /// The hash or the encryption failed.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurred.
        /// </exception>
        Task<byte[]> HashAsync(Stream dataStream);

        /// <summary>
        /// Asynchronously verifies that the <paramref name="hash"/> of a <paramref name="dataStream" /> is correct.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The hash to verify, optionally prepended with the salt.</param>
        /// <returns>
        /// A <see cref="T:Task{bool}"/> object representing the process and the verification result:
        /// <see langword="true"/>
        /// if <paramref name="hash"/> is correct or if both <paramref name="dataStream" /> and <paramref name="hash"/> are <see langword="null" />, 
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="dataStream" /> is not <see langword="null" /> and <paramref name="hash"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="hash"/> has invalid length.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        Task<bool> TryVerifyHashAsync(Stream dataStream, byte[] hash);
    }

    [ContractClassFor(typeof(IHasherAsync))]
    abstract class IHasherAsyncContract : IHasherAsync
    {
        #region IHasherAsync Members

        public Task<byte[]> HashAsync(Stream dataStream)
        {
            Contract.Requires<ArgumentException>(dataStream==null || dataStream.CanRead, "The "+nameof(dataStream)+" cannot be read from.");
            Contract.Ensures(!(dataStream==null ^ Contract.Result<byte[]>()==null), "The returned value is invalid.");

            throw new NotImplementedException();
        }

        public Task<bool> TryVerifyHashAsync(Stream dataStream, byte[] hash)
        {
            Contract.Requires<ArgumentException>(dataStream==null || dataStream.CanRead, "The "+nameof(dataStream)+" cannot be read from.");
            Contract.Requires<ArgumentNullException>(dataStream==null || hash!=null, nameof(hash));
            Contract.Ensures(dataStream!=null || Contract.Result<bool>()==(hash==null), "Invalid return value.");

            throw new NotImplementedException();
        }

        #endregion

        #region IHasher Members

        public int SaltLength
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public byte[] Hash(System.IO.Stream dataStream)
        {
            throw new NotImplementedException();
        }

        public bool TryVerifyHash(System.IO.Stream dataStream, byte[] hash)
        {
            throw new NotImplementedException();
        }

        public byte[] Hash(byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool TryVerifyHash(byte[] data, byte[] hash)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
