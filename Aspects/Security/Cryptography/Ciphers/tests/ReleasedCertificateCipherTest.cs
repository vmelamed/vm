using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for DuplicateGenericTest
    /// </summary>
    [TestClass]
    public class ReleasedCertificateCipherTest : ClonedLightCipherTest
    {
        const string _keyFileName = "resetToDuplicate.key";

        protected override EncryptedKeyCipher GetCipher(bool base64 = false)
        {
            var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), _keyFileName)
            {
                Base64Encoded = base64,
            };

            cipher.ReleaseCertificate();
            return cipher;
        }

        [TestMethod]
        public override void DuplicateCanDecryptFromOriginal()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), _keyFileName))
            {
                cipher.ExportSymmetricKey();

                var encrypted = cipher.Encrypt(expected);

                cipher.ReleaseCertificate();

                var decrypted = cipher.Decrypt<string>(encrypted);

                Assert.AreEqual(expected, decrypted);
            }
        }

        [TestMethod]
        public override void OriginalCanDecryptFromDuplicate()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), _keyFileName))
            {
                cipher.ExportSymmetricKey();

                using (var dupe = cipher.CloneLightCipher())
                {
                    var encrypted = dupe.Encrypt(expected);

                    cipher.ReleaseCertificate();

                    var decrypted = cipher.Decrypt<string>(encrypted);

                    Assert.AreEqual(expected, decrypted);
                }
            }
        }
    }
}
