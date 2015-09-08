using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NET45
using System;
#endif

namespace vm.Aspects.Security.Cryptography.Ciphers.Test
{
    [TestClass]
    public class NullHasherTest : GenericHasherTest<NullHasher>
    {
        public override IHasherAsync GetHasher()
        {
            return new NullHasher();
        }

        public override IHasherAsync GetHasher(int saltLength)
        {
            return new NullHasher();
        }

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (NullHasher)GetHasher();

            Assert.IsNotNull(target);

            // should do nothing:
            target.Dispose();
        }

#if NET45
        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<NullHasher>((NullHasher)GetHasher());

            GC.Collect();

            NullHasher collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        } 
#endif
        #endregion
    }
}
