using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\src\\schemas\\Expression.xsd")]
    [DeploymentItem("..\\..\\..\\src\\schemas\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\..\\src\\schemas\\DataContract.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public class ConstantExpressionSerializerComplexTypesTest
    {
        public TestContext TestContext
        { get; set; }

        void TestConstant<E>(E test, string expected, bool validate = true)
        {
            TestHelpers.TestSerializeExpression(TestContext, Expression.Constant(test, typeof(E)), expected, validate);
        }

        void Test(Expression test, string expected, bool validate = true)
        {
            TestHelpers.TestSerializeExpression(TestContext, test, expected, validate);
        }

        [TestMethod]
        public void TestConstantExpressionNullObject()
        {
            Test(
                Expression.Constant(null, typeof(Object1)),
                "TestFiles\\Object1Null.xml");
        }

        [TestMethod]
        public void TestConstantExpressionObject1()
        {
            TestConstant(
                Object1.GetObject1(),
                "TestFiles\\Object1.xml");
        }

        [TestMethod]
        public void TestConstantExpressionIntArray()
        {
            TestConstant(
                new int[] { 1, 2, 3, 4, },
                "TestFiles\\ArrayOfInt.xml");
        }

        [TestMethod]
        public void TestConstantExpressionNullableIntArray()
        {
            TestConstant(
                new int?[] { 1, 2, null, null },
                "TestFiles\\ArrayOfNullableInt.xml");
        }

        [TestMethod]
        public void TestConstantExpressionIntList()
        {
            TestConstant(
                new List<int> { 1, 2, 3, 4, },
                "TestFiles\\ListOfInt.xml");
        }

        [TestMethod]
        public void TestConstantExpressionNullableIntList()
        {
            TestConstant(
                new List<int?> { 1, 2, null, null },
                "TestFiles\\ListOfNullableInt.xml");
        }

        [TestMethod]
        public void TestConstantExpressionClassDataContract1()
        {
            TestConstant(
                new ClassDataContract1
                {
                    IntProperty = 7,
                    StringProperty = "vm",
                },
                "TestFiles\\ClassDataContract1.xml");
        }

        [TestMethod]
        public void TestConstantExpressionClassDataContract1Array()
        {
            TestConstant(
                new ClassDataContract1[]
                {
                    new ClassDataContract1
                    {
                        IntProperty = 0,
                        StringProperty = "vm",
                    },
                    new ClassDataContract1
                    {
                        IntProperty = 1,
                        StringProperty = "vm vm",
                    },
                    null,
                },
                "TestFiles\\ArrayOfClassDataContract1.xml");
        }

        [TestMethod]
        public void TestConstantExpressionClassSerializable1()
        {
            TestConstant(
                new ClassSerializable1
                {
                    IntProperty = 8,
                    StringProperty = "vm",
                },
                "TestFiles\\ClassSerializable1.xml");
        }

        [TestMethod]
        [ExpectedException(typeof(SerializationException))]
        public void TestConstantExpressionClassNonSerializable()
        {
            TestConstant(
                new ClassNonSerializable
                {
                    IntProperty = 9,
                    StringProperty = "vm",
                },
                "");
        }

        [TestMethod]
        public void TestConstantExpressionStructDataContract1()
        {
            TestConstant(
                new StructDataContract1
                {
                    IntProperty = 7,
                    StringProperty = "vm",
                },
                "TestFiles\\StructDataContract1.xml");
        }

        [TestMethod]
        public void TestConstantExpressionStructNullableDataContract1()
        {
            Test(
                Expression.Constant(
                    new StructDataContract1?(
                        new StructDataContract1
                        {
                            IntProperty = 7,
                            StringProperty = "vm",
                        }),
                    typeof(StructDataContract1?)),
                "TestFiles\\NullableStructDataContract1.xml");
        }

        [TestMethod]
        public void TestConstantExpressionNullStruct()
        {
            TestConstant(
                default(StructDataContract1?),
                "TestFiles\\StructDataContract1Null.xml");
        }

        [TestMethod]
        public void TestConstantExpressionStructDataContract1Array()
        {
            TestConstant(
                new StructDataContract1[]
                {
                    new StructDataContract1
                    {
                        IntProperty = 0,
                        StringProperty = "vm",
                    },
                    new StructDataContract1
                    {
                        IntProperty = 1,
                        StringProperty = "vm vm",
                    },
                },
                "TestFiles\\ArrayOfStructDataContract1.xml");
        }

        [TestMethod]
        public void TestConstantExpressionNullableStructDataContract1Array()
        {
            TestConstant(
                new StructDataContract1?[]
                {
                    new StructDataContract1
                    {
                        IntProperty = 0,
                        StringProperty = "vm",
                    },
                    null,
                    new StructDataContract1
                    {
                        IntProperty = 1,
                        StringProperty = "vm vm",
                    },
                    null,
                },
                "TestFiles\\ArrayOfNullableStructDataContract1.xml");
        }

        [TestMethod]
        public void TestConstantExpressionStructSerializable1()
        {
            TestConstant(
                new StructSerializable1
                {
                    IntProperty = 8,
                    StringProperty = "vm",
                },
                "TestFiles\\StructSerializable1.xml");
        }

        [TestMethod]
        public void TestConstantExpressionIntStringDictionary()
        {
            TestConstant(
                new Dictionary<int, string>
                {
                    { 1, "one" },
                    { 2, "two" },
                    { 3, "three" },
                },
                "TestFiles\\IntStringDictionary.xml");
        }

        [TestMethod]
        public void TestConstantExpressionStructClassDictionary()
        {
            TestConstant(
                new Dictionary<StructDataContract1, ClassDataContract1>
                {
                    {
                        new StructDataContract1
                        {
                            IntProperty = 1,
                            StringProperty = "aaa",
                        },
                        new ClassDataContract1
                        {
                            IntProperty = 1,
                            StringProperty = "aaa",
                        }
                    },
                    {
                        new StructDataContract1
                        {
                            IntProperty = 2,
                            StringProperty = "bbb",
                        },
                        new ClassDataContract1
                        {
                            IntProperty = 2,
                            StringProperty = "bbb",
                        }
                    },
                    {
                        new StructDataContract1
                        {
                            IntProperty = 3,
                            StringProperty = "ccc",
                        },
                        new ClassDataContract1
                        {
                            IntProperty = 3,
                            StringProperty = "ccc",
                        }
                    },
                },
                "TestFiles\\StructDataContract1ClassDataContract1Dictionary.xml");
        }

        [TestMethod]
        public void TestConstantExpressionAnonymous()
        {
            TestConstant(
                new
                {
                    ObjectProperty = (object)null,
                    NullIntProperty = (int?)null,
                    NullLongProperty = (long?)1L,
                    BoolProperty = true,
                    CharProperty = 'A',
                    ByteProperty = (byte)1,
                    SByteProperty = (sbyte)1,
                    ShortProperty = (short)1,
                    IntProperty = 1,
                    LongProperty = (long)1,
                    UShortProperty = (ushort)1,
                    UIntProperty = (uint)1,
                    ULongProperty = (ulong)1,
                    DoubleProperty = 1.0,
                    FloatProperty = (float)1.0,
                    DecimalProperty = 1M,
                    GuidProperty = Guid.Empty,
                    UriProperty = new Uri("http://localhost"),
                    DateTimeProperty = new DateTime(2013, 1, 13),
                    TimeSpanProperty = new TimeSpan(123L),
                    DateTimeOffsetProperty = new DateTimeOffset(new DateTime(2013, 1, 13)),
                },
                "TestFiles\\Anonymous.xml");
        }
    }
}
