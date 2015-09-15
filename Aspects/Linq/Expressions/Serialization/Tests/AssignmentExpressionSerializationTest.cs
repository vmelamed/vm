using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\Documents\\Expression.xsd")]
    [DeploymentItem("..\\..\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public class AssignmentExpressionSerializationTest
    {
        public TestContext TestContext
        { get; set; }

        void Test(Expression expression, string fileName, bool validate = true)
        {
            TestHelpers.TestSerializeExpression(TestContext, expression, fileName, validate);
        }

        [TestMethod]
        public void TestAssign()
        {
            var expression = Expression.Assign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\Assign.xml");
        }

        [TestMethod]
        public void TestAddAssign()
        {
            var expression = Expression.AddAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\AddAssign.xml");
        }

        [TestMethod]
        public void TestAddAssignChecked()
        {
            var expression = Expression.AddAssignChecked(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\AddAssignChecked.xml");
        }

        [TestMethod]
        public void TestSubtractAssign()
        {
            var expression = Expression.SubtractAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\SubtractAssign.xml");
        }

        [TestMethod]
        public void TestSubtractAssignChecked()
        {
            var expression = Expression.SubtractAssignChecked(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\SubtractAssignChecked.xml");
        }

        [TestMethod]
        public void TestMultiplyAssign()
        {
            var expression = Expression.MultiplyAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\MultiplyAssign.xml");
        }

        [TestMethod]
        public void TestDivideAssign()
        {
            var expression = Expression.DivideAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\DivideAssign.xml");
        }

        [TestMethod]
        public void TestModuloAssign()
        {
            var expression = Expression.ModuloAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\ModuloAssign.xml");
        }

        [TestMethod]
        public void TestMultiplyAssignChecked()
        {
            var expression = Expression.MultiplyAssignChecked(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\MultiplyAssignChecked.xml");
        }

        [TestMethod]
        public void TestAndAssign()
        {
            var expression = Expression.AndAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\AndAssign.xml");
        }

        [TestMethod]
        public void TestOrAssign()
        {
            var expression = Expression.OrAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\OrAssign.xml");
        }

        [TestMethod]
        public void TestXorAssign()
        {
            var expression = Expression.ExclusiveOrAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\XorAssign.xml");
        }

        [TestMethod]
        public void TestPowerAssign()
        {
            var expression = Expression.PowerAssign(
                                Expression.Parameter(typeof(double), "a"),
                                Expression.Parameter(typeof(double), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\PowerAssign.xml");
        }

        [TestMethod]
        public void LeftShiftAssign()
        {
            var expression = Expression.LeftShiftAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\LeftShiftAssign.xml");
        }

        [TestMethod]
        public void RightShiftAssign()
        {
            var expression = Expression.RightShiftAssign(
                                Expression.Parameter(typeof(int), "a"),
                                Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\Assignments\\RightShiftAssign.xml");
        }
    }
}
