using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Validation;

namespace vm.Aspects.Tests.Validation
{
    /// <summary>
    /// Summary description for ValidatorXmlStringTests
    /// </summary>
    [TestClass]
    [DeploymentItem("..\\..\\Linq\\Expressions\\Serialization\\Tests\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\Linq\\Expressions\\Serialization\\Tests\\DataContract.xsd")]
    [DeploymentItem("..\\..\\Linq\\Expressions\\Serialization\\Documents\\Expression.xsd")]
    public class ValidatorXmlStringTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            XmlStringValidator.AddSchema("http://schemas.microsoft.com/2003/10/Serialization/", "Microsoft.Serialization.xsd");
            XmlStringValidator.AddSchema("http://schemas.datacontract.org/2004/07/System", "DataContract.xsd");
            XmlStringValidator.AddSchema("urn:schemas-vm-com:Aspects.Linq.Expression", "Expression.xsd");
        }

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup]
        // public void MyTestCleanup() { }
        //
        #endregion

        void TestValidator(string value, bool isValid)
        {
            Helper.TestValidator<string>(TestContext, new XmlStringValidatorAttribute("expression", "urn:schemas-vm-com:Aspects.Expression"), value, isValid);
        }

        void TestNoRootValidator(string value, bool isValid)
        {
            Helper.TestValidator<string>(TestContext, new XmlStringValidatorAttribute(), value, isValid);
        }

        const string Lambda = @"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<!-- (a, b) => (a + b) -->
<expression xmlns=""urn:schemas-vm-com:Aspects.Expression"">
    <lambda>
        <parameters>
            <parameter type=""int""
                       name=""a"" />
            <parameter type=""int""
                       name=""b"" />
        </parameters>
        <body>
            <addChecked>
                <parameter name=""a"" />
                <parameter name=""b"" />
            </addChecked>
        </body>
    </lambda>
</expression>";

        [TestMethod]
        public void TestXml()
        {
            TestValidator(null, false);
            TestValidator("", false);
            TestValidator("bogus", false);
            TestValidator("<element/>", false);
            TestValidator(Lambda, true);
            TestNoRootValidator(null, false);
            TestNoRootValidator("", false);
            TestNoRootValidator("bogus", false);
            TestNoRootValidator("<element/>", true);
            TestNoRootValidator(Lambda, true);
        }
    }
}
