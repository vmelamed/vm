using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Diagnostics.ObjectDumper.Test
{
    [TestClass]
    public class DumpUtilitiesTest
    {
        [AssemblyInitialize]
        public static void AssemblyInitializer(TestContext testContext)
        {
            ClassMetadataRegistrar.RegisterMetadata();
        }

        [TestMethod]
        public void TestGetIndent()
        {
            using (var writer = new StreamWriter(new MemoryStream(), Encoding.ASCII))
            {
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
        }
    }
}
