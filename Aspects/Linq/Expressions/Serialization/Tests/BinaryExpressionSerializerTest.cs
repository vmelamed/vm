using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\src\\schemas\\Expression.xsd")]
    [DeploymentItem("..\\..\\..\\src\\schemas\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public class BinaryExpressionSerializerTest
    {
        public TestContext TestContext
        { get; set; }

        void Test(Expression expression, string fileName, bool validate = true)
        {
            TestHelpers.TestSerializeExpression(TestContext, expression, fileName, validate);
        }

        [TestMethod]
        public void TestAdd()
        {
            Expression<Func<int, int, int>> expression = (a, b) => a+b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaAdd.xml");
        }

        [TestMethod]
        public void TestAddChecked()
        {
            Expression<Func<int, int, int>> expression = (a, b) => checked(a+b);

            Test(
                expression,
                "TestFiles\\Binary\\LambdaAddChecked.xml");
        }

        [TestMethod]
        public void TestSubtract()
        {
            Expression<Func<int, int, int>> expression = (a, b) => a - b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaSubtract.xml");
        }

        [TestMethod]
        public void TestSubtractChecked()
        {
            Expression<Func<int, int, int>> expression = (a, b) => checked(a - b);

            Test(
                expression,
                "TestFiles\\Binary\\LambdaSubtractChecked.xml");
        }

        [TestMethod]
        public void TestAnd()
        {
            Expression<Func<uint, uint, uint>> expression = (a, b) => a & b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaAnd.xml");
        }

        [TestMethod]
        public void TestAndAlso()
        {
            Expression<Func<bool, bool, bool>> expression = (a, b) => a && b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaAndAlso.xml");
        }

        [TestMethod]
        public void TestOr()
        {
            Expression<Func<uint, uint, uint>> expression = (a, b) => a | b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaOr.xml");
        }

        [TestMethod]
        public void TestOrElse()
        {
            Expression<Func<bool, bool, bool>> expression = (a, b) => a || b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaOrElse.xml");
        }

        [TestMethod]
        public void TestArrayIndex()
        {
            Expression<Func<object[], int, object>> expression = (a, x) => a[x];

            Test(
                expression,
                "TestFiles\\Binary\\LambdaArrayIndex.xml");
        }

        [TestMethod]
        public void TestDivide()
        {
            Expression<Func<int, int, int>> expression = (a, b) => a / b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaDivide.xml");
        }

        [TestMethod]
        public void TestPower()
        {
            var expression = Expression.Lambda(
                 Expression.Power(
                     Expression.Parameter(typeof(double), "a"),
                     Expression.Parameter(typeof(double), "b")),
                 new ParameterExpression[]
                {
                    Expression.Parameter(typeof(double), "a"),
                    Expression.Parameter(typeof(double), "b"),
                });

            Test(
                expression,
                "TestFiles\\Binary\\LambdaPower.xml");
        }

        [TestMethod]
        public void TestModulo()
        {
            Expression<Func<int, int, int>> expression = (a, b) => a % b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaModulo.xml");
        }

        [TestMethod]
        public void TestEqual()
        {
            Expression<Func<int, int, bool>> expression = (a, b) => a==b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaEqual.xml");
        }

        [TestMethod]
        public void TestNotEqual()
        {
            Expression<Func<int, int, bool>> expression = (a, b) => a != b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaNotEqual.xml");
        }

        [TestMethod]
        public void TestMultiply()
        {
            Expression<Func<int, int, int>> expression = (a, b) => a*b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaMultiply.xml");
        }

        [TestMethod]
        public void TestMultiplyChecked()
        {
            Expression<Func<int, int, int>> expression = (a, b) => checked(a * b);

            Test(
                expression,
                "TestFiles\\Binary\\LambdaMultiplyChecked.xml");
        }

        [TestMethod]
        public void TestGreaterThan()
        {
            Expression<Func<int, int, bool>> expression = (a, b) => a > b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaGreaterThan.xml");
        }

        [TestMethod]
        public void TestGreaterThanOrEqual()
        {
            Expression<Func<int, int, bool>> expression = (a, b) => a >= b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaGreaterThanOrEqual.xml");
        }

        [TestMethod]
        public void TestLessThan()
        {
            Expression<Func<int, int, bool>> expression = (a, b) => a < b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaLessThan.xml");
        }

        [TestMethod]
        public void TestLessThanOrEqual()
        {
            Expression<Func<int, int, bool>> expression = (a, b) => a <= b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaLessThanOrEqual.xml");
        }

        [TestMethod]
        public void TestLeftShift()
        {
            Expression<Func<uint, int, uint>> expression = (a, b) => a << b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaLeftShift.xml");
        }

        [TestMethod]
        public void TestRightShift()
        {
            Expression<Func<uint, int, uint>> expression = (a, b) => a >> b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaRightShift.xml");
        }

        [TestMethod]
        public void TestCoalesce()
        {
            Expression<Func<int?, int, int>> expression = (a, b) => a ?? b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaCoalesce.xml");
        }

        [TestMethod]
        public void TestExclusiveOr()
        {
            Expression<Func<uint, uint, uint>> expression = (a, b) => a ^ b;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaExclusiveOr.xml");
        }

        [TestMethod]
        public void TestLambdaAddMultiply()
        {
            Expression<Func<int, int, int>> expression = (a, b) => a + b * 71;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaAddMultiply.xml");
        }

        [TestMethod]
        public void TestLambdaMultiplyAdd()
        {
            Expression<Func<int, int, int>> expression = (a, b) => (a + b) * 71;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaMultiplyAdd.xml");
        }

        [TestMethod]
        public void TestLambdaAddAdd()
        {
            Expression<Func<int, int, int>> expression = (a, b) => a + b + 77;

            Test(
                expression,
                "TestFiles\\Binary\\LambdaAddAdd.xml");
        }

    }
}
