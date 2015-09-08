using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace vm.Aspects.Tests
{
    class Utilities
    {
        internal static void ResetContainer()
        {
            DIContainer.Reset();
            Assert.IsNotNull(DIContainer.Root);
            Assert.IsFalse(DIContainer.IsInitialized);
        }

        internal static void ReinitContainer()
        {
            ResetContainer();
            DIContainer.Initialize();
            Assert.IsNotNull(DIContainer.Root);
            Assert.IsTrue(DIContainer.IsInitialized);
        }

    }
}
