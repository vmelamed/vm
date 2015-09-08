using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Test
{
    [TestClass]
    public class SymmetricKeyLocationStrategyTest
    {
        [TestMethod]
        public void GetKeyLocationNonEmptyParamTest()
        {
            var target = new KeyLocationStrategy();

            Assert.AreEqual("foo", target.GetKeyLocation("foo"));
        }

        [TestMethod]
        public void GetKeyLocationNullParamTest()
        {
            var target = new KeyLocationStrategy();

            Assert.IsFalse(string.IsNullOrWhiteSpace(target.GetKeyLocation(null)));
        }

        [TestMethod]
        public void GetKeyLocationEmptyParamTest()
        {
            var target = new KeyLocationStrategy();

            Assert.IsFalse(string.IsNullOrWhiteSpace(target.GetKeyLocation("")));
        }

        [TestMethod]
        public void GetKeyLocationBlankParamTest()
        {
            var target = new KeyLocationStrategy();

            Assert.IsFalse(string.IsNullOrWhiteSpace(target.GetKeyLocation(" ")));
        }
    }
}
