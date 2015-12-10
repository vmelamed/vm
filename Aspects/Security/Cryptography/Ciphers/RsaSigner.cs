using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class RsaSigner generates an encrypted hash (signature) of the protected data. 
    /// The signer supports only SHA1-RSA and SHA256-RSA.
    /// </summary>
    public class RsaSigner : Hasher
    {
        /// <summary>
        /// The implementation of the asymmetric algorithm.
        /// </summary>
        readonly AsymmetricAlgorithm _asymmetric;

        #region Properties
        /// <summary>
        /// Gets or sets the implementation of the asymmetric algorithm.
        /// </summary>
        protected AsymmetricAlgorithm Asymmetric
        {
            get
            {
                Contract.Ensures(Contract.Result<AsymmetricAlgorithm>() != null);

                return _asymmetric;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedNewKeyCipher" /> class.
        /// </summary>
        /// <param name="hashAlgorithmName">
        /// The name of the symmetric algorithm implementation. Use any of the constants from <see cref="Algorithms.Symmetric"/> or
        /// <see langword="null"/>, empty or whitespace characters only - it will default to <see cref="Algorithms.Symmetric.Default"/>.
        /// </param>
        /// <param name="signCertificate">
        /// The certificate containing the public and optionally the private key.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;SigningCertificate&quot;.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="signCertificate"/> is <see langword="null"/> and could not be resolved from the Common Service Locator.
        /// </exception>
        public RsaSigner(
            X509Certificate2 signCertificate = null,
            string hashAlgorithmName = null)
            : base(hashAlgorithmName==null ? signCertificate.HashAlgorithm() : hashAlgorithmName, 0)
        {
            if (signCertificate == null)
                try
                {
                    signCertificate = ServiceLocatorWrapper.Default.GetInstance<X509Certificate2>(Algorithms.Hash.CertificateResolveName);
                }
                catch (ActivationException x)
                {
                    throw new ArgumentNullException("The parameter \"signCertificate\" was null and could not be resolved from the Common Service Locator.", x);
                }

            _asymmetric = signCertificate.HasPrivateKey
                                ? (RSACryptoServiceProvider)signCertificate.PrivateKey
                                : (RSACryptoServiceProvider)signCertificate.PublicKey.Key;
        }
        #endregion

        #region IHasher Members
        /// <summary>
        /// Computes the hash of a <paramref name="dataStream" /> stream.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns>The hash of the stream optionally prepended with the generated salt or <see langword="null" /> if <paramref name="dataStream" /> is <see langword="null" />.</returns>
        /// <exception cref="System.InvalidOperationException">The certificate did not contain a private key.</exception>
        public override byte[] Hash(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;

            var hash = base.Hash(dataStream);
            var formatter = new RSAPKCS1SignatureFormatter(Asymmetric);

            formatter.SetHashAlgorithm(HashAlgorithmName);
            return formatter.CreateSignature(hash);
        }

        /// <summary>
        /// Tries the verify hash.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="signature">The signature.</param>
        /// <returns><see langword="true" /> if the hash is valid, <see langword="false" /> otherwise.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#", Justification = "n/a")]
        public override bool TryVerifyHash(
            Stream dataStream,
            byte[] signature)
        {
            if (dataStream==null)
                return signature == null;

            var hash = base.Hash(dataStream);
            var deformatter = new RSAPKCS1SignatureDeformatter(Asymmetric);

            deformatter.SetHashAlgorithm(HashAlgorithmName);
            return deformatter.VerifySignature(hash, signature);
        }

        /// <summary>
        /// Computes the hash of a specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <returns>
        /// The hash of the <paramref name="data" /> optionally prepended with the generated salt or 
        /// <see langword="null" /> if <paramref name="data" /> is <see langword="null" />.
        /// </returns>
        public override byte[] Hash(
            byte[] data)
        {
            if (data == null)
                return null;

            var hash = base.Hash(data);
            var formatter = new RSAPKCS1SignatureFormatter(Asymmetric);

            formatter.SetHashAlgorithm(HashAlgorithmName);
            return formatter.CreateSignature(hash);
        }

        /// <summary>
        /// Tries the verify hash.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="signature">The signature.</param>
        /// <returns><see langword="true" /> if the hash is valid, <see langword="false" /> otherwise.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#", Justification = "n/a")]
        public override bool TryVerifyHash(
            byte[] data,
            byte[] signature)
        {
            if (data == null)
                return signature==null;

            var hash = base.Hash(data);
            var deformatter = new RSAPKCS1SignatureDeformatter(Asymmetric);

            deformatter.SetHashAlgorithm(HashAlgorithmName);
            return deformatter.VerifySignature(hash, signature);
        }
        #endregion

        #region IHasherAsync methods
        /// <summary>
        /// hash as an asynchronous operation.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns>The hash of the stream optionally prepended with the generated salt or <see langword="null" /> if <paramref name="dataStream" /> is <see langword="null" />.</returns>
        public override async Task<byte[]> HashAsync(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;

            var hash = await base.HashAsync(dataStream);
            var formatter = new RSAPKCS1SignatureFormatter(Asymmetric);

            formatter.SetHashAlgorithm(HashAlgorithmName);
            return formatter.CreateSignature(hash);
        }

        /// <summary>
        /// try to verify hash as an asynchronous operation.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#", Justification = "n/a")]
        public override async Task<bool> TryVerifyHashAsync(
            Stream dataStream,
            byte[] signature)
        {
            if (dataStream==null)
                return signature == null;

            var hash = await base.HashAsync(dataStream);
            var deformatter = new RSAPKCS1SignatureDeformatter(Asymmetric);

            deformatter.SetHashAlgorithm(HashAlgorithmName);
            return deformatter.VerifySignature(hash, signature);
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="M:Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~Signer()"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="M:Dispose()"/>, it will try to release all managed resources 
        /// (usually aggregated objects which implement <see cref="IDisposable"/> as well) and then it will release all unmanaged resources if any.
        /// If the parameter is <c>false</c> then the method will only try to release the unmanaged resources.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _asymmetric.Dispose();

            base.Dispose(disposing);
        }
        #endregion

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(_asymmetric != null, "The asymmetric algorithm cannot be null.");
        }
    }
}
