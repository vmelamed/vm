using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class KeyedHasherTest : GenericHasherTest<Hasher>
    {
        const string keyFileName = "encryptedHashKey.key";

        static IHasherAsync GetHasherImpl()
        {
            return new KeyedHasher(CertificateFactory.GetDecryptingCertificate(), null, "encryptedHashKey.key");
        }

        public override IHasherAsync GetHasher()
        {
            return GetHasherImpl();
        }

        public override IHasherAsync GetHasher(int saltLength)
        {
            return GetHasherImpl();
        }


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

            KeyedHasher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        }
        #endregion

        class InheritedHasher : KeyedHasher
        {
            public InheritedHasher()
                : base(CertificateFactory.GetDecryptingCertificate())
            {
            }

            public byte[] PublicFinalizeHashing(
                CryptoStream hashStream,
                HashAlgorithm hashAlgorithm)
            {
                return base.FinalizeHashing(hashStream, hashAlgorithm);
            }
        }

        InheritedHasher GetInheritedHasher()
        {
            return new InheritedHasher();
        }

        CryptoStream GetCryptoStream(
            InheritedHasher hasher)
        {
            return new CryptoStream(
                            TestUtilities.CreateNonWritableStream(),
                            HashAlgorithm.Create(Algorithms.KeyedHash.HmacSha256),
                            CryptoStreamMode.Read);
        }

        CryptoStream GetCryptoStream2(
            InheritedHasher hasher)
        {
            return new CryptoStream(
                            new MemoryStream(new byte[10], true),
                            HashAlgorithm.Create(Algorithms.KeyedHash.HmacSha256),
                            CryptoStreamMode.Write);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FinalizeHashingNonWritableCryptoStreamTest()
        {
            using (var hasher = GetInheritedHasher())
                hasher.PublicFinalizeHashing(GetCryptoStream(hasher), HashAlgorithm.Create(Algorithms.KeyedHash.HmacSha256));
        }
    }
}
