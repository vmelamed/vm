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
        const string _keyFileName = "releasedCertificateHash.key";

        public override IHasherTasks GetHasher() => new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), _keyFileName, hashAlgorithmName: null).ReleaseCertificate();

        public override IHasherTasks GetHasher(int saltLength) => GetHasher();

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            const string expected = "The quick fox jumps over the lazy dog.";
            var keyManagement = new EncryptedKeyCipher(CertificateFactory.GetEncryptingCertificate(), expected) as IKeyManagement;

            if (keyManagement.KeyLocation.EndsWith(expected, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        [TestMethod]
        public void OriginalCanVerifyStripped()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var stripped = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), _keyFileName, hashAlgorithmName: null).ReleaseCertificate())
            using (var original = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), _keyFileName, hashAlgorithmName: null))
            {
                var hash = stripped.Hash(expected);

                Assert.IsTrue(original.TryVerifyHash(expected, hash));
            }
        }

        [TestMethod]
        public void StrippedCanVerifyOriginal()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var original = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), _keyFileName, hashAlgorithmName: null))
            {
                var hash = original.Hash(expected);

                var stripped = original.ReleaseCertificate();

                Assert.IsTrue(stripped.TryVerifyHash(expected, hash));
            }
        }

        [TestMethod]
        public void StrippedCanVerifyStripped()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var stripped = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), _keyFileName, hashAlgorithmName: null).ReleaseCertificate())
            using (var stripped2 = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), _keyFileName, hashAlgorithmName: null).ReleaseCertificate())
            {
                var hash = stripped.Hash(expected);

                Assert.IsTrue(stripped2.TryVerifyHash(expected, hash));
            }
        }

        [TestMethod]
        public void StrippedCanVerifyItsOwn()
        {
            const string expected = "The quick fox jumps over the lazy dog.";

            using (var stripped = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), _keyFileName, hashAlgorithmName: null).ReleaseCertificate())
            {
                var hash = stripped.Hash(expected);

                Assert.IsTrue(stripped.TryVerifyHash(expected, hash));
            }
        }
    }
}
