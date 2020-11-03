using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Diagnostics.ObjectDumper.Tests
{
    [TestClass]
    public class DumpUtilitiesTest
    {
        [AssemblyInitialize]
        public static void AssemblyInitializer(TestContext _)
        {
            ClassMetadataRegistrar.RegisterMetadata();
        }

        [TestMethod]
        public void TestGetIndent()
        {
            using var writer = new StreamWriter(new MemoryStream(), Encoding.ASCII);

            Assert.AreEqual("\r\n", writer.Indent(-2).NewLine);
            Assert.AreEqual("\r\n", writer.Indent(-1).NewLine);
            Assert.AreEqual("\r\n", writer.Indent(0).NewLine);
            Assert.AreEqual("\r\n  ", writer.Indent(1).NewLine);
            Assert.AreEqual("\r\n      ", writer.Indent(2).NewLine);
            Assert.AreEqual("\r\n  ", writer.Unindent(2).NewLine);
            Assert.AreEqual("\r\n", writer.Unindent(1).NewLine);
            Assert.AreEqual("\r\n", writer.Unindent(-1).NewLine);
            Assert.AreEqual("\r\n", writer.Unindent(0).NewLine);
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithNullString()
        {
            const string target = null;

            Assert.IsTrue(target.IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithEmptyString()
        {
            const string target = "";

            Assert.IsTrue(target.IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithBlankString()
        {
            const string target = " ";

            Assert.IsTrue(target.IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithNonBlankString1()
        {
            const string target = " abc";

            Assert.IsFalse(target.IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithNonBlankString2()
        {
            const string target = "abc";

            Assert.IsFalse(target.IsNullOrWhiteSpace());
        }
    }
}
