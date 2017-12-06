using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class KeyedHasherTest : GenericHasherTest<Hasher>
    {
        const string keyFileName = "encryptedHashKey.key";

        static IHasherAsync GetHasherImpl() => new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, "encryptedHashKey.key");

        public override IHasherAsync GetHasher() => GetHasherImpl();

        public override IHasherAsync GetHasher(int saltLength) => GetHasherImpl();


        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var keyManagement = GetHasherImpl() as IKeyManagement;

            if (keyManagement.KeyLocation != null &&
                keyManagement.KeyLocation.EndsWith(keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (KeyedHasher)GetHasher();

            Assert.IsNotNull(target);

            using (target)
                Assert.IsFalse(target.IsDisposed);
            Assert.IsTrue(target.IsDisposed);

            // should do nothing:
            target.Dispose();
        }

        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<KeyedHasher>((KeyedHasher)GetHasher());

            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
        #endregion

        class InheritedHasher : KeyedHasher
        {
            public InheritedHasher()
                : base(CertificateFactory.GetDecryptingCertificate())
            {
            }

            public byte[] PublicFinalizeHashing(
                CryptoStream hashStream) => base.FinalizeHashing(hashStream);
        }

        InheritedHasher GetInheritedHasher() => new InheritedHasher();

        CryptoStream GetCryptoStream(
            InheritedHasher hasher) => new CryptoStream(
                                                TestUtilities.CreateNonWritableStream(),
                                                HashAlgorithm.Create(Algorithms.KeyedHash.HmacSha256),
                                                CryptoStreamMode.Read);

        CryptoStream GetCryptoStream2(
            InheritedHasher hasher) => new CryptoStream(
                                                new MemoryStream(new byte[10], true),
                                                HashAlgorithm.Create(Algorithms.KeyedHash.HmacSha256),
                                                CryptoStreamMode.Write);
    }
}
