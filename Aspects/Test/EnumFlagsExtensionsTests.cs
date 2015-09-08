using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Summary description for EnumFlagsExtensionsTests
    /// </summary>
    [TestClass]
    public class EnumFlagsExtensionsTests
    {
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [Flags]
        enum TestEnum
        {
            None  = 0,
            One   = 1 << 0,
            Two   = 1 << 1,
            Three = 1 << 2,
            Four  = 1 << 3,
            Five  = 1 << 4,
            All   = One | Two | Three | Four | Five,
        };

        [TestMethod]
        public void IsEmptyTest()
        {
            var target = TestEnum.None;

            Assert.IsTrue(target.IsEmpty());
        }

        [TestMethod]
        public void IsNotEmptyTest()
        {
            var target = TestEnum.One | TestEnum.Three;

            Assert.IsFalse(target.IsEmpty());
        }

        [TestMethod]
        public void IsNotEmpty2Test()
        {
            var target = TestEnum.All;

            Assert.IsFalse(target.IsEmpty());
        }

        [TestMethod]
        public void HasAnyFlagsTest()
        {
            var target = TestEnum.One | TestEnum.Three;

            Assert.IsTrue(target.HasAnyFlags(TestEnum.One | TestEnum.Two));
        }

        [TestMethod]
        public void HasAnyFlagsNotTest()
        {
            var target = TestEnum.One | TestEnum.Three;

            Assert.IsFalse(target.HasAnyFlags(TestEnum.Four | TestEnum.Two));
        }

        [TestMethod]
        public void HasAnyFlagsForNoneTest()
        {
            var target = TestEnum.One | TestEnum.Three;

            Assert.IsFalse(target.HasAnyFlags(TestEnum.None));
        }

        [TestMethod]
        public void HasAnyFlagsNoneTest()
        {
            var target = TestEnum.None;

            Assert.IsFalse(target.HasAnyFlags(TestEnum.Four | TestEnum.Two));
        }

        [TestMethod]
        public void HasAnyFlagsAllTest()
        {
            var target = TestEnum.All;

            Assert.IsTrue(target.HasAnyFlags(TestEnum.Four | TestEnum.Two));
        }

        [TestMethod]
        public void HasAllFlagsTest()
        {
            var target = TestEnum.One | TestEnum.Three | TestEnum.Five;

            Assert.IsTrue(target.HasAllFlags(TestEnum.One | TestEnum.Three));
        }

        [TestMethod]
        public void HasAllFlagsNotTest()
        {
            var target = TestEnum.One | TestEnum.Three;

            Assert.IsFalse(target.HasAllFlags(TestEnum.One | TestEnum.Two));
        }

        [TestMethod]
        public void HasNoFlagsButTest()
        {
            var target = TestEnum.One | TestEnum.Three;

            Assert.IsTrue(target.HasNoFlagsBut(TestEnum.One | TestEnum.Three | TestEnum.Five));
        }

        [TestMethod]
        public void HasNoFlagsButNotTest()
        {
            var target = TestEnum.One | TestEnum.Three | TestEnum.Four;

            Assert.IsFalse(target.HasNoFlagsBut(TestEnum.One | TestEnum.Two));
        }
    }
}
