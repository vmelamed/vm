using System.Security.Cryptography.X509Certificates;
using vm.Aspects.Security;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Encapsulates the data by which to find a certificate in the Windows certificate stores.
    /// </summary>
    public class CertificateData
    {
        /// <summary>
        /// In case the remote site uses client certificates this property identifies the store location where the client certificate is.
        /// </summary>
        /// <value>The store location where the store with the client certificate should be found.</value>
        public StoreLocation StoreLocation { get; set; }

        /// <summary>
        /// In case the remote site uses client certificates this property identifies the store where the client certificate is.
        /// </summary>
        /// <value>The store where the client certificate should be found.</value>
        public StoreName StoreName { get; set; }

        /// <summary>
        /// Gets or sets the part by which the certificate can be found in the store.
        /// </summary>
        /// <value>
        /// The field by which the certificate can be found in the store.
        /// </value>
        /// <remarks>
        /// When extracting the certificate the property <see cref="Certificate"/> always combines this criterion with 
        /// <see cref="X509FindType.FindByTimeValid"/> and the current time, so specifying this value as well as the other time related criteria 
        /// does not make sense.
        /// </remarks>
        public X509FindType FindBy { get; set; }

        /// <summary>
        /// Gets or sets the certificate field value by which it can be found in the store.
        /// </summary>
        /// <value>
        /// The certificate field value by which it can be found in the store.
        /// </value>
        public string FindByValue { get; set; }

        /// <summary>
        /// Extracts the first 509 certificate from the Windows certificate stores which matches the <see cref="FindBy"/> and combined with 
        /// <see cref="X509FindType.FindByTimeValid"/> and the current date and time.
        /// </summary>
        /// <returns>
        /// The 509 certificate or <c>null</c> if the instance does not specify certificate, e.g. all fields are <c>null</c>.
        /// </returns>
        public X509Certificate2 Certificate
            => CertificateFactory.GetCertificate(StoreLocation, StoreName, FindBy, FindByValue);
    }
}
