using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using vm.Aspects.Validation;

namespace vm.Aspects.Validation.Tests
{
    /// <summary>
    /// Summary description for ValidatorBeforeAfterTests
    /// </summary>
    [TestClass]
    public class ValidatorBeforeAfterTests
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

        const string _boundString = "2006-01-20T00:00:00.0000000";
        readonly DateTime _bound = DateTime.ParseExact(_boundString, "o", CultureInfo.InvariantCulture);

        public void TestValidators(DateTime value)
        {
            Helper.TestValidator<DateTime>(TestContext, new NotBeforeValidatorAttribute(_bound), value, value >= _bound);
            Helper.TestValidator<DateTime>(TestContext, new NotAfterValidatorAttribute(_bound), value, value <= _bound);
            Helper.TestValidator<DateTime>(TestContext, new NotBeforeValidatorAttribute(_boundString), value, value >= _bound);
            Helper.TestValidator<DateTime>(TestContext, new NotAfterValidatorAttribute(_boundString), value, value <= _bound);
        }

        [TestMethod]
        public void Test()
        {
            TestValidators(new DateTime(1, 1, 1, 0, 0, 0));
            TestValidators(new DateTime(2006, 1, 20, 0, 0, 0));
            TestValidators(new DateTime(2006, 1, 20, 1, 0, 0));
        }
    }
}
