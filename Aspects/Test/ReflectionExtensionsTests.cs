using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    [TestClass]
    public class ReflectionExtensionsTests
    {
        interface IFoo
        {
            int Property1 { get; set; }
            int Property2 { get; set; }
        }

        class Foo : IFoo
        {
            public int NonVirtual { get; set; }

            public virtual int Virtual { get; set; }

            public int Property1 { get; set; }
            public virtual int Property2 { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPiTest()
        {
            PropertyInfo pi = null;

            Assert.IsFalse(pi.IsVirtual().GetValueOrDefault());
        }

        [TestMethod]
        public void NonVirtualPropertyTest()
        {
            var pi = typeof(Foo).GetProperty("NonVirtual");

            Assert.IsTrue(pi.IsVirtual().HasValue);
            Assert.IsFalse(pi.IsVirtual().Value);
        }

        [TestMethod]
        public void VirtualPropertyTest()
        {
            var pi = typeof(Foo).GetProperty("Virtual");

            Assert.IsTrue(pi.IsVirtual().HasValue);
            Assert.IsTrue(pi.IsVirtual().Value);
        }

        [TestMethod]
        public void InterfacePropertyTest()
        {
            var pi = typeof(Foo).GetProperty("Property1");

            Assert.IsTrue(pi.IsVirtual().HasValue);
            Assert.IsTrue(pi.IsVirtual().Value);
        }

        [TestMethod]
        public void InterfaceVirtualPropertyTest()
        {
            var pi = typeof(Foo).GetProperty("Property2");

            Assert.IsTrue(pi.IsVirtual().HasValue);
            Assert.IsTrue(pi.IsVirtual().Value);
        }
    }
}
