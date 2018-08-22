using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class KeyedHasherTest : GenericHasherTest<Hasher>
    {
        const string _keyFileName = "encryptedHashKey.key";

        static IHasherTasks GetHasherImpl() => new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), "encryptedHashKey.key");

        public override IHasherTasks GetHasher() => GetHasherImpl();

        public override IHasherTasks GetHasher(int saltLength) => GetHasherImpl();


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
                keyManagement.KeyLocation.EndsWith(_keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
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
