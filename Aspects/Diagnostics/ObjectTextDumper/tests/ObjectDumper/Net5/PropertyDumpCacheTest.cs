using System.ComponentModel.DataAnnotations;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics.Implementation;

namespace vm.Aspects.Diagnostics.ObjectDumper.Tests
{
    [TestClass]
    public class PropertyDumpCacheTest
    {
        class Test1
        {
            public int Property { get; set; }
        }

        [TestMethod]
        public void GetPropertyDumpAttribute1()
        {
            var pi = typeof(Test1).GetProperty("Property");

            var dumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(pi);

            Assert.AreEqual(DumpAttribute.Default, dumpAttribute);
        }

        class Test2
        {
            [Dump(0)]
            public int Property { get; set; }
        }

        [TestMethod]
        public void GetPropertyDumpAttribute2()
        {
            var pi = typeof(Test2).GetProperty("Property");

            var dumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(pi);

            Assert.AreEqual(new DumpAttribute(0), dumpAttribute);
        }

        [MetadataType(typeof(Test3Meta))]
        class Test3
        {
            public int Property { get; set; }
        }

        class Test3Meta
        {
            [Dump(0)]
            public object Property { get; set; }
        }

        [TestMethod]
        public void GetPropertyDumpAttribute3()
        {
            var pi = typeof(Test3).GetProperty("Property");

            var dumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(pi, typeof(Test3Meta));
            var dumpAttribute1 = PropertyDumpResolver.GetPropertyDumpAttribute(pi, typeof(Test3Meta));

            Assert.AreEqual(new DumpAttribute(0), dumpAttribute);
            Assert.AreSame(dumpAttribute, dumpAttribute1);
        }
    }
}
