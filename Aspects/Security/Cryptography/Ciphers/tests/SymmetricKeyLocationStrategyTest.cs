using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class SymmetricKeyLocationStrategyTest
    {
        [TestMethod]
        public void GetKeyLocationNonEmptyParamTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyLocationStrategy>();

            Assert.AreEqual("foo", target.GetKeyLocation("foo"));
        }

        [TestMethod]
        public void GetKeyLocationNullParamTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyLocationStrategy>();

            Assert.IsFalse(string.IsNullOrWhiteSpace(target.GetKeyLocation(null)));
        }

        [TestMethod]
        public void GetKeyLocationEmptyParamTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyLocationStrategy>();

            Assert.IsFalse(string.IsNullOrWhiteSpace(target.GetKeyLocation("")));
        }

        [TestMethod]
        public void GetKeyLocationBlankParamTest()
        {
            var target = DefaultServices.Resolver.GetInstance<IKeyLocationStrategy>();

            Assert.IsFalse(string.IsNullOrWhiteSpace(target.GetKeyLocation(" ")));
        }
    }
}
