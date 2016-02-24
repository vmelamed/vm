using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vm.Aspects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace vm.Aspects.Tests
{
    [TestClass]
    public class LambdaExpressionExtensionsTests
    {
        class A
        {
            public int PropertyA { get; set; }
            public int Method()  => 1;
            public int Field = 1;
#pragma warning disable 67
            public event Action Event;
#pragma warning restore 67
        }

        [TestMethod]
        public void GetMemberNameTest()
        {
            Expression<Func<A, int>> target = a => a.PropertyA;

            Assert.AreEqual(target.GetMemberName(), "PropertyA");
        }

        [TestMethod]
        public void GetMemberNameTest1()
        {
            Expression<Func<A, int>> target = a => -a.PropertyA;

            Assert.AreEqual(target.GetMemberName(), "PropertyA");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void GetMemberNameNullLambdaTest()
        {
            Expression<Func<A, int>> target = null;

            target.GetMemberName();
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void GetMemberNameMethodTest()
        {
            Expression<Func<A, int>> target = a => a.Method();

            target.GetMemberName();
        }

        [TestMethod]
        public void GetMemberNameFieldTest()
        {
            Expression<Func<A, int>> target = a => a.Field;

            target.GetMemberName();
        }

        [TestMethod]
        public void GetMemberNameEventTest()
        {
            Expression<Func<A, int>> target = a => a.Field;

            target.GetMemberName();
        }
    }
}
