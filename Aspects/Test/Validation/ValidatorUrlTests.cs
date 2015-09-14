using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Validation;

namespace vm.Aspects.Tests.Validation
{
    [TestClass]
    [DeploymentItem("..\\..\\Validation\\TestDataUrlValidator.csv")]
    public class ValidatorUrlTests
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

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\TestDataUrlValidator.csv", "TestDataUrlValidator#csv", DataAccessMethod.Sequential)]
        [TestMethod]
        public void ValidateTest()
        {
            if (TestContext.DataRow[0].ToString().StartsWith("#"))
                return;

            var url       = Convert.ToString(TestContext.DataRow["url"]);
            var isValid   = Convert.ToBoolean(TestContext.DataRow["isValid"]);

            try
            {
                Helper.TestValidator<string>(TestContext, new UrlValidatorAttribute(), url, isValid);
            }
            catch (UnitTestAssertException)
            {
                TestContext.WriteLine("{0} ??? !{1}", url, isValid);
                throw;
            }
        }
    }
}
