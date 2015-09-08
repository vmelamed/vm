using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Test
{
    [TestClass]
    public class PasswordHasherTest : GenericHasherTest<PasswordHasher>
    {
        public override IHasherAsync GetHasher()
        {
            return new PasswordHasher(
                        PasswordDerivationConstants.MinNumberOfIterations,
                        PasswordDerivationConstants.DefaultHashLength,
                        PasswordDerivationConstants.DefaultSaltLength);
        }
        public override IHasherAsync GetHasher(
            int saultLength)
        {
            return new PasswordHasher(
                        PasswordDerivationConstants.MinNumberOfIterations,
                        PasswordDerivationConstants.DefaultHashLength,
                        saultLength);
        }

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (PasswordHasher)GetHasher();

            Assert.IsNotNull(target);

            using (target as IDisposable)
                Assert.IsFalse(target.IsDisposed);
            Assert.IsTrue(target.IsDisposed);

            // should do nothing:
            target.Dispose();
        }

#if NET45
        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<PasswordHasher>((PasswordHasher)GetHasher());

            GC.Collect();

            PasswordHasher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        } 
#endif
        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateWithBadNumberOfIterations()
        {
            new PasswordHasher(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateWithBadNumberOfIterations2()
        {
            new PasswordHasher(PasswordDerivationConstants.MinNumberOfIterations-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateWithBadHashLength()
        {
            new PasswordHasher(PasswordDerivationConstants.DefaultNumberOfIterations, PasswordDerivationConstants.MinHashLength-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateWithBadSaltLength()
        {
            new PasswordHasher(PasswordDerivationConstants.DefaultNumberOfIterations, PasswordDerivationConstants.MinHashLength, PasswordDerivationConstants.MinSaltLength-1);
        }
    }
}
