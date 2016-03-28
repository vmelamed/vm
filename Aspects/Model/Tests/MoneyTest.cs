using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace vm.Aspects.Model.Tests
{
    /// <summary>
    /// Summary description for MoneyTest
    /// </summary>
    [TestClass]
    public class MoneyTest : IdentityTester<Money>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoneyTest"/> class - initializes the identity tester.
        /// </summary>
        public MoneyTest()
        {
            Initialize(
                new Money(123.45M, "USD"),
                new Money(123.45M, "USD"),
                new Money(123.45M, "USD"),
                new Money(123.46M, "USD"),
                m => { });
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}

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
        #endregion

        [TestMethod]
        public void TestConstructor()
        {
            var target = new Money(123.45M, "USD");

            Assert.AreEqual(123.45M, target.Amount);
            Assert.AreEqual("USD", target.Currency);
        }

        [TestMethod]
        public void TestConstructorRounding()
        {
            var target = new Money(123.456M, "USD");

            Assert.AreEqual(123.46M, target.Amount);
            Assert.AreEqual("USD", target.Currency);
        }

        [TestMethod]
        public void TestConstructorDefaultCurrency()
        {
            var target = new Money(123.45M);

            Assert.AreEqual(123.45M, target.Amount);
            Assert.AreEqual("USD", target.Currency);
        }

        [TestMethod]
        public void RegistrationLookupOperatorEqualsTest()
        {
            var target1 = new Money(123.45M, "USD");
            var target2 = new Money(123.45M, "USD");
            var target3 = new Money(123.45M, "USD");
            var target4 = new Money(123.46M, "USD");

            Assert.IsTrue(!(target1 == (Money)null), "target1 must not be equal to null.");
            Assert.IsTrue(!((Money)null == target1), "target1 must not be equal to obj1.");

            // reflexitivity
            var t = target1;

            Assert.IsTrue(target1 == t, "The operator == must be reflexive.");
            Assert.IsFalse(target1 != t, "The operator == must be reflexive.");

            // symmetricity
            Assert.AreEqual(target1 == target2, target2 == target1, "The operator == must be symmetric.");
            Assert.AreEqual(target1 != target4, target4 != target1, "The operator != must be symmetric.");

            // transityvity
            Assert.IsTrue(target1 == target2 && target2 == target3 && target3 == target1, "The operator == must be transitive.");
            Assert.IsTrue(target1 == target2 && target1 != target4 && target2 != target4, "The operator != must be transitive.");
        }

        [TestMethod]
        public void TestSerialization()
        {
            var target = new Money(123.45M);
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, target);

            stream.Seek(0, SeekOrigin.Begin);

            var target1 = formatter.Deserialize(stream);

            Assert.AreEqual(target, target1);
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            var target2 = new Money(123.45M, "USD");
            var target3 = new Money(123.45M, "USD");
            var target4 = new Money(123.46M, "USD");

            Assert.AreEqual(target2.GetHashCode(), target3.GetHashCode());
            Assert.AreNotEqual(target3.GetHashCode(), target4.GetHashCode());
        }

        [TestMethod]
        public void TestClone()
        {
            var target = new Money(123.45M, "USD");
            var clone = target.Clone();

            Assert.IsFalse(ReferenceEquals(target, clone));
            Assert.AreEqual(target, clone);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCompareToNullMoney()
        {
            var target = new Money(123.45M, "USD");

            target.CompareTo((Money)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The currencies are different.")]
        public void TestCompareToDifferentCurrency()
        {
            var target = new Money(123.45M, "USD");
            var target1 = new Money(123.45M, "EUR");

            target.CompareTo(target1);
        }

        [TestMethod]
        public void TestCompareEquals()
        {
            var target = new Money(123.45M, "USD");
            var target1 = new Money(123.45M, "USD");

            Assert.AreEqual(0, target.CompareTo(target1));
        }

        [TestMethod]
        public void TestCompareGreater()
        {
            var target = new Money(123.45M, "USD");
            var target1 = new Money(123.44M, "USD");

            Assert.IsTrue(target.CompareTo(target1) > 0);
        }

        [TestMethod]
        public void TestCompareLess()
        {
            var target = new Money(123.43M, "USD");
            var target1 = new Money(123.45M, "USD");

            Assert.IsTrue(target.CompareTo(target1) < 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCompareToNullObject()
        {
            var target = new Money(123.45M, "USD");

            target.CompareTo(null);
        }

        [TestMethod]
        public void TestCompareToEqualObject()
        {
            var target = new Money(123.45M, "USD");
            object target1 = new Money(123.45M, "USD");

            Assert.AreEqual(0, target.CompareTo(target1));
        }

        [TestMethod]
        public void TestCompareToGreaterObject()
        {
            var target = new Money(123.45M, "USD");
            object target1 = new Money(123.46M, "USD");

            Assert.IsTrue(target.CompareTo(target1) < 0);
        }

        [TestMethod]
        public void TestCompareToLessObject()
        {
            var target = new Money(123.45M, "USD");
            object target1 = new Money(123.44M, "USD");

            Assert.IsTrue(target.CompareTo(target1) > 0);
        }

        [TestMethod]
        public void TestOperatorLess()
        {
            var targetLeft   = new Money(123.45M, "USD");
            var targetRight1 = new Money(123.45M, "USD");
            var targetRight2 = new Money(123.46M, "USD");
            var targetRight3 = new Money(123.44M, "USD");

            Assert.IsFalse(targetLeft < targetRight1);
            Assert.IsTrue(targetLeft < targetRight2);
            Assert.IsFalse(targetLeft < targetRight3);
        }

        [TestMethod]
        public void TestOperatorGreater()
        {
            var targetLeft   = new Money(123.45M, "USD");
            var targetRight1 = new Money(123.45M, "USD");
            var targetRight2 = new Money(123.46M, "USD");
            var targetRight3 = new Money(123.44M, "USD");

            Assert.IsFalse(targetLeft > targetRight1);
            Assert.IsFalse(targetLeft > targetRight2);
            Assert.IsTrue(targetLeft > targetRight3);
        }

        [TestMethod]
        public void TestOperatorLessOrEqual()
        {
            var targetLeft   = new Money(123.45M, "USD");
            var targetRight1 = new Money(123.45M, "USD");
            var targetRight2 = new Money(123.46M, "USD");
            var targetRight3 = new Money(123.44M, "USD");

            Assert.IsTrue(targetLeft <= targetRight1);
            Assert.IsTrue(targetLeft <= targetRight2);
            Assert.IsFalse(targetLeft <= targetRight3);
        }

        [TestMethod]
        public void TestOperatorGreaterOrEqual()
        {
            var targetLeft   = new Money(123.45M, "USD");
            var targetRight1 = new Money(123.45M, "USD");
            var targetRight2 = new Money(123.46M, "USD");
            var targetRight3 = new Money(123.44M, "USD");

            Assert.IsTrue(targetLeft >= targetRight1);
            Assert.IsFalse(targetLeft >= targetRight2);
            Assert.IsTrue(targetLeft >= targetRight3);
        }

        // ---------------------------------

        [TestMethod]
        public void TestOperationPlus()
        {
            var target = new Money(123.45M, "USD");
            var actual = Money.Plus(target);

            Assert.IsFalse(ReferenceEquals(target, actual));
            Assert.IsTrue(target == actual);
        }

        [TestMethod]
        public void TestOperationNegate()
        {
            var target = new Money(123.45M, "USD");
            var actual = Money.Negate(target);

            Assert.IsFalse(ReferenceEquals(target, actual));
            Assert.IsTrue(target.Amount == -actual.Amount);
            Assert.IsTrue(target.Currency == actual.Currency);
        }

        [TestMethod]
        public void TestOperationAdd()
        {
            var target1 = new Money(123.45M, "USD");
            var target2 = new Money(123.46M, "USD");
            var actual = Money.Add(target1, target2);

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.IsFalse(ReferenceEquals(target2, actual));
            Assert.AreEqual(246.91M, actual.Amount);
            Assert.AreEqual("USD", actual.Currency);
        }

        [TestMethod]
        public void TestOperationSub()
        {
            var target1 = new Money(123.45M, "USD");
            var target2 = new Money(123.46M, "USD");
            var actual = Money.Subtract(target1, target2);

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.IsFalse(ReferenceEquals(target2, actual));
            Assert.AreEqual(-0.01M, actual.Amount);
            Assert.AreEqual("USD", actual.Currency);
        }

        [TestMethod]
        public void TestOperationDivRatio()
        {
            var target1 = new Money(123.45M, "USD");
            var target2 = new Money(123.45M, "USD");
            var actual = Money.Divide(target1, target2);

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.IsFalse(ReferenceEquals(target2, actual));
            Assert.AreEqual(1, actual);
        }

        [TestMethod]
        public void TestOperationDiv()
        {
            var target1 = new Money(123.45M, "USD");
            var actual = Money.Divide(target1, 2);

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.AreEqual(61.72M, actual.Amount);
            Assert.AreEqual("USD", actual.Currency);
        }

        [TestMethod]
        public void TestOperationMod()
        {
            var target1 = new Money(123.45M, "USD");
            var actual = Money.Mod(target1, 2);

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.AreEqual(1.45M, actual.Amount);
            Assert.AreEqual("USD", actual.Currency);
        }
        // ---------------------------------

        [TestMethod]
        public void TestOperatorPlus()
        {
            var target = new Money(123.45M, "USD");
            var actual = +target;

            Assert.IsFalse(ReferenceEquals(target, actual));
            Assert.IsTrue(target == actual);
        }

        [TestMethod]
        public void TestOperatorNegate()
        {
            var target = new Money(123.45M, "USD");
            var actual = -target;

            Assert.IsFalse(ReferenceEquals(target, actual));
            Assert.IsTrue(target.Amount == -actual.Amount);
            Assert.IsTrue(target.Currency == actual.Currency);
        }

        [TestMethod]
        public void TestOperatorAdd()
        {
            var target1 = new Money(123.45M, "USD");
            var target2 = new Money(123.46M, "USD");
            var actual = target1 + target2;

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.IsFalse(ReferenceEquals(target2, actual));
            Assert.AreEqual(246.91M, actual.Amount);
            Assert.AreEqual("USD", actual.Currency);
        }

        [TestMethod]
        public void TestOperatorSub()
        {
            var target1 = new Money(123.45M, "USD");
            var target2 = new Money(123.46M, "USD");
            var actual = target1 - target2;

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.IsFalse(ReferenceEquals(target2, actual));
            Assert.AreEqual(-0.01M, actual.Amount);
            Assert.AreEqual("USD", actual.Currency);
        }

        [TestMethod]
        public void TestOperatorDivRatio()
        {
            var target1 = new Money(123.45M, "USD");
            var target2 = new Money(123.45M, "USD");
            var actual = target1 / target2;

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.IsFalse(ReferenceEquals(target2, actual));
            Assert.AreEqual(1, actual);
        }

        [TestMethod]
        public void TestOperatorDiv()
        {
            var target1 = new Money(123.45M, "USD");
            var actual = target1 / 2;

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.AreEqual(61.72M, actual.Amount);
            Assert.AreEqual("USD", actual.Currency);
        }

        [TestMethod]
        public void TestOperatorMod()
        {
            var target1 = new Money(123.45M, "USD");
            var actual = target1 % 2;

            Assert.IsFalse(ReferenceEquals(target1, actual));
            Assert.AreEqual(1.45M, actual.Amount);
            Assert.AreEqual("USD", actual.Currency);
        }
    }
}
