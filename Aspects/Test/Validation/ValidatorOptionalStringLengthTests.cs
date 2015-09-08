using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Validation;

namespace vm.Aspects.Tests.Validation
{
    /// <summary>
    /// Summary description for ValidatorOptionalStringLengthTests
    /// </summary>
    [TestClass]
    public class ValidatorOptionalStringLengthTests
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

        void TestValidatorNoMoreThan6(string value, bool isValid)
        {
            Helper.TestValidator<string>(TestContext, new OptionalStringLengthValidatorAttribute(6), value, isValid);
        }

        void TestValidatorBetween3And6(string value, bool isValid)
        {
            Helper.TestValidator<string>(TestContext, new OptionalStringLengthValidatorAttribute(3, 6), value, isValid);
        }

        [TestMethod]
        public void TestNoMoreThan6()
        {
            TestValidatorNoMoreThan6(null, true);
            TestValidatorNoMoreThan6("", true);
            TestValidatorNoMoreThan6("1", true);
            TestValidatorNoMoreThan6("1234", true);
            TestValidatorNoMoreThan6("123456", true);
            TestValidatorNoMoreThan6("1234567", false);
        }

        [TestMethod]
        public void TestBetween3And6()
        {
            TestValidatorBetween3And6(null, true);
            TestValidatorBetween3And6("", false);
            TestValidatorBetween3And6("1", false);
            TestValidatorBetween3And6("123", true);
            TestValidatorBetween3And6("1234", true);
            TestValidatorBetween3And6("1234567", false);
        }
    }
}
