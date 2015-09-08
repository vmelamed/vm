using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Validation;

namespace vm.Aspects.Tests.Validation
{
    /// <summary>
    /// Summary description for ValidatorsTests
    /// </summary>
    [TestClass]
    public class ValidatorEnumDomainTests
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

        enum Number
        {
            One,
            Two,
            Three,
        }

        void TestValidator(Number value, bool isValid)
        {
            Helper.TestValidator<EnumDomainValidatorAttribute, Number>(TestContext, value, isValid);
        }

        void TestNValidator(Number? value, bool isValid)
        {
            Helper.TestValidator<EnumDomainValidatorAttribute, Number?>(TestContext, value, isValid);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestEnumValidatorAttributeOnDouble()
        {
            Helper.TestValidator<EnumDomainValidatorAttribute, double>(TestContext, 1.0D, false);
        }

        [TestMethod]
        public void TestEnumValidatorOnDouble()
        {
            var results = new ValidationResults();

            new EnumDomainValidator<Number>("", "", false).DoValidate(1.0, null, "", results);
            Assert.IsFalse(results.IsValid);
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestMethod]
        public void TestEnumValidatorOnDouble1()
        {
            var results = new ValidationResults();

            new EnumDomainValidator<double>("", "", false);
        }

        [TestMethod]
        public void TestEnumDomainValidator()
        {
            TestValidator(Number.One, true);
            TestValidator((Number)4, false);
            TestNValidator(new Number?(Number.One), true);
            TestNValidator(new Number?((Number)4), false);
            TestNValidator(new Number?(), true);
        }
    }
}
