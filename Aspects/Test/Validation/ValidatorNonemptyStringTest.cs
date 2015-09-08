using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Diagnostics;
using vm.Aspects.Validation;

namespace vm.Aspects.Tests.Validation
{
    /// <summary>
    /// Summary description for NonemptyStringValidator
    /// </summary>
    [TestClass]
    public class ValidatorNonemptyStringTest
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

        void TestValidator(string value, bool isValid)
        {
            Helper.TestValidator<NonemptyStringValidatorAttribute, string>(TestContext, value, isValid);
        }

        [TestMethod]
        public void TestNonemptyStringValidator()
        {
            TestValidator("test", true);
            TestValidator(" test\t", true);
            TestValidator("\0 te\0st\t", true);
            TestValidator("\x0300", true);
            TestValidator("\0", true);
            TestValidator(" test\t", true);
            TestValidator("", false);
            TestValidator(null, false);
            TestValidator(" ", false);
            TestValidator(" \t\n\r\v", false);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestNonemptyStringValidatorOnNonStringType()
        {
            Helper.TestValidator<NonemptyStringValidatorAttribute, int>(TestContext, 1, false);
        }

        [TestMethod]
        public void TestNonemptyStringValidatorDefaultCtor()
        {
            var validator = new NonemptyStringValidator();
            var results = new ValidationResults();

            validator.DoValidate("nonempty", null, null, results);
            Assert.IsTrue(results.IsValid);
        }
    }
}
