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
    /// Class <c>EncryptedNewKeySignedCipher</c> is similar to <see cref="EncryptedNewKeyHashedCipher"/>.
    /// For document integrity it adds a cryptographic signature to the crypto package.
    /// <remarks>
    /// <para>
    /// The asymmetric key is stored in a key container with a name specified by the caller or by a certificate containing the public
    /// and possibly the private key.
    /// </para><para>
    /// By default the cipher uses the <see cref="T:System.Security.Cryptography.SHA256Cng"/> algorithm implementation for hashing.
    /// </para><para>
    /// By default the cipher uses the <see cref="T:System.Security.Cryptography.AesCryptoServiceProvider"/> with default parameters. 
    /// This can be changed by reconfiguring the Unity container and injecting any of the other symmetric ciphers. 
    /// Reconfiguration of the container allows also to change the parameters of the symmetric cipher in use.
    /// </para><para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item><description>Length of the signature (serialized Int32) - 4 bytes.</description></item>
    ///         <item><description>The bytes of the signature.</description></item>
    ///         <item><description>Length of the encrypted symmetric key (serialized Int32) - 4 bytes.</description></item>
    ///         <item><description>The bytes of the encrypted symmetric key.</description></item>
    ///         <item><description>Length of the symmetric initialization vector (serialized Int32) - 4 bytes. Must be equal to the symmetric block size divided by 8.</description></item>
    ///         <item><description>The bytes of the initialization vector.</description></item>
    ///         <item><description>The bytes of the encrypted text.</description></item>
    ///     </list>
    /// </para>
    /// </remarks>
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification="Makes sense.")]
    public class EncryptedNewKeySignedCipher : EncryptedNewKeyHashedCipher
    {
        #region Fields
        /// <summary>
        /// The signer which implements the asymmetric algorithm for encrypting the hash.
        /// </summary>
        readonly AsymmetricAlgorithm _asymmetric;
        /// <summary>
        /// Temporary stores the read signature from the crypto-package before verifying it.
        /// </summary>
        byte[] _signature;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the signer which implements the asymmetric algorithm for encrypting the hash.
        /// </summary>
        protected AsymmetricAlgorithm Asymmetric
        {
            get { return _asymmetric; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedNewKeyCipher" /> class.
        /// </summary>
        /// <param name="exchangeCertificate">
        /// The certificate containing the public and optionally the private keys for encrypting the symmetric key. Cannot be <see langword="null"/>.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;EncryptingCertificate&quot;.
        /// </param>
        /// <param name="signCertificate">
        /// The certificate containing the public and optionally the private keys for encrypting the hash - signing. Cannot be <see langword="null"/>.
        /// If the parameter is <see langword="null"/> the method will try to resolve its value from the Common Service Locator with resolve name &quot;SigningCertificate&quot;.
        /// </param>
        /// <param name="symmetricAlgorithmName">
        /// The name of the symmetric algorithm implementation. You can use any of the constants from <see cref="Algorithms.Symmetric" /> or
        /// <see langword="null" />, empty or whitespace characters only - these will default to <see cref="Algorithms.Symmetric.Default" />.
        /// Also a string instance with name &quot;DefaultSymmetricEncryption&quot; can be defined in a Common Service Locator compatible dependency injection container.
        /// </param>
        /// <param name="hashAlgorithmName">
        /// The name of the hash algorithm. By default the cipher will pick the algorithm from the <paramref name="signCertificate"/> but the caller
        /// may choose to use lower length signature key, e.g. the certificate may be for SHA256 but the caller may override that to SHA1.
        /// Also a string instance with name &quot;DefaultHash&quot; can be defined in a Common Service Locator compatible dependency injection container.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when either the <paramref name="exchangeCertificate" /> or the <paramref name="signCertificate" /> is <see langword="null" /> 
        /// and could not be resolved from the Common Service Locator.
        /// </exception>
        /// <remarks>
        /// Note that for XML signing the cipher supports only SHA1 and SHA256.
        /// </remarks>
        public EncryptedNewKeySignedCipher(
            X509Certificate2 exchangeCertificate = null,
            X509Certificate2 signCertificate = null,
            string symmetricAlgorithmName = null,
            string hashAlgorithmName = null)
            : base(exchangeCertificate, symmetricAlgorithmName, !string.IsNullOrWhiteSpace(hashAlgorithmName)
                                                                    ? hashAlgorithmName 
                                                                    : signCertificate.HashAlgorithm())
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

        #region Overrides of the primitives called by the GoF template-methods
        /// <summary>
        /// Reserves the space for the hash (the signature here).
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected override void ReserveSpaceForHash(
            Stream encryptedStream)
        {
            // reserve space in the encrypted stream for the signature
            var length = Asymmetric.KeySize / 8;
            var placeholderSignature = new byte[length];

            encryptedStream.Write(BitConverter.GetBytes(placeholderSignature.Length), 0, sizeof(int));
            encryptedStream.Write(placeholderSignature, 0, placeholderSignature.Length);
        }

        /// <summary>
        /// Writes the signature in the reserved space.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="hash">The hash.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected override void WriteHashInReservedSpace(
            Stream encryptedStream,
            byte[] hash)
        {
            try
            {
                var rsaFormatter = new RSAPKCS1SignatureFormatter(Asymmetric);

                rsaFormatter.SetHashAlgorithm(HashAlgorithmName);

                _signature = rsaFormatter.CreateSignature(hash);

                Contract.Assume(_signature.Length == Asymmetric.KeySize / 8);

                // write the hash into the reserved space
                encryptedStream.Seek(sizeof(int), SeekOrigin.Begin);
                encryptedStream.Write(_signature, 0, _signature.Length);
            }
            finally
            {
                _signature = null;
            }
        }

        /// <summary>
        /// Loads the signature to validate.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        /// <exception cref="System.ArgumentException">
        /// The input data does not represent a valid crypto package: could not read the length of the signature.
        /// or
        /// The input data does not represent a valid crypto package: could not read the signature.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected override void LoadHashToValidate(
            Stream encryptedStream)
        {
            //read the encrypted length and hash
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            if (encryptedStream.Read(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the length of the signature.", "encryptedStream");
            length = BitConverter.ToInt32(lengthBuffer, 0);

            _signature = new byte[length];

            if (encryptedStream.Read(_signature, 0, _signature.Length) != _signature.Length)
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the signature.", "encryptedStream");
        }

        /// <summary>
        /// Gives an opportunity to the inheritors to read more unencrypted information from the
        /// <paramref name="encryptedStream" /> or perform other activities, e.g. here it verifies the signature or the hash of the
        /// <paramref name="encryptedStream" />.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="cryptoStream">The crypto stream.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Invalid signature.</exception>
        /// <remarks>The method is called by the GoF template-methods.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected override void AfterReadDecrypted(
            Stream encryptedStream,
            CryptoStream cryptoStream)
        {
            try
            {
                var deformatter = new RSAPKCS1SignatureDeformatter(Asymmetric);

                deformatter.SetHashAlgorithm(HashAlgorithmName);
                if (!deformatter.VerifySignature(
                                    FinalizeHashAfterRead(encryptedStream, cryptoStream), _signature))
                    throw new CryptographicException("Invalid signature.");
            }
            finally
            {
                _signature = null;
            }
        }
        #endregion

#if NET45
        #region Overrides of the async primitives
        /// <summary>
        /// Loads asynchronously the signature to validate.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <exception cref="System.ArgumentNullException">encryptedStream</exception>
        /// <exception cref="System.ArgumentException">
        /// The input data does not represent a valid crypto package: could not read the length of the signature.
        /// or
        /// The input data does not represent a valid crypto package: could not read the signature.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        protected override async Task LoadHashToValidateAsync(
            Stream encryptedStream)
        {
            //read the encrypted length and hash
            var lengthBuffer = new byte[sizeof(int)];
            var length = 0;

            if (await encryptedStream.ReadAsync(lengthBuffer, 0, sizeof(int)) != sizeof(int))
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the length of the signature.", "encryptedStream");
            length = BitConverter.ToInt32(lengthBuffer, 0);

            _signature = new byte[length];

            if (await encryptedStream.ReadAsync(_signature, 0, _signature.Length) != _signature.Length)
                throw new ArgumentException("The input data does not represent a valid crypto package: could not read the signature.", "encryptedStream");
        }
        #endregion
#endif

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
                Asymmetric.Dispose();

            base.Dispose(disposing);
        }
        #endregion

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(Asymmetric != null, "The asymmetric algorithm cannot be null.");
        }
    }
}
