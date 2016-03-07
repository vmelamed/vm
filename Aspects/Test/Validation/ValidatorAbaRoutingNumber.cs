using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Diagnostics;
using vm.Aspects.Validation;

namespace vm.Aspects.Validation.Tests
{
    /// <summary>
    /// Summary description for NonemptyStringValidator
    /// </summary>
    [TestClass]
    public class ValidatorAbaRoutingNumber
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
            Helper.TestValidator<AbaRoutingNumberValidatorAttribute, string>(TestContext, value, isValid);
        }

        [TestMethod]
        public void TestAbaRoutingNumberValidator()
        {
            TestValidator(null, false);
            TestValidator("", false);
            TestValidator("123", false);
            TestValidator("1234567890", false);
            TestValidator("123456789", false);
            TestValidator("123456780", true);
            TestValidator("021100361", true);
            TestValidator("056073573", true);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestAbaRoutingNumberValidatorAttributeOnNonStringType()
        {
            Helper.TestValidator<AbaRoutingNumberValidatorAttribute, int>(TestContext, 1, false);
        }
    }
}
