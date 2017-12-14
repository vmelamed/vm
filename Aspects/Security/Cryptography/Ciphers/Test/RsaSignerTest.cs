using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class RsaSignerTest : GenericHasherTest<RsaSigner>
    {
        public override IHasherAsync GetHasher() => new RsaSigner(CertificateFactory.GetSigningCertificate());

        public override IHasherAsync GetHasher(int saultLength) => GetHasher();

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (RsaSigner)GetHasher();

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
            var target = new WeakReference<RsaSigner>((RsaSigner)GetHasher());

            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullCertTest()
        {
            var target = new RsaSigner(null);
        }
    }
}
