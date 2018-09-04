using System;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\src\\schemas\\Expression.xsd")]
    [DeploymentItem("..\\..\\..\\src\\schemas\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\..\\src\\schemas\\DataContract.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public class UnaryExpressionSerializerTest
    {
        public TestContext TestContext
        { get; set; }

        void Test(Expression expression, string fileName, bool validate = true)
        {
            TestHelpers.TestSerializeExpression(TestContext, expression, fileName, validate);
        }

        [TestMethod]
        public void TestArrayLength()
        {
            Expression<Func<int[], int>> expression = a => a.Length;

            Test(
                expression,
                "TestFiles\\Unary\\LambdaArrayLength.xml");
        }

        [TestMethod]
        public void TestLambdaNegateParam()
        {
            Expression<Func<int, int>> expression = i => -i;
            Test(
                expression,
                "TestFiles\\Unary\\LambdaNegateParam.xml");
        }

        [TestMethod]
        public void TestLambdaNegateOverloaded()
        {
            Expression<Func<A, A>> expression = a => -a;

            Test(
                expression,
                "TestFiles\\Unary\\LambdaNegateOverloaded.xml");
        }


        [TestMethod]
        public void TestLambdaNegateChecked()
        {
            Expression<Func<int, int>> expression = i => checked(-i);

            Test(
                expression,
                "TestFiles\\Unary\\LambdaNegateChecked.xml");
        }

        [TestMethod]
        public void TestLambdaNot()
        {
            Expression<Func<bool, bool>> expression = i => !i;

            Test(
                expression,
                "TestFiles\\Unary\\LambdaNot.xml");
        }

        [TestMethod]
        public void TestLambdaMethodNot()
        {
            Expression<Func<B, B>> expression = b => !b;

            Test(
                expression,
                "TestFiles\\Unary\\LambdaMethodNot.xml");
        }

        [TestMethod]
        public void TestLambdaTypeAs()
        {
            Expression<Func<object, int?>> expression = m => m as int?;

            Test(
                expression,
                "TestFiles\\Unary\\LambdaTypeAs.xml");
        }

        [TestMethod]
        public void TestLambdaConvert()
        {
            var expression = Expression.Lambda<Func<double, int>>(
                                Expression.Convert(
                                    Expression.Parameter(typeof(double), "m"), typeof(int)),
                                Expression.Parameter(typeof(double), "m"));

            Test(
                expression,
                "TestFiles\\Unary\\LambdaConvert.xml");
        }

        [TestMethod]
        public void TestQuote()
        {
            Expression<Func<object, int?>> expressionQ = m => m as int?;
            var expression = Expression.Quote(expressionQ);

            Test(
                expression,
                "TestFiles\\Unary\\Quote.xml");
        }

        [TestMethod]
        public void TestTypeAs()
        {
            var expression = Expression.TypeAs(
                                Expression.Constant(5), typeof(double?));

            Test(
                expression,
                "TestFiles\\Unary\\TypeAs.xml");
        }

        [TestMethod]
        public void TestUnaryPlus()
        {
            var expression = Expression.UnaryPlus(
                                Expression.Constant(new A()));

            Test(
                expression,
                "TestFiles\\Unary\\UnaryPlus.xml",
                false);
        }

        [TestMethod]
        public void TestOnesComplement()
        {
            Expression expression = Expression.OnesComplement(
                                        Expression.Constant((uint)5));

            Test(
                expression,
                "TestFiles\\Unary\\OnesComplement.xml",
                false);
        }

        [TestMethod]
        public void TestThrow()
        {
            Expression expression = Expression.Throw(
                                        Expression.New(typeof(Exception).GetConstructor(Type.EmptyTypes)));

            Test(
                expression,
                "TestFiles\\Unary\\Throw.xml",
                false);
        }
    }
}
