using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    [TestClass()]
    public class RegistrationLookupTests : IdentityTester<RegistrationLookup>
    {
        public RegistrationLookupTests()
        {
            Initialize(
                new RegistrationLookup(typeof(string), "abc"),
                new RegistrationLookup(typeof(string), "abc"),
                new RegistrationLookup(typeof(string), "abc"),
                new RegistrationLookup(typeof(object), "xyz"),
                rl => Thread.Sleep(50));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationLookupNullTypeTest()
        {
            new RegistrationLookup(null);
        }

        [TestMethod]
        public void RegistrationLookupNullNameTest()
        {
            var target = new RegistrationLookup(typeof(string));

            Assert.AreEqual(typeof(string), target.RegisteredType);
            Assert.AreEqual(string.Empty, target.Name);
        }

        [TestMethod]
        public void RegistrationLookupEmptyNameTest()
        {
            var target = new RegistrationLookup(typeof(string), "");

            Assert.AreEqual(typeof(string), target.RegisteredType);
            Assert.AreEqual(string.Empty, target.Name);
        }

        [TestMethod]
        public void RegistrationLookupTest()
        {
            var target = new RegistrationLookup(typeof(string), "abc");

            Assert.AreEqual(typeof(string), target.RegisteredType);
            Assert.AreEqual("abc", target.Name);
        }

        [TestMethod]
        public void RegistrationLookupOperatorEqualsTest()
        {
            var target1 = new RegistrationLookup(typeof(string), "abc");
            var target2 = new RegistrationLookup(typeof(string), "abc");
            var target3 = new RegistrationLookup(typeof(string), "abc");
            var target4 = new RegistrationLookup(typeof(object), "xyz");

            Assert.IsTrue(!(target1==(RegistrationLookup)null), "target1 must not be equal to null.");
            Assert.IsTrue(!((RegistrationLookup)null==target1), "target1 must not be equal to obj1.");

            // reflexitivity
            var t = target1;

            Assert.IsTrue(target1==t, "The operator == must be reflexive.");
            Assert.IsFalse(target1!=t, "The operator == must be reflexive.");

            // symmetricity
            Assert.AreEqual(target1==target2, target2==target1, "The operator == must be symmetric.");
            Assert.AreEqual(target1!=target4, target4!=target1, "The operator != must be symmetric.");

            // transityvity
            Assert.IsTrue(target1==target2 && target2==target3 && target3==target1, "The operator == must be transitive.");
            Assert.IsTrue(target1==target2 && target1!=target4 && target2!=target4, "The operator != must be transitive.");
        }
    }
}
