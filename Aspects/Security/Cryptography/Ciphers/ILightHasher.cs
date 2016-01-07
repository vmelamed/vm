using System;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Interface ILightHasher defines a behavior where the implementing hasher drops certain composed data (e.g. public/private keys) and some associated functionality (e.g. IKeyManagement)
    /// and remains a pure hasher that is used for hashing/hash verification only. This is useful when many ciphers need to be cached in memory for a long time.
    /// </summary>
    public interface ILightHasher
    {
        /// <summary>
        /// Releases the associated asymmetric keys. By doing so the instance looses its <see cref="IKeyManagement" /> behavior but the memory footprint becomes much lighter.
        /// The certificate can be dropped only if the underlying symmetric algorithm instance is already initialized.
        /// </summary>
        /// <returns>The hasher.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the underlying hash instance is not initialized yet or if the hashing/hash verification functionality requires asymmetric encryption as well, e.g. signing.
        /// </exception>
        IHasherAsync ReleaseCertificate();

        /// <summary>
        /// Creates a new, lightweight clone off of the current hasher and copies certain characteristics, e.g. the hashing key of this instance to it.
        /// </summary>
        /// <returns>The clone.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the underlying hashing algorithm instance is not initialized yet or if the hashing/hash verification functionality requires asymmetric encryption, e.g. signing.
        /// </exception>
        IHasherAsync CloneLightHasher();
    }
}
