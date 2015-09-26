using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Validation;

namespace vm.Aspects.Validation.Tests
{
    [TestClass]
    public class ValidatorPositivityTests
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

        void TestNonnegativeValidator<T>(T value, bool isValid)
        {
            Helper.TestValidator<NonnegativeValidatorAttribute, T>(TestContext, value, isValid);
        }

        void TestPositiveValidator<T>(T value, bool isValid)
        {
            Helper.TestValidator<PositiveValidatorAttribute, T>(TestContext, value, isValid);
        }

        [TestMethod]
        public void NonnegativeValidator()
        {
            TestNonnegativeValidator(0, true);
            TestNonnegativeValidator(1, true);
            TestNonnegativeValidator(-1, false);
            TestNonnegativeValidator(0L, true);
            TestNonnegativeValidator(1L, true);
            TestNonnegativeValidator(-1L, false);
            TestNonnegativeValidator(0M, true);
            TestNonnegativeValidator(1M, true);
            TestNonnegativeValidator(-1M, false);
            TestNonnegativeValidator(0.0, true);
            TestNonnegativeValidator(0.1, true);
            TestNonnegativeValidator(-0.1, false);
        }

        //-----------------------------------

        [TestMethod]
        public void PositiveValidator()
        {
            TestPositiveValidator(0, false);
            TestPositiveValidator(1, true);
            TestPositiveValidator(-1, false);
            TestPositiveValidator(0L, false);
            TestPositiveValidator(1L, true);
            TestPositiveValidator(-1L, false);
            TestPositiveValidator(0M, false);
            TestPositiveValidator(1M, true);
            TestPositiveValidator(-1M, false);
            TestPositiveValidator(0.0, false);
            TestPositiveValidator(0.1, true);
            TestPositiveValidator(-0.1, false);
        }
    }
}
