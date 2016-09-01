using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Interface ILightCipher defines a behavior where the implementing cipher drops certain composed data (e.g. public/private keys) and some associated functionality (e.g. IKeyManagement)
    /// and remains a pure cipher that is used for encryption/decryption only. This is useful when many ciphers need to be cached in memory for a long time.
    /// </summary>
    [ContractClass(typeof(ILightCipherContract))]
    public interface ILightCipher
    {
        /// <summary>
        /// Releases the associated asymmetric keys. By doing so the instance looses its <see cref="IKeyManagement" /> behavior but the memory footprint becomes much lighter.
        /// The certificate can be dropped only if the underlying symmetric algorithm instance is already initialized.
        /// </summary>
        /// <returns>The cipher.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the underlying symmetric algorithm instance is not initialized yet or if the encryption/decryption functionality requires asymmetric encryption as well,
        /// e.g. encryption of the IV.
        /// </exception>
        ICipherAsync ReleaseCertificate();

        /// <summary>
        /// Creates a new, lightweight clone off of the current cipher and copies certain characteristics, e.g. the symmetric key of this instance to it.
        /// </summary>
        /// <returns>The clone.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the underlying symmetric algorithm instance is not initialized yet or if the encryption/decryption functionality requires asymmetric encryption as well,
        /// e.g. encryption of the IV.
        /// </exception>
        ICipherAsync CloneLightCipher();
    }

    [ContractClassFor(typeof(ILightCipher))]
    abstract class ILightCipherContract : ILightCipher
    {
        public ICipherAsync CloneLightCipher()
        {
            Contract.Ensures(Contract.Result<ICipherAsync>() != null);

            throw new NotImplementedException();
        }

        public ICipherAsync ReleaseCertificate()
        {
            Contract.Ensures(Contract.Result<ICipherAsync>() != null);

            throw new NotImplementedException();
        }
    }
}
