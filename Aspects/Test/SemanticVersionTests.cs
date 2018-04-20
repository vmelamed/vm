using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Summary description for SemanticVersion
    /// </summary>
    [TestClass]
    [DeploymentItem("..\\..\\SemVerParseTestData.csv")]
    [DeploymentItem("..\\..\\SemVerCompareTestData.csv")]
    public class SemanticVersionTests
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

        [TestMethod]
        public void TestConstructorWith3Parameters()
        {
            var target = new SemanticVersion(1, 2, 3);

            Assert.AreEqual("1.2.3", target.ToString());
        }

        [TestMethod]
        public void TestConstructorWith4Parameters()
        {
            var target = new SemanticVersion(1, 2, 3, "beta.1");

            Assert.AreEqual("1.2.3-beta.1", target.ToString());
        }

        [TestMethod]
        public void TestConstructorWith5Parameters()
        {
            var target = new SemanticVersion(1, 2, 3, "beta.1", "build.dev");

            Assert.AreEqual("1.2.3-beta.1+build.dev", target.ToString());
        }

        [DataSource(
            "Microsoft.VisualStudio.TestTools.DataSource.CSV",
            ".\\SemVerParseTestData.csv",
            "SemVerParseTestData#csv",
            DataAccessMethod.Sequential)]
        [TestMethod]
        public void TestParseVersion()
        {
            var row = TestContext.DataRow;

            if (row[0].ToString().StartsWith("#"))
                return;

            TestParseVersionParameterized(
                Convert.ToString(row["version"]),
                Convert.ToBoolean(row["success"]),
                Convert.ToInt32(row["major"]),
                Convert.ToInt32(row["minor"]),
                Convert.ToInt32(row["patch"]),
                Convert.ToString(row["prerelease"]),
                Convert.ToString(row["build"]));
        }

        void TestParseVersionParameterized(
            string version,
            bool successfulParse,
            int major,
            int minor,
            int patch,
            string prerelease,
            string build)
        {
            var success = SemanticVersion.TryParse(version, out var target);

            Assert.AreEqual(successfulParse, success);

            if (!success)
                return;

            Assert.AreEqual(major, target.Major);
            Assert.AreEqual(minor, target.Minor);
            Assert.AreEqual(patch, target.Patch);
            Assert.AreEqual(prerelease, target.Prerelease);
            Assert.AreEqual(build, target.Build);
        }

        [DataSource(
            "Microsoft.VisualStudio.TestTools.DataSource.CSV",
            ".\\SemVerCompareTestData.csv",
            "SemVerCompareTestData#csv",
            DataAccessMethod.Sequential)]
        [TestMethod]
        public void TestCompareVersion()
        {
            var row = TestContext.DataRow;

            if (row[0].ToString().StartsWith("#"))
                return;

            TestCompareVersionParameterized(
                Convert.ToString(row["left"]),
                Convert.ToString(row["right"]),
                Convert.ToBoolean(row["isGreater"]),
                Convert.ToBoolean(row["isLess"]),
                Convert.ToBoolean(row["isEqual"]));
        }

        void TestCompareVersionParameterized(
            string left,
            string right,
            bool isGreater,
            bool isLess,
            bool isEqual)
        {

            SemanticVersion.TryParse(left, out var leftVersion);
            SemanticVersion.TryParse(right, out var rightVersion);

            var result = leftVersion.CompareTo(rightVersion);

            if (isGreater)
            {
                Assert.IsFalse(leftVersion.Equals(rightVersion));
                Assert.IsTrue(result > 0);
                Assert.IsTrue(leftVersion > rightVersion);
                Assert.IsFalse(leftVersion < rightVersion);
                Assert.IsFalse(leftVersion == rightVersion);
            }

            if (isLess)
            {
                Assert.IsFalse(leftVersion.Equals(rightVersion));
                Assert.IsTrue(result < 0);
                Assert.IsTrue(leftVersion < rightVersion);
                Assert.IsFalse(leftVersion > rightVersion);
                Assert.IsFalse(leftVersion == rightVersion);
            }

            if (isEqual)
            {
                Assert.IsTrue(leftVersion.Equals(rightVersion));
                Assert.IsTrue(result == 0);
                Assert.IsFalse(leftVersion < rightVersion);
                Assert.IsFalse(leftVersion > rightVersion);
                Assert.IsTrue(leftVersion == rightVersion);
            }
        }
    }
}
