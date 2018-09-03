using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Linq.Expressions.Serialization;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\docs\\Expression.xsd")]
    [DeploymentItem("..\\..\\..\\docs\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\..\\docs\\DataContract.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public class ConstantExpressionDeserializerComplexTypesTest
    {
        public TestContext TestContext
        { get; set; }

        void TestConstant(string fileName, Expression expected)
        {
            TestHelpers.TestDeserializeExpression(TestContext, fileName, expected);
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeObject1()
        {
            TestConstant(
                "TestFiles\\Object1.xml",
                Expression.Constant(Object1.GetObject1()));
        }

        [TestMethod]
        public void TestConstantExpressionNullObject()
        {
            TestConstant(
                "TestFiles\\Object1Null.xml",
                Expression.Constant(null, typeof(Object1)));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeIntArray()
        {
            TestConstant(
                "TestFiles\\ArrayOfInt.xml",
                Expression.Constant(new int[] { 1, 2, 3, 4, }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeNullableIntArray()
        {
            TestConstant(
                "TestFiles\\ArrayOfNullableInt.xml",
                Expression.Constant(new int?[] { 1, 2, null, null }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeIntList()
        {
            TestConstant(
                "TestFiles\\ListOfInt.xml",
                Expression.Constant(new List<int> { 1, 2, 3, 4, }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeNullableIntList()
        {
            TestConstant(
                "TestFiles\\ListOfNullableInt.xml",
                Expression.Constant(new List<int?> { 1, 2, null, null }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeClassDataContract1()
        {
            TestConstant(
                "TestFiles\\ClassDataContract1.xml",
                Expression.Constant(
                                new ClassDataContract1
                                {
                                    IntProperty = 7,
                                    StringProperty = "vm",
                                }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeClassDataContract1Array()
        {
            TestConstant(
                "TestFiles\\ArrayOfClassDataContract1.xml",
                Expression.Constant(
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
                                }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeClassSerializable1()
        {
            TestConstant(
                "TestFiles\\ClassSerializable1.xml",
                Expression.Constant(
                                new ClassSerializable1
                                {
                                    IntProperty = 8,
                                    StringProperty = "vm",
                                }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeStructDataContract1()
        {
            TestConstant(
                "TestFiles\\StructDataContract1.xml",
                Expression.Constant(
                                new StructDataContract1
                                {
                                    IntProperty = 7,
                                    StringProperty = "vm",
                                }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeNullStruct()
        {
            TestConstant(
                "TestFiles\\StructDataContract1Null.xml",
                Expression.Constant(default(StructDataContract1?), typeof(StructDataContract1?)));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeStructDataContract1Array()
        {
            TestConstant(
                "TestFiles\\ArrayOfStructDataContract1.xml",
                Expression.Constant(
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
                            }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeNullableStructDataContract1Array()
        {
            TestConstant(
                "TestFiles\\ArrayOfNullableStructDataContract1.xml",
                Expression.Constant(
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
                                }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeStructSerializable1()
        {
            TestConstant(
                "TestFiles\\StructSerializable1.xml",
                Expression.Constant(
                                new StructSerializable1
                                {
                                    IntProperty = 8,
                                    StringProperty = "vm",
                                }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeIntStringDictionary()
        {
            TestConstant(
                "TestFiles\\IntStringDictionary.xml",
                Expression.Constant(
                                new Dictionary<int, string>
                                {
                                    { 1, "one" },
                                    { 2, "two" },
                                    { 3, "three" },
                                }));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeStructClassDictionary()
        {
            TestConstant(
                "TestFiles\\StructDataContract1ClassDataContract1Dictionary.xml",
                Expression.Constant(
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
                            }));
        }


        [TestMethod]
        public void TestConstantExpressionDeserializeAnonymous()
        {
            TestConstant(
                "TestFiles\\Anonymous.xml",
                Expression.Constant(
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
                                    IntProperty = (int)1,
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
                                }));
        }
    }
}
