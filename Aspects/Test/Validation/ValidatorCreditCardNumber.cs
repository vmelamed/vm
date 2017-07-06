using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace vm.Aspects.Validation.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\Validation\\TestCreditCardNumberValidator.csv")]
    public class ValidatorCreditCardNumber
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

        [DataSource(
            "Microsoft.VisualStudio.TestTools.DataSource.CSV",
            ".\\TestCreditCardNumberValidator.csv",
            "TestCreditCardNumberValidator#csv",
            DataAccessMethod.Sequential)]
        [TestMethod]
        public void ValidateTest()
        {
            if (TestContext.DataRow[0].ToString().StartsWith("#"))
                return;

            var cc        = Convert.ToString(TestContext.DataRow["cc"]);
            var isValid   = Convert.ToBoolean(TestContext.DataRow["isValid"]);
            var fix       = Convert.ToString(TestContext.DataRow["fix"]);

            try
            {
                Helper.TestValidator<string>(TestContext, new LuhnValidatorAttribute(), cc, isValid);
            }
            catch (UnitTestAssertException)
            {
                TestContext.WriteLine("{0} ??? !{1}", cc, isValid);
                throw;
            }
        }
    }
}
