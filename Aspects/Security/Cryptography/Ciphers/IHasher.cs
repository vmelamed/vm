using System;
using System.Diagnostics.Contracts;
using System.IO;
using vm.Aspects.Security.Cryptography.Ciphers.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface <c>IHasher</c> defines the behavior of objects which are configured according to some cryptographic scheme and
    /// have the responsibility to protect data for integrity, and possibly authentication by generating cryptographically strong hashes and signatures.
    /// </summary>
    [ContractClass(typeof(IHasherContract))]
    public interface IHasher : IDisposable
    {
        /// <summary>
        /// Gets or sets the length of the salt in bytes. If set to 0 salt will not be applied to the hash.
        /// </summary>        
        int SaltLength { get; set; }

        /// <summary>
        /// Computes the hash of the <paramref name="dataStream" />.
        /// </summary>
        /// <param name="dataStream">
        /// The data stream to compute the hash of.
        /// </param>
        /// <returns>
        /// The hash of the stream, optionally prepended with the generated salt.
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
        byte[] Hash(Stream dataStream);

        /// <summary>
        /// Verifies that the <paramref name="hash"/> of a <paramref name="dataStream" /> is correct.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The hash to verify, optionally prepended with the salt.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="hash"/> is correct or if both <paramref name="dataStream" /> and <paramref name="hash"/> are <see langword="null" />, 
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="dataStream" /> is not <see langword="null" /> and <paramref name="hash"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="hash"/> has invalid length.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        bool TryVerifyHash(Stream dataStream, byte[] hash);

        /// <summary>
        /// Computes the hash of a specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <returns>
        /// The hash of the <paramref name="data"/>, optionally prepended with the generated salt.
        /// If <paramref name="data" /> is <see langword="null" /> returns <see langword="null" />.
        /// </returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        byte[] Hash(byte[] data);

        /// <summary>
        /// Verifies the hash of the specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data which hash needs to be verified.</param>
        /// <param name="hash">The hash to be verified, optionally prepended with the salt.</param>
        /// <returns>
        /// <see langword="true" /> if the hash is correct or if both <paramref name="data" /> and <paramref name="hash"/> are <see langword="null" />, otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hash"/> is <see langword="null" /> and <paramref name="data" /> is not.</exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The hash or the encryption failed.</exception>
        bool TryVerifyHash(byte[] data, byte[] hash);
    }
}
