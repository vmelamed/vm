using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace vm.Aspects.Tests
{
    [TestClass]
    public class ResolveNameAttributeTests
    {
        [TestMethod]
        public void ResolveNameAttributeTest()
        {
            var target = new ResolveNameAttribute("test");

            Assert.AreEqual("test", target.Name);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void ResolveNameAttributeNullTest()
        {
            var target = new ResolveNameAttribute(null);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void ResolveNameAttributeEmptyTest()
        {
            var target = new ResolveNameAttribute(string.Empty);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void ResolveNameAttributeBlankTest()
        {
            var target = new ResolveNameAttribute(" ");
        }
    }
}
