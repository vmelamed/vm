using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\docs\\Expression.xsd")]
    [DeploymentItem("..\\..\\..\\docs\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\..\\docs\\DataContract.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public class ExpressionSerializerTest
    {
        public TestContext TestContext
        { get; set; }

        void Test(Expression expression, string fileName, bool validate = true)
        {
            TestHelpers.TestSerializeExpression(TestContext, expression, fileName, validate);
        }

        [TestMethod]
        public void TestDefaultInt()
        {
            Test(
                Expression.Default(typeof(int)),
                "TestFiles\\DefaultInt.xml");
        }

        [TestMethod]
        public void TestDefaultNullableInt()
        {
            Test(
                Expression.Default(typeof(int?)),
                "TestFiles\\DefaultNullableInt.xml");
        }

        [TestMethod]
        public void TestLambdaToBoolConstant()
        {
            Expression<Func<bool>> expression = () => true;

            Test(
                expression,
                "TestFiles\\LambdaToBoolConstant.xml");
        }

        [TestMethod]
        public void TestLambdaParamToBoolConstant()
        {
            Expression<Func<int, bool>> expression = i => true;

            Test(
                expression,
                "TestFiles\\LambdaParamToBoolConstant.xml");
        }

        [TestMethod]
        public void TestLambdaReturnParam()
        {
            Expression<Func<int, int>> expression = i => i;
            Test(
                expression,
                "TestFiles\\LambdaReturnParam.xml");
        }


        [TestMethod]
        public void TestLambdaReturnParam2()
        {
            Expression<Func<int, int, int>> expression = (i, j) => i;
            Test(
                expression,
                "TestFiles\\LambdaReturnParam2.xml");
        }

        [TestMethod]
        public void TestLambda2ParamToConstant()
        {
            Expression<Func<string, DateTime, bool>> expression = (s, d) => true;

            Test(
                expression,
                "TestFiles\\Lambda2ParamToConstant.xml");
        }

        [TestMethod]
        public void TestLambdaComplexParamToConstant()
        {
            Expression<Func<Object1, bool>> expression = o => true;

            Test(
                expression,
                "TestFiles\\LambdaComplexParamToConstant.xml");
        }

        [TestMethod]
        public void TestBlock()
        {
            var pa = Expression.Parameter(typeof(int), "a");
            var pb = Expression.Parameter(typeof(int), "b");

            Expression expression = Expression.Block(
                new[]
                {
                    pa,
                    pb,
                },
                new[]
                {
                    Expression.Assign(pa,
                        Expression.Add(
                            Expression.Add(
                                pa,
                                pb),
                            Expression.Constant(77))),
                    Expression.Assign(pb, pa),
                });

            Test(
                expression,
                "TestFiles\\Block.xml");
        }

        [TestMethod]
        public void TestConditional()
        {
            Expression<Func<bool, int>> expression = b => b ? 1 : 3;

            Test(
                expression,
                "TestFiles\\Conditional.xml");
        }

        [TestMethod]
        public void TestMember()
        {
            Expression<Func<Object1, int>> expression = o => o.IntProperty;

            Test(
                expression,
                "TestFiles\\LambdaMemberProperty.xml");
        }

        [TestMethod]
        public void TestFieldMember()
        {
            Expression<Func<Object1, string>> expression = o => o.StringField;

            Test(
                expression,
                "TestFiles\\LambdaMemberField.xml");
        }

        [TestMethod]
        public void TestArrayAccess()
        {
            var array = Expression.Parameter(typeof(int[,]), "a");

            var expression = Expression.Lambda(
                Expression.ArrayAccess(
                    array,
                    Expression.Constant(2, typeof(int)),
                    Expression.Constant(3, typeof(int))),
                array);

            Test(
                expression,
                "TestFiles\\LambdaArrayAccess.xml");
        }

        [TestMethod]
        public void TestTypeBinary()
        {
            Expression<Func<object, bool>> expression = a => a is bool;

            Test(
                expression,
                "TestFiles\\LambdaTypeBinary.xml");
        }

        [TestMethod]
        public void TestMethod1()
        {
            Expression<Func<int>> expression = () => TestMethods.Method1();

            Test(
                expression,
                "TestFiles\\LambdaMethodCall1.xml");
        }

        [TestMethod]
        public void TestMethod2()
        {
            Expression<Func<int>> expression = () => TestMethods.Method2(7, "seven");

            Test(
                expression,
                "TestFiles\\LambdaMethodCall2.xml");
        }

        [TestMethod]
        public void TestMethod3()
        {
            Expression<Func<TestMethods, int>> expression = tm => tm.Method3(7, 3.1415);

            Test(
                expression,
                "TestFiles\\LambdaMethodCall3.xml");
        }

        [TestMethod]
        public void TestInvocation()
        {
            Expression<Func<Func<int, int>, int, int>> expression = (d, a) => d(a);

            Test(
                expression,
                "TestFiles\\LambdaInvocation.xml");
        }

        [TestMethod]
        public void TestGoto1()
        {
            var returnTarget = Expression.Label();
            var expression = Expression.Block(
                                Expression.Call(
                                    typeof(Console).GetMethod(
                                                        "WriteLine",
                                                        new Type[] { typeof(string) }),
                                                        Expression.Constant("GoTo")),
                                Expression.Goto(returnTarget),
                                Expression.Call(
                                    typeof(Console).GetMethod(
                                                        "WriteLine",
                                                        new Type[] { typeof(string) }),
                                                        Expression.Constant("Other Work")),
                                Expression.Label(returnTarget));

            Test(
                expression,
                "TestFiles\\Goto1.xml");
        }

        [TestMethod]
        public void TestGoto2()
        {
            var returnTarget = Expression.Label();
            var variable = Expression.Parameter(typeof(int), "a");
            var expression = Expression.Block(
                                new List<ParameterExpression> { variable, },
                                Expression.Assign(variable, Expression.Constant(0)),
                                Expression.Increment(variable),
                                Expression.Goto(returnTarget),
                                Expression.Increment(variable),
                                Expression.Label(returnTarget));

            Test(
                expression,
                "TestFiles\\Goto2.xml");
        }

        [TestMethod]
        public void TestGoto11()
        {
            var returnTarget = Expression.Label();
            var expression = Expression.Block(
                                Expression.Call(
                                    typeof(Console).GetMethod(
                                                        "WriteLine",
                                                        new Type[] { typeof(string) }),
                                                        Expression.Constant("GoTo")),
                                                        Expression.Goto(returnTarget, type: null),
                                Expression.Call(
                                    typeof(Console).GetMethod(
                                                        "WriteLine",
                                                        new Type[] { typeof(string) }),
                                                        Expression.Constant("Other Work")),
                                Expression.Label(returnTarget));

            Test(
                expression,
                "TestFiles\\Goto11.xml");
        }

        [TestMethod]
        public void TestGoto21()
        {
            var returnTarget = Expression.Label();
            var variable = Expression.Parameter(typeof(int), "a");
            var expression = Expression.Block(
                                new List<ParameterExpression> { variable, },
                                Expression.Assign(variable, Expression.Constant(0)),
                                Expression.Increment(variable),
                                Expression.Goto(returnTarget, type: null),
                                Expression.Increment(variable),
                                Expression.Label(returnTarget));

            Test(
                expression,
                "TestFiles\\Goto21.xml");
        }

        [TestMethod]
        public void TestLoop1()
        {
            ParameterExpression value = Expression.Parameter(typeof(int), "value");
            ParameterExpression result = Expression.Parameter(typeof(int), "result");
            LabelTarget label = Expression.Label(typeof(int));

            // Creating a method body.
            BlockExpression expression = Expression.Block(
                new[]
                {
                    value,
                    result,
                },
                Expression.Assign(value, Expression.Constant(5)),
                Expression.Assign(result, Expression.Constant(1)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.GreaterThan(value, Expression.Constant(1)),
                        Expression.MultiplyAssign(result,
                            Expression.PostDecrementAssign(value)),
                        Expression.Break(label, result)),
                    label));

            Test(
                expression,
                "TestFiles\\Loop1.xml");
        }

        [TestMethod]
        public void TestSwitch()
        {
            ConstantExpression switchValue = Expression.Constant(3);

            // This expression represents a switch statement  
            // that has a default case.
            SwitchExpression expression = Expression.Switch(
                    switchValue,
                    Expression.Call(
                                null,
                                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                Expression.Constant("Default")),
                    new SwitchCase[]
                    {
                        Expression.SwitchCase(
                            Expression.Call(
                                null,
                                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                Expression.Constant("First")),
                            Expression.Constant(1)),
                        Expression.SwitchCase(
                            Expression.Call(
                                null,
                                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                Expression.Constant("Second")),
                            Expression.Constant(2),
                            Expression.Constant(3),
                            Expression.Constant(4))
                    });

            Test(
                expression,
                "TestFiles\\Switch.xml");
        }

        [TestMethod]
        public void TestTry1()
        {
            //try
            //{
            //    Console.WriteLine("TryBody");
            //}
            //catch
            //{
            //    Console.WriteLine("catch {}");
            //}

            var expression = Expression.TryFault(
                                Expression.Block(
                                    new Expression[]
                                    {
                                        Expression.Call(
                                                    null,
                                                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                    Expression.Constant("TryBody")),
                                        Expression.Throw(
                                                    Expression.New(
                                                        typeof(Exception).GetConstructor(Type.EmptyTypes))),
                                    }),
                                Expression.Call(
                                            null,
                                            typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                            Expression.Constant("catch {}")));

            Test(
                expression,
                "TestFiles\\Try1.xml");
        }

        [TestMethod]
        public void TestTry2()
        {
            //try
            //{
            //    Console.WriteLine("TryBody");
            //    throw new Exception();
            //}
            //catch (ArgumentException)
            //{
            //    Console.WriteLine("catch {}");
            //}

            var expression = Expression.TryCatch(
                                Expression.Block(
                                    new Expression[]
                                    {
                                        Expression.Call(
                                                    null,
                                                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                    Expression.Constant("TryBody")),
                                        Expression.Throw(
                                                    Expression.New(
                                                        typeof(Exception).GetConstructor(Type.EmptyTypes))),
                                    }),
                                new CatchBlock[]
                                {
                                    Expression.MakeCatchBlock(
                                        typeof(ArgumentException),
                                        null,
                                        Expression.Call(
                                                    null,
                                                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                    Expression.Constant("catch (ArgumentException) {}")),
                                        null),
                                });

            Test(
                expression,
                "TestFiles\\Try2.xml");
        }

        [TestMethod]
        public void TestTry3()
        {
            //try
            //{
            //    Console.WriteLine("TryBody");
            //    throw new Exception();
            //}
            //catch (ArgumentException)
            //{
            //    Console.WriteLine("catch {}");
            //}
            //finally
            //{
            //    Console.WriteLine("finally {}");
            //}

            var expression = Expression.TryCatchFinally(
                                Expression.Block(
                                    new Expression[]
                                    {
                                        Expression.Call(
                                                    null,
                                                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                    Expression.Constant("TryBody")),
                                        Expression.Throw(
                                                    Expression.New(
                                                        typeof(Exception).GetConstructor(Type.EmptyTypes))),
                                    }),
                                Expression.Call(
                                            null,
                                            typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                            Expression.Constant("finally {}")),
                                new CatchBlock[]
                                {
                                    Expression.MakeCatchBlock(
                                        typeof(ArgumentException),
                                        null,
                                        Expression.Call(
                                                    null,
                                                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                    Expression.Constant("catch (ArgumentException) {}")),
                                        null),
                                });

            Test(
                expression,
                "TestFiles\\Try3.xml");
        }

        [TestMethod]
        public void TestTry4()
        {
            //try
            //{
            //    Console.WriteLine("TryBody");
            //    throw new Exception();
            //}
            //finally
            //{
            //    Console.WriteLine("finally {}");
            //}

            var expression = Expression.TryFinally(
                                Expression.Block(
                                    new Expression[]
                                    {
                                        Expression.Call(
                                                    null,
                                                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                    Expression.Constant("TryBody")),
                                        Expression.Throw(
                                                    Expression.New(
                                                        typeof(Exception).GetConstructor(Type.EmptyTypes))),
                                    }),
                                Expression.Call(
                                            null,
                                            typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                            Expression.Constant("finally {}")));

            Test(
                expression,
                "TestFiles\\Try4.xml");
        }

        [TestMethod]
        public void TestTry5()
        {
            //try
            //{
            //    Console.WriteLine("TryBody");
            //    throw new Exception();
            //}
            //catch(ArgumentException x)
            //{
            //    Console.WriteLine(x.ParamName);
            //}

            var exception = Expression.Parameter(typeof(ArgumentException), "x");
            var expression = Expression.TryCatch(
                                    Expression.Block(
                                        new Expression[]
                                        {
                                            Expression.Call(
                                                        null,
                                                        typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                        Expression.Constant("TryBody")),
                                            Expression.Throw(
                                                        Expression.New(
                                                            typeof(Exception).GetConstructor(Type.EmptyTypes))),
                                        }),
                                    new CatchBlock[]
                                    {
                                        Expression.MakeCatchBlock(
                                            typeof(ArgumentException),
                                            exception,
                                            Expression.Call(
                                                        null,
                                                        typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                        Expression.MakeMemberAccess(exception, typeof(ArgumentException).GetProperty("Message"))),
                                            null),
                                    });

            Test(
                expression,
                "TestFiles\\Try5.xml");
        }

        [TestMethod]
        public void TestTry6()
        {
            //try
            //{
            //    Console.WriteLine("TryBody");
            //    throw new Exception();
            //}
            //catch (ArgumentException x) // when x.ParamName=="x"
            //{
            //    Console.WriteLine(x.ParamName);
            //}

            var exception = Expression.Parameter(typeof(ArgumentException), "x");
            var expression = Expression.TryCatch(
                                    Expression.Block(
                                        new Expression[]
                                        {
                                            Expression.Call(
                                                        null,
                                                        typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                        Expression.Constant("TryBody")),
                                            Expression.Throw(
                                                        Expression.New(
                                                            typeof(Exception).GetConstructor(Type.EmptyTypes))),
                                        }),
                                    new CatchBlock[]
                                    {
                                        Expression.MakeCatchBlock(
                                            typeof(ArgumentException),
                                            exception,
                                            Expression.Call(
                                                        null,
                                                        typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                                                        Expression.MakeMemberAccess(exception, typeof(ArgumentException).GetProperty("ParamName"))),
                                            Expression.Equal(
                                                Expression.MakeMemberAccess(exception, typeof(ArgumentException).GetProperty("ParamName")),
                                                Expression.Constant("x"))),
                                    });

            Test(
                expression,
                "TestFiles\\Try6.xml");
        }

        [TestMethod]
        public void TestListInit()
        {
            Expression<Func<IEnumerable<string>>> expression =
                () => new List<string>
                {
                    "aaa",
                    "bbb",
                    "ccc"
                };

            Test(
                expression,
                "TestFiles\\LambdaListInit.xml");
        }

        [TestMethod]
        public void TestNewArrayItems()
        {
            Expression<Func<string[]>> expression =
                () => new string[]
                {
                    "aaa",
                    "bbb",
                    "ccc"
                };

            Test(
                expression,
                "TestFiles\\LambdaNewArrayItems.xml");
        }

        [TestMethod]
        public void TestNewArrayBounds()
        {
            Expression<Func<string[,,]>> expression =
                () => new string[2, 3, 4];

            Test(
                expression,
                "TestFiles\\LambdaNewArrayBounds.xml");
        }

        [TestMethod]
        public void TestListInit1()
        {
            Expression<Func<IDictionary<int, string>>> expression =
                () => new Dictionary<int, string>
                {
                    { 1, "one" },
                    { 2, "two" },
                    { 3, "three" },
                };

            Test(
                expression,
                "TestFiles\\LambdaListInit1.xml");
        }

        [TestMethod]
        public void TestMemberInit()
        {
            Expression<Func<TestMembersInitialized>> expression =
                () => new TestMembersInitialized
                {
                    TheOuterIntProperty = 42,
                    Time = new DateTime(1776, 7, 4),
                    InnerProperty = new Inner
                    {
                        IntProperty = 23,
                        StringProperty = "inner string"
                    },
                    MyProperty = new List<string>
                    {
                        "aaa",
                        "bbb",
                        "ccc",
                    },
                };

            Test(
                expression,
                "TestFiles\\LambdaMemberInit.xml");
        }

        [TestMethod]
        public void TestRuntimeVariables()
        {
            var expression = Expression.RuntimeVariables(
                                            Expression.Parameter(typeof(string), "a"),
                                            Expression.Parameter(typeof(int), "b"));

            Test(
                expression,
                "TestFiles\\RuntimeVariables.xml");
        }

        [TestMethod]
        public void TestDynamic()
        {
            var x = Expression.Parameter(typeof(object), "x");
            var y = Expression.Parameter(typeof(object), "y");
            var binder = Binder.BinaryOperation(
                            CSharpBinderFlags.None,
                            ExpressionType.Add,
                            GetType(),
                            new CSharpArgumentInfo[]
                            {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                            });

            Expression<Func<dynamic, dynamic, dynamic>> f =
                Expression.Lambda<Func<object, object, object>>(
                            Expression.Dynamic(binder, typeof(object), x, y),
                            new[]
                            {
                                x,
                                y,
                            });
            TestContext.WriteLine(f.Compile()(1, "abc"));
        }
    }
}
