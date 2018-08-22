using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class NullHasherTest : GenericHasherTest<NullHasher>
    {
        public override IHasherTasks GetHasher() => new NullHasher();

        public override IHasherTasks GetHasher(int saltLength) => new NullHasher();

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (NullHasher)GetHasher();

            Assert.IsNotNull(target);

            // should do nothing:
            target.Dispose();
        }

        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<NullHasher>((NullHasher)GetHasher());

            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
        #endregion
    }
}
