using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for DuplicateGenericTest
    /// </summary>
    [TestClass]
    public class ResetToDuplicateTest : DuplicateTest
    {
        const string keyFileName = "resetToDuplicate.key";

        protected override EncryptedKeyCipher GetCipher(bool base64 = false)
        {
            var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName);

            cipher.Base64Encoded = base64;
            cipher.ExportSymmetricKey();

            cipher.ResetAsymmetricKeys();
            return cipher;
        }


        [TestMethod]
        public override void DuplicateCanDecryptFromOriginal()
        {
            using (var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName))
            {
                cipher.ExportSymmetricKey();

                var encrypted = cipher.Encrypt(keyFileName);

                cipher.ResetAsymmetricKeys();

                var decrypted = cipher.Decrypt<string>(encrypted);

                Assert.AreEqual(keyFileName, decrypted);
            }
        }

        [TestMethod]
        public override void OriginalCanDecryptFromDuplicate()
        {
            using (var cipher = new EncryptedKeyCipher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName))
            {
                cipher.ExportSymmetricKey();

                using (var dupe = cipher.Duplicate())
                {
                    var encrypted = dupe.Encrypt(keyFileName);

                    cipher.ResetAsymmetricKeys();

                    var decrypted = cipher.Decrypt<string>(encrypted);

                    Assert.AreEqual(keyFileName, decrypted);
                }
            }
        }
    }
}
