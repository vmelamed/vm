using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics.Implementation;

namespace vm.Aspects.Diagnostics.Tests.ObjectDumper
{
    [TestClass]
    public class PropertyDumpOrderComparerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PropertyOrderComparer_Nulls()
        {
            var c = new MemberInfoComparer().SetMetadata(null);

            c.Compare(null, null);
        }

        class Test1
        {
            public int Property1 { get; set; }
            public int Property2 { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PropertyOrderComparer_Null1()
        {
            var c = new MemberInfoComparer();

            c.Compare(null, typeof(Test1).GetProperty("Property1"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PropertyOrderComparer_Null2()
        {
            var c = new MemberInfoComparer();

            c.Compare(typeof(Test1).GetProperty("Property2"), null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PropertyOrderComparer_Unrelated()
        {
            var c = new MemberInfoComparer();

            c.Compare(typeof(Test1).GetProperty("Property1"), typeof(Test2).GetProperty("Property1"));
        }

        [TestMethod]
        public void PropertyOrderComparer_Alpha()
        {
            var c = new MemberInfoComparer();
            var r = c.Compare(typeof(Test1).GetProperty("Property1"), typeof(Test1).GetProperty("Property2"));

            Assert.IsTrue(r < 0);

            r = c.Compare(typeof(Test1).GetProperty("Property2"), typeof(Test1).GetProperty("Property1"));

            Assert.IsTrue(r > 0);
        }

        class Test2
        {
            [Dump(0)]
            public int Property1 { get; set; }

            public int Property2 { get; set; }
        }

        [TestMethod]
        public void PropertyOrderComparer_Order2()
        {
            var c = new MemberInfoComparer();
            var r = c.Compare(typeof(Test2).GetProperty("Property1"), typeof(Test2).GetProperty("Property2"));

            Assert.IsTrue(r < 0);

            r = c.Compare(typeof(Test2).GetProperty("Property2"), typeof(Test2).GetProperty("Property1"));

            Assert.IsTrue(r > 0);
        }

        class Test3
        {
            [Dump(0)]
            public int Property1 { get; set; }

            [Dump(1)]
            public int Property2 { get; set; }
        }

        [TestMethod]
        public void PropertyOrderComparer_Order3()
        {
            var c = new MemberInfoComparer();
            var r = c.Compare(typeof(Test3).GetProperty("Property1"), typeof(Test3).GetProperty("Property2"));

            Assert.IsTrue(r < 0);

            r = c.Compare(typeof(Test3).GetProperty("Property2"), typeof(Test3).GetProperty("Property1"));

            Assert.IsTrue(r > 0);
        }

        class Test4
        {
            [Dump(0)]
            public int Property1 { get; set; }

            [Dump(-1)]
            public int Property2 { get; set; }
        }

        [TestMethod]
        public void PropertyOrderComparer_Order4()
        {
            var c = new MemberInfoComparer();
            var r = c.Compare(typeof(Test4).GetProperty("Property1"), typeof(Test4).GetProperty("Property2"));

            Assert.IsTrue(r < 0);

            r = c.Compare(typeof(Test4).GetProperty("Property2"), typeof(Test4).GetProperty("Property1"));

            Assert.IsTrue(r > 0);
        }

        class Test5 : Test4
        {
            [Dump(0)]
            public int Property3 { get; set; }

            [Dump(-1)]
            public int Property4 { get; set; }
        }

        [TestMethod]
        public void PropertyOrderComparer_Order5()
        {
            var c = new MemberInfoComparer();
            int r;

            r = c.Compare(typeof(Test4).GetProperty("Property1"), typeof(Test5).GetProperty("Property3"));

            Assert.IsTrue(r > 0);

            r = c.Compare(typeof(Test5).GetProperty("Property3"), typeof(Test4).GetProperty("Property1"));

            Assert.IsTrue(r < 0);



            r = c.Compare(typeof(Test5).GetProperty("Property3"), typeof(Test4).GetProperty("Property2"));

            Assert.IsTrue(r < 0);

            r = c.Compare(typeof(Test4).GetProperty("Property2"), typeof(Test5).GetProperty("Property3"));

            Assert.IsTrue(r > 0);



            r = c.Compare(typeof(Test5).GetProperty("Property4"), typeof(Test4).GetProperty("Property1"));

            Assert.IsTrue(r > 0);

            r = c.Compare(typeof(Test4).GetProperty("Property1"), typeof(Test5).GetProperty("Property4"));

            Assert.IsTrue(r < 0);



            r = c.Compare(typeof(Test5).GetProperty("Property4"), typeof(Test4).GetProperty("Property2"));

            Assert.IsTrue(r > 0);

            r = c.Compare(typeof(Test4).GetProperty("Property2"), typeof(Test5).GetProperty("Property4"));

            Assert.IsTrue(r < 0);
        }
    }
}
