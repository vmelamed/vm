using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Model.EFRepository.HiLoIdentity;

namespace vm.Aspects.Model.Tests
{
    /// <summary>
    /// Summary description for HiLoIdentityGeneratorTests
    /// </summary>
    [TestClass]
    public class HiLoIdentityGeneratorTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        [TestMethod]
        public void TestParameterlessConstructor()
        {
            var target = new HiLoIdentityGenerator();

            Assert.AreEqual(1, target.HighValue);
            Assert.AreEqual(0, target.LowValue);
            Assert.AreEqual(HiLoIdentityGenerator.DefaultMaxLowValue, target.MaxLowValue);
            Assert.IsTrue(target.MaxHighValue * target.MaxLowValue + 1 > long.MaxValue / target.MaxLowValue);
            Assert.AreEqual(null, target.EntitySetName);
            Assert.AreEqual(-1, target.GetId());
        }

        [TestMethod]
        public void TestConstructorWithParameters()
        {
            var target = new HiLoIdentityGenerator("Test");

            Assert.AreEqual(1, target.HighValue);
            Assert.AreEqual(0, target.LowValue);
            Assert.AreEqual(HiLoIdentityGenerator.DefaultMaxLowValue, target.MaxLowValue);
            Assert.IsTrue(target.MaxHighValue * target.MaxLowValue + 1 > long.MaxValue / target.MaxLowValue);
            Assert.AreEqual("Test", target.EntitySetName);
            Assert.AreEqual(-1, target.GetId());
        }

        [TestMethod]
        public void IncrementHighValueTest()
        {
            var target = new HiLoIdentityGenerator();

            Assert.AreEqual(1, target.HighValue);
            target.IncrementHighValue();

            Assert.AreEqual(2, target.HighValue);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetIdLimitsTest()
        {
            var target = new HiLoIdentityGenerator();

            try
            {
                Assert.AreEqual(1, target.HighValue);

                target.HighValue = long.MaxValue;
                target.HighValue /= target.MaxLowValue;
                target.HighValue--;
                target.HighValue--;
                target.IncrementHighValue();

                for (var i = 0; i < target.MaxLowValue; i++)
                    target.GetId();
                var id = target.GetId();

                Assert.AreEqual(-1L, id);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
            target.IncrementHighValue();
        }
    }
}
