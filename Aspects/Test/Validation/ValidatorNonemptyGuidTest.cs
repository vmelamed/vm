using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Validation.Tests
{
    /// <summary>
    /// Summary description for NonemptyStringValidator
    /// </summary>
    [TestClass]
    public class ValidatorNonemptyGuidTest
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

        void TestValidator(Guid value, bool isValid)
        {
            Helper.TestValidator<NonemptyGuidValidatorAttribute, Guid>(TestContext, value, isValid);
        }

        [TestMethod]
        public void TestNonemptyStringValidator()
        {
            TestValidator(Guid.NewGuid(), true);
            TestValidator(Guid.Empty, false);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestNonemptyStringValidatorOnNonStringType()
        {
            Helper.TestValidator<NonemptyGuidValidatorAttribute, int>(TestContext, 1, false);
        }

        [TestMethod]
        public void TestNonemptyStringValidatorDefaultCtor()
        {
            var validator = new NonemptyGuidValidator();
            var results = new ValidationResults();

            validator.DoValidate(Guid.NewGuid(), null, null, results);
            Assert.IsTrue(results.IsValid);
        }
    }
}
