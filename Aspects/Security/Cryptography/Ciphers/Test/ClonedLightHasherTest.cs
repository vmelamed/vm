using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    /// <summary>
    /// Summary description for DuplicateGenericTest
    /// </summary>
    [TestClass]
    public class ClonedLightHasherTest : GenericHasherTest<KeyedHasher>
    {
        const string keyFileName = "duplicateHash.key";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        //public TestContext TestContext { get; set; }

        public override IHasherAsync GetHasher()
        {
            using (var cipher = new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, keyFileName))
            {
                return cipher.CloneLightHasher() as IHasherAsync;
            }
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


    }
}
