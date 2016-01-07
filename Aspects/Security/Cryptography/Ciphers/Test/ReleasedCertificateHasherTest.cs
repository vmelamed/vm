using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for DuplicateGenericTest
    /// </summary>
    [TestClass]
    public class ReleasedCertificateHasherTest : GenericHasherTest<KeyedHasher>
    {
        const string keyFileName = "releasedCertificateHash.key";

        public override IHasherAsync GetHasher()
        {
            return new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName).ReleaseCertificate();
        }

        public override IHasherAsync GetHasher(int saltLength)
        {
            return GetHasher();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            const string expected = "The quick fox jumps over the lazy dog.";
            var keyManagement = new EncryptedKeyCipher(CertificateFactory.GetEncryptingCertificate(), null, expected) as IKeyManagement;

            if (keyManagement.KeyLocation.EndsWith(expected, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        [TestMethod]
        public void OriginalCanVerifyStripped()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var stripped = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName).ReleaseCertificate())
            using (var original = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName))
            {
                var hash = stripped.Hash(expected);

                Assert.IsTrue(original.TryVerifyHash(expected, hash));
            }
        }

        [TestMethod]
        public void StrippedCanVerifyOriginal()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var original = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName))
            {
                var hash = original.Hash(expected);

                var stripped = ((KeyedHasher)original).ReleaseCertificate();

                Assert.IsTrue(stripped.TryVerifyHash(expected, hash));
            }
        }

        [TestMethod]
        public void StrippedCanVerifyStripped()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var stripped = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName).ReleaseCertificate())
            using (var stripped2 = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName).ReleaseCertificate())
            {
                var hash = stripped.Hash(expected);

                Assert.IsTrue(stripped2.TryVerifyHash(expected, hash));
            }
        }

        [TestMethod]
        public void StrippedCanVerifyItsOwn()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var stripped = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName).ReleaseCertificate())
            {
                var hash = stripped.Hash(expected);

                Assert.IsTrue(stripped.TryVerifyHash(expected, hash));
            }
        }
    }
}
