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
    public class ConstantExpressionSerializerBasicTypesTest
    {
        public TestContext TestContext
        { get; set; }

        void TestConstant<E>(E test, string fileName, bool validate = true)
        {
            TestHelpers.TestSerializeExpression(TestContext, Expression.Constant(test, typeof(E)), fileName, validate);
        }

        [TestMethod]
        public void TestBasicConstantDBNull()
        {
            TestConstant(
                DBNull.Value,
                "TestFiles\\DBNull.xml");
        }

        [TestMethod]
        public void TestBasicConstantBool()
        {
            TestConstant(
                true,
                "TestFiles\\Bool.xml");
        }

        [TestMethod]
        public void TestBasicConstantChar()
        {
            TestConstant(
                'V',
                "TestFiles\\Char.xml");
        }

        [TestMethod]
        public void TestBasicConstantByte()
        {
            TestConstant(
                (byte)5,
                "TestFiles\\Byte.xml");
        }

        [TestMethod]
        public void TestBasicConstantSByte()
        {
            TestConstant(
                (sbyte)5,
                "TestFiles\\SByte.xml");
        }

        [TestMethod]
        public void TestBasicConstantShort()
        {
            TestConstant(
                (short)5,
                "TestFiles\\Short.xml");
        }

        [TestMethod]
        public void TestBasicConstantUshort()
        {
            TestConstant(
                (ushort)5,
                "TestFiles\\UShort.xml");
        }

        [TestMethod]
        public void TestBasicConstantInt()
        {
            TestConstant(
                5,
                "TestFiles\\Int.xml");
        }

        [TestMethod]
        public void TestBasicConstantNullableInt()
        {
            int? value = 5;

            TestConstant(
                value,
                "TestFiles\\NullableInt.xml");
        }

        [TestMethod]
        public void TestBasicConstantUint()
        {
            TestConstant(
                (uint)5,
                "TestFiles\\UInt.xml");
        }

        [TestMethod]
        public void TestBasicConstantLong()
        {
            TestConstant(
                (long)5,
                "TestFiles\\Long.xml");
        }

        [TestMethod]
        public void TestBasicConstantNullableLong()
        {
            long? value = null;

            TestConstant(
                value,
                "TestFiles\\NullableLong.xml");
        }

        [TestMethod]
        public void TestBasicConstantUlong()
        {
            TestConstant(
                (ulong)5,
                "TestFiles\\Ulong.xml");
        }

        [TestMethod]
        public void TestBasicConstantFloat()
        {
            TestConstant(
                (float)5.5123453123E-34,
                "TestFiles\\Float.xml");
        }

        [TestMethod]
        public void TestBasicConstantDouble()
        {
            TestConstant(
                5.512345098711111123E-123,
                "TestFiles\\Double.xml");
        }

        [TestMethod]
        public void TestBasicConstantDecimal()
        {
            TestConstant(
                5.5M,
                "TestFiles\\Decimal.xml");
        }

        [TestMethod]
        public void TestBasicConstantIntPtr()
        {
            IntPtr a = (IntPtr)4;

            TestConstant(
                (IntPtr)5,
                "TestFiles\\IntPtr.xml");
        }

        [TestMethod]
        public void TestBasicConstantUIntPtr()
        {
            TestConstant(
                (UIntPtr)5,
                "TestFiles\\UIntPtr.xml");
        }

        [TestMethod]
        public void TestBasicConstantEnum()
        {
            TestConstant(
                EnumTest.Three,
                "TestFiles\\Enum.xml");
        }

        [TestMethod]
        public void TestBasicConstantEnumFlags()
        {
            EnumFlagsTest value = EnumFlagsTest.One | EnumFlagsTest.Three;

            TestConstant(
                value,
                "TestFiles\\EnumFlags.xml");
        }

        [TestMethod]
        public void TestBasicConstantString()
        {
            TestConstant("abrah-cadabrah", "TestFiles\\String.xml");
        }

        [TestMethod]
        public void TestBasicConstantGuid()
        {
            TestConstant(
                Guid.Empty,
                "TestFiles\\Guid.xml");
        }

        [TestMethod]
        public void TestBasicConstantUri()
        {
            TestConstant(
                new Uri("http://www.yahoo.com"),
                "TestFiles\\Uri.xml");
        }

        [TestMethod]
        public void TestBasicConstantDateTime()
        {
            TestConstant(
                new DateTime(2013, 1, 13, 15, 48, 33, 222, DateTimeKind.Local),
                "TestFiles\\DateTime.xml");
        }

        [TestMethod]
        public void TestBasicConstantTimeSpan()
        {
            TestConstant(
                new TimeSpan(3, 4, 15, 32, 123),
                "TestFiles\\TimeSpan.xml");
        }

        [TestMethod]
        public void TestBasicConstantDateTimeOffset()
        {
            TestConstant(
                new DateTimeOffset(new DateTime(2013, 1, 13, 15, 43, 33, 333, DateTimeKind.Utc)),
                "TestFiles\\DateTimeOffset.xml");
        }
    }
}
