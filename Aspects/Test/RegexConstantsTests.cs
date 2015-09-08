using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    [TestClass]
    [DeploymentItem(".\\RegexTestData.csv")]
    public class RegexConstantsTests
    {
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            testContext.WriteLine("Before the test class.");
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
        }

        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            TestContext.WriteLine("Before the test.");
        }
        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup() 
        {
            TestContext.WriteLine("After the test.");
        }
        //
        #endregion

        [DataSource(
            "Microsoft.VisualStudio.TestTools.DataSource.CSV", 
            ".\\RegexTestData.csv", 
            "RegexTestData#csv", 
            DataAccessMethod.Sequential)]
        [TestMethod]
        public void RegexTest()
        {
            if (TestContext.DataRow[0].ToString().StartsWith("#"))
                return;

            ParameterizedRegexTest(
                Convert.ToString(TestContext.DataRow["regexIdentifier"]),
                Convert.ToString(TestContext.DataRow["testValue"]),
                Convert.ToBoolean(TestContext.DataRow["expectedResult"]));

        }

        public void ParameterizedRegexTest(
            string regexIdentifier,
            string testValue,
            bool expected)
        {
            TestContext.WriteLine("Testing {0} against {1}, expecting {2}", regexIdentifier, testValue, expected);
            var regex = typeof(RegularExpression)
                            .GetProperty(regexIdentifier, BindingFlags.Public|
                                                          BindingFlags.Static|
                                                          BindingFlags.GetField|
                                                          BindingFlags.IgnoreCase)
                            .GetValue(null) as Regex;

            Assert.IsNotNull(regex, string.Format("The name of the Regex property {0} is either misspelled in the test file or does not exist.", regexIdentifier));

            Assert.AreEqual(
                expected,
                regex.IsMatch(testValue),
                "["+testValue+"] <=> " + regex);
        }

        [TestMethod]
        public void UnicodeTests()
        {
            ParameterizedRegexTest("EmailAddress", "あいうえお@domain.com", true);
            ParameterizedRegexTest("WcfUrl", "ws://あいうえお.香港/test", true);
            ParameterizedRegexTest("UrlFull", "https://あいうえお.香港/test", true);
        }
    }
}
