using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Exceptions;
using vm.Aspects.Validation;

namespace vm.Aspects.Model.Tests
{
    [TestClass]
    public class ValueTests
    {
        [TestMethod]
        public void ValidationTest()
        {
            var target = new TestValue
            {
                Name = "gogo",
            };

            Assert.IsTrue(target.IsValid());
            Assert.IsNotNull(target.ConfirmValid());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidObjectException))]
        public void InvalidTest()
        {
            var target = new TestValue
            {
                Name = "0123456789012345678901234567890",
            };

            Assert.IsFalse(target.IsValid());
            target.ConfirmValid();
        }
    }
}
