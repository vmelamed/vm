using System;
using System.Security.Cryptography.X509Certificates;

namespace vm.Aspects.Security
{
    /// <summary>
    /// Class CertificateFactory provides a method for fetching certificates from the Windows certificate stores.
    /// </summary>
    public static class CertificateFactory
    {
        /// <summary>
        /// Gets a certificate from the Windows certificate stores.
        /// </summary>
        /// <param name="storeLocation">The store location.</param>
        /// <param name="storeName">The name of the store.</param>
        /// <param name="findByType">The type of the value to find the certificate by.</param>
        /// <param name="findByValue">The value to find the certificate by.</param>
        /// <param name="ignoreTimeValid">
        /// If set to <see langword="true" /> the method will ignore the time valid property of the certificate.
        /// otherwise the method will return <see langword="null"/> if the found certificate has expired.
        /// </param>
        /// <returns>
        /// The first X509Certificate2 certificate found to match the criterion, otherwise <see langword="null"/>.
        /// </returns>
        public static X509Certificate2 GetCertificate(
            StoreLocation storeLocation,
            StoreName storeName,
            X509FindType findByType,
            string findByValue,
            bool ignoreTimeValid = false)
        {
            X509Store store = null;

            try
            {
                store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly);

                var certs = store.Certificates
                                 .Find(findByType, findByValue, false);

                if (!ignoreTimeValid)
                    certs = certs.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                if (certs.Count > 0)
                    return certs[0];
                else
                    return null;
            }
            finally
            {
                if (store != null)
                    store.Close();
            }
        }
    }
}
