using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\Documents\\Expression.xsd")]
    [DeploymentItem("..\\..\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public class ChangeByOneSerializerTest
    {
        public TestContext TestContext
        { get; set; }

        void Test(Expression expression, string fileName)
        {
            TestHelpers.TestSerializeExpression(TestContext, expression, fileName);
        }

        [TestMethod]
        public void TestIncrement()
        {
            var expression = Expression.Increment(
                                Expression.Parameter(typeof(int), "a"));

            Test(
                expression,
                "TestFiles\\ChangeByOne\\Increment.xml");
        }

        [TestMethod]
        public void TestDecrement()
        {
            var expression = Expression.Decrement(
                                Expression.Parameter(typeof(int), "a"));

            Test(
                expression,
                "TestFiles\\ChangeByOne\\Decrement.xml");
        }

        [TestMethod]
        public void TestPreIncrement()
        {
            var expression = Expression.PreIncrementAssign(
                                Expression.Parameter(typeof(int), "a"));

            Test(
                expression,
                "TestFiles\\ChangeByOne\\PreIncrement.xml");
        }

        [TestMethod]
        public void TestPreDecrement()
        {
            var expression = Expression.PreDecrementAssign(
                                Expression.Parameter(typeof(int), "a"));

            Test(
                expression,
                "TestFiles\\ChangeByOne\\PreDecrement.xml");
        }

        [TestMethod]
        public void TestPostIncrement()
        {
            var expression = Expression.PostIncrementAssign(
                                Expression.Parameter(typeof(int), "a"));

            Test(
                expression,
                "TestFiles\\ChangeByOne\\PostIncrement.xml");
        }

        [TestMethod]
        public void TestPostDecrement()
        {
            var expression = Expression.PostDecrementAssign(
                                Expression.Parameter(typeof(int), "a"));

            Test(
                expression,
                "TestFiles\\ChangeByOne\\PostDecrement.xml");
        }
    }
}
