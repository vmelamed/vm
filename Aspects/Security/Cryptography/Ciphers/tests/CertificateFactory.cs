using System;
using System.Security.Cryptography.X509Certificates;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    static class CertificateFactory
    {
        public static X509Certificate2 GetDecryptingCertificate() => GetCert(StoreName.My, "vm.EncryptionCipherUnitTest");

        public static X509Certificate2 GetEncryptingCertificate() => GetCert(StoreName.TrustedPublisher, "vm.EncryptionCipherUnitTest");

        public static X509Certificate2 GetSigningCertificate() => GetCert(StoreName.My, "vm.SignatureCipherUnitTest");

        public static X509Certificate2 GetSignVerifyCertificate() => GetCert(StoreName.TrustedPublisher, "vm.SignatureCipherUnitTest");

        public static X509Certificate2 GetDecryptingSha256Certificate() => GetCert(StoreName.My, "vm.Sha256EncryptionCipherUnitTest");

        public static X509Certificate2 GetEncryptingSha256Certificate() => GetCert(StoreName.TrustedPublisher, "vm.Sha256EncryptionCipherUnitTest");

        public static X509Certificate2 GetSigningSha256Certificate() => GetCert(StoreName.My, "vm.Sha256SignatureCipherUnitTest");

        public static X509Certificate2 GetSignVerifySha256Certificate() => GetCert(StoreName.TrustedPublisher, "vm.Sha256SignatureCipherUnitTest");

        public static X509Certificate2 GetNotSupportedHashCertificate()
        {
            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            var thumbprint = "742c3192e607e424eb4549542be1bbc53e6174e2"; // Stella
            //"‎22D5D8DF8F0231D18DF79DB7CF8A2D64C93F6C3A";   // Laura

            store.Open(OpenFlags.ReadOnly);
            try
            {
                var certs = store.Certificates
                                 .Find(X509FindType.FindByThumbprint, thumbprint, false);

                var cert = certs.Count > 0 ? certs[0] : null;

                if (cert == null)
                    throw new InvalidOperationException(
                                $"Could not find the certificate with the given thumb-print {thumbprint}. "+
                                 "Change it to another value specifying certificate with any hash algorithm but MD5 or SHA, e.g. MD2.");

                return cert;
            }
            finally
            {
                store.Close();
            }
        }

        static X509Certificate2 GetCert(
            StoreName storeName,
            string subject,
            bool ignoreTimeValid = false)
        {
            var store = new X509Store(storeName, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);
            try
            {
                var certs = store.Certificates
                                 .Find(X509FindType.FindBySubjectName, subject, false);

                if (!ignoreTimeValid)
                    certs = certs.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                var cert = certs.Count > 0 ? certs[0] : null;

                if (cert == null)
                    throw new InvalidOperationException("Could not find the certificate. Did you run the certificate creation scripts?");

                //Contract.Assume(cert!=null);

                return cert;
            }
            finally
            {
                store.Close();
            }
        }
    }
}
