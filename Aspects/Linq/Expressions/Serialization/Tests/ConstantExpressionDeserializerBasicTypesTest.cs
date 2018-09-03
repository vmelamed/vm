using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\docs\\Expression.xsd")]
    [DeploymentItem("..\\..\\..\\docs\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public class ConstantExpressionDeserializerBasicTypesTest
    {
        public TestContext TestContext
        { get; set; }

        void TestConstant(string fileName, Expression expected)
        {
            TestHelpers.TestDeserializeExpression(TestContext, fileName, expected);
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeBool()
        {
            TestConstant(
                "TestFiles\\Bool.xml",
                Expression.Constant(true));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeChar()
        {
            TestConstant(
                "TestFiles\\Char.xml",
                Expression.Constant('V'));
        }


        [TestMethod]
        public void TestConstantExpressionDeserializeByte()
        {
            TestConstant(
                "TestFiles\\Byte.xml",
                Expression.Constant((byte)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeSByte()
        {
            TestConstant(
                "TestFiles\\SByte.xml",
                Expression.Constant((sbyte)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeShort()
        {
            TestConstant(
                "TestFiles\\Short.xml",
                Expression.Constant((short)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeUshort()
        {
            TestConstant(
                "TestFiles\\UShort.xml",
                Expression.Constant((ushort)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeInt()
        {
            TestConstant(
                "TestFiles\\Int.xml",
                Expression.Constant((int)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeNullableInt()
        {
            int? value = 5;
            TestConstant(
                "TestFiles\\NullableInt.xml",
                Expression.Constant(value, typeof(int?)));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeUint()
        {
            TestConstant(
                "TestFiles\\UInt.xml",
                Expression.Constant((uint)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeLong()
        {
            TestConstant(
                "TestFiles\\Long.xml",
                Expression.Constant((long)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeNullableLong()
        {
            long? value = null;
            TestConstant(
                "TestFiles\\NullableLong.xml",
                Expression.Constant(value, typeof(long?)));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeUlong()
        {
            TestConstant(
                "TestFiles\\ULong.xml",
                Expression.Constant((ulong)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeFloat()
        {
            TestConstant(
                "TestFiles\\Float.xml",
                Expression.Constant((float)5.5123453123E-34));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeDouble()
        {
            TestConstant(
                "TestFiles\\Double.xml",
                Expression.Constant((double)5.512345098711111123E-123));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeDecimal()
        {
            TestConstant(
                "TestFiles\\Decimal.xml",
                Expression.Constant((decimal)5.5M));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeIntPtr()
        {
            TestConstant(
                "TestFiles\\IntPtr.xml",
                Expression.Constant((IntPtr)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeUIntPtr()
        {
            TestConstant(
                "TestFiles\\UIntPtr.xml",
                Expression.Constant((UIntPtr)5));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeEnum()
        {
            TestConstant(
                "TestFiles\\Enum.xml",
                Expression.Constant(EnumTest.Three));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeFlagsEnum()
        {
            TestConstant(
                "TestFiles\\EnumFlags.xml",
                Expression.Constant(EnumFlagsTest.One | EnumFlagsTest.Three));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeString()
        {
            TestConstant(
                "TestFiles\\String.xml",
                Expression.Constant("abrah-cadabrah"));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeGuid()
        {
            TestConstant(
                "TestFiles\\Guid.xml",
                Expression.Constant(Guid.Empty));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeUri()
        {
            TestConstant(
                "TestFiles\\Uri.xml",
                Expression.Constant(new Uri("http://www.yahoo.com")));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeDateTime()
        {
            TestConstant(
                "TestFiles\\DateTime.xml",
                Expression.Constant(new DateTime(2013, 1, 13, 15, 48, 33, 222, DateTimeKind.Local)));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeTimeSpan()
        {
            TestConstant(
                "TestFiles\\TimeSpan.xml",
                Expression.Constant(new TimeSpan(3, 4, 15, 32, 123)));
        }

        [TestMethod]
        public void TestConstantExpressionDeserializeDateTimeOffset()
        {
            TestConstant(
                "TestFiles\\DateTimeOffset.xml",
                Expression.Constant(new DateTimeOffset(new DateTime(2013, 1, 13, 15, 43, 33, 333, DateTimeKind.Utc)), typeof(DateTimeOffset)));
        }
    }
}
