﻿using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Diagnostics.Tests.ObjectDumper
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

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithNullString()
        {
            string target = null;

            Assert.IsTrue(target.IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithEmptyString()
        {
            string target = "";

            Assert.IsTrue(target.IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithBlankString()
        {
            string target = " ";

            Assert.IsTrue(target.IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithNonBlankString1()
        {
            string target = " abc";

            Assert.IsFalse(target.IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpaceExtensionWithNonBlankString2()
        {
            string target = "abc";

            Assert.IsFalse(target.IsNullOrWhiteSpace());
        }
    }
}
