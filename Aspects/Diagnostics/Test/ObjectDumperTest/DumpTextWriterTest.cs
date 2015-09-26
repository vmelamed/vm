using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics.DumpImplementation;

namespace vm.Aspects.Diagnostics.ObjectDumper.Tests
{
    /// <summary>
    /// Summary description for DumpTextWriterTest
    /// </summary>
    [TestClass]
    public class DumpTextWriterTest
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
        public void WriteBufferNull()
        {
            try
            {
                var w = new DumpTextWriter();

                w.Write(new char[] { 'a', 'b', 'c' }, -1, -2);
                Assert.Fail("Expected System.Diagnostics.Contracts.__ContractsRuntime+ContractException");
            }
            catch (Exception x)
            {
                Assert.AreEqual("System.Diagnostics.Contracts.__ContractsRuntime+ContractException", x.GetType().FullName);
            }
        }
    }
}
