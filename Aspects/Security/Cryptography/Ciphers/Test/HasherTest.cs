using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class HasherTest : GenericHasherTest<Hasher>
    {
        public override IHasherTasks GetHasher() => new Hasher(Hasher.DefaultSaltLength);
        public override IHasherTasks GetHasher(int saltLength) => new Hasher(saltLength);

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (Hasher)GetHasher();

            Assert.IsNotNull(target);

            using (target as IDisposable)
                Assert.IsFalse(target.IsDisposed);
            Assert.IsTrue(target.IsDisposed);

            // should do nothing:
            target.Dispose();
        }

        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<Hasher>((Hasher)GetHasher());

            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
        #endregion

        class InheritedHasher : Hasher
        {
            public byte[] PublicWriteSalt(
                CryptoStream hashStream,
                byte[] salt) => base.WriteSalt(hashStream, salt);

            public async Task<byte[]> PublicWriteSaltAsync(
                    CryptoStream hashStream,
                    byte[] salt) => await base.WriteSaltAsync(hashStream, salt);

            public byte[] PublicFinalizeHashing(
                CryptoStream hashStream,
                byte[] salt) => base.FinalizeHashing(hashStream, salt);
        }

        InheritedHasher GetInheritedHasher() => new InheritedHasher() { SaltLength = 8, };

        CryptoStream GetCryptoStream(
            InheritedHasher hasher) => new CryptoStream(
                                                TestUtilities.CreateNonWritableStream(),
                                                HashAlgorithm.Create(Algorithms.Hash.Sha256),
                                                CryptoStreamMode.Read);

        CryptoStream GetCryptoStream2(
            InheritedHasher hasher) => new CryptoStream(
                                                new MemoryStream(new byte[10], true),
                                                HashAlgorithm.Create(Algorithms.Hash.Sha256),
                                                CryptoStreamMode.Write);

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteSaltNonWritableStreamTest()
        {
            using (var hasher = GetInheritedHasher())
                hasher.PublicWriteSalt(
                            GetCryptoStream(hasher),
                            new byte[8]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteSaltNullStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var hasher = GetInheritedHasher())
                    hasher.PublicWriteSaltAsync(null, new byte[8]).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteSaltNonWritableStreamAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                using (var hasher = GetInheritedHasher())
                    hasher.PublicWriteSaltAsync(
                                GetCryptoStream(hasher),
                                new byte[8]).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FinalizeHashingNonWritableCryptoStreamTest()
        {
            using (var hasher = GetInheritedHasher())
                hasher.PublicFinalizeHashing(GetCryptoStream(hasher), new byte[8]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinalizeHashingNullSaltTest()
        {
            using (var hasher = GetInheritedHasher())
                hasher.PublicFinalizeHashing(GetCryptoStream2(hasher), null);
        }
    }
}
