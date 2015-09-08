using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Tests
{
    /// <summary>
    /// Summary description for IListExtensionsTest
    /// </summary>
    [TestClass]
    public class IListExtensionsTest
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTest()
        {
            List<int> target = null;

            target.InsertSorted(0);
        }

        [TestMethod]
        public void EmptyListTest()
        {
            List<int> target = new List<int>();

            target.InsertSorted(2);

            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(2, target[0]);
        }

        [TestMethod]
        public void OneItemListTest1()
        {
            List<int> target = new List<int> { 5 };

            target.InsertSorted(2);

            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(2, target[0]);
            Assert.AreEqual(5, target[1]);
        }

        [TestMethod]
        public void OneItemListTest2()
        {
            List<int> target = new List<int> { 5 };

            target.InsertSorted(7);

            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(7, target[1]);
        }

        [TestMethod]
        public void TwoItemListTest1()
        {
            List<int> target = new List<int> { 5, 10 };

            target.InsertSorted(2);

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(2, target[0]);
            Assert.AreEqual(5, target[1]);
            Assert.AreEqual(10, target[2]);
        }

        [TestMethod]
        public void TwoItemListTest2()
        {
            List<int> target = new List<int> { 5, 10 };

            target.InsertSorted(7);

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(7, target[1]);
            Assert.AreEqual(10, target[2]);
        }

        [TestMethod]
        public void TwoItemListTest3()
        {
            List<int> target = new List<int> { 5, 10 };

            target.InsertSorted(12);

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(12, target[2]);
        }

        [TestMethod]
        public void TwoItemListTest4()
        {
            List<int> target = new List<int> { 5, 10 };

            target.InsertSorted(5);

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(5, target[1]);
            Assert.AreEqual(10, target[2]);
        }

        [TestMethod]
        public void TwoItemListTest5()
        {
            List<int> target = new List<int> { 5, 10 };

            target.InsertSorted(10);

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
        }

        [TestMethod]
        public void ThreeItemListTest1()
        {
            List<int> target = new List<int> { 5, 10, 15 };

            target.InsertSorted(2);

            Assert.AreEqual(4, target.Count);
            Assert.AreEqual(2, target[0]);
            Assert.AreEqual(5, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(15, target[3]);
        }

        [TestMethod]
        public void ThreeItemListTest2()
        {
            List<int> target = new List<int> { 5, 10, 15 };

            target.InsertSorted(7);

            Assert.AreEqual(4, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(7, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(15, target[3]);
        }

        [TestMethod]
        public void ThreeItemListTest3()
        {
            List<int> target = new List<int> { 5, 10, 15 };

            target.InsertSorted(12);

            Assert.AreEqual(4, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(12, target[2]);
            Assert.AreEqual(15, target[3]);
        }

        [TestMethod]
        public void ThreeItemListTest4()
        {
            List<int> target = new List<int> { 5, 10, 15 };

            target.InsertSorted(17);

            Assert.AreEqual(4, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(15, target[2]);
            Assert.AreEqual(17, target[3]);
        }

        [TestMethod]
        public void ThreeItemListTest5()
        {
            List<int> target = new List<int> { 5, 10, 15 };

            target.InsertSorted(5);

            Assert.AreEqual(4, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(5, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(15, target[3]);
        }

        [TestMethod]
        public void ThreeItemListTest6()
        {
            List<int> target = new List<int> { 5, 10, 15 };

            target.InsertSorted(15);

            Assert.AreEqual(4, target.Count);
            Assert.AreEqual(5, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(15, target[2]);
            Assert.AreEqual(15, target[3]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest1()
        {
            List<int> target = new List<int> { 5, 10, 10, 10, 15 };

            target.InsertSorted(12);

            Assert.AreEqual(6, target.Count);

            Assert.AreEqual(05, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(12, target[4]);
            Assert.AreEqual(15, target[5]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest2()
        {
            List<int> target = new List<int> { 5, 10, 10, 10, 15 };

            target.InsertSorted(3);

            Assert.AreEqual(6, target.Count);

            Assert.AreEqual(03, target[0]);
            Assert.AreEqual(05, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(10, target[4]);
            Assert.AreEqual(15, target[5]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest3()
        {
            List<int> target = new List<int> { 5, 10, 10, 10, 15 };

            target.InsertSorted(30);

            Assert.AreEqual(6, target.Count);

            Assert.AreEqual(05, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(15, target[4]);
            Assert.AreEqual(30, target[5]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest4()
        {
            List<int> target = new List<int> { 5, 10, 10, 10, 15 };

            target.InsertSorted(10);

            Assert.AreEqual(6, target.Count);

            Assert.AreEqual(05, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(10, target[4]);
            Assert.AreEqual(15, target[5]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest5()
        {
            List<int> target = new List<int> { 10, 10, 10, 15 };

            target.InsertSorted(10);

            Assert.AreEqual(5, target.Count);

            Assert.AreEqual(10, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(15, target[4]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest6()
        {
            List<int> target = new List<int> { 5, 10, 10, 10 };

            target.InsertSorted(10);

            Assert.AreEqual(5, target.Count);

            Assert.AreEqual(05, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(10, target[4]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest7()
        {
            List<int> target = new List<int> { 10, 10, 10, 15 };

            target.InsertSorted(10);

            Assert.AreEqual(5, target.Count);

            Assert.AreEqual(10, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(15, target[4]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest8()
        {
            List<int> target = new List<int> { 5, 10, 10, 10 };

            target.InsertSorted(10);

            Assert.AreEqual(5, target.Count);

            Assert.AreEqual(05, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(10, target[4]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest9()
        {
            List<int> target = new List<int> { 10, 10, 10, 10 };

            target.InsertSorted(10);

            Assert.AreEqual(5, target.Count);

            Assert.AreEqual(10, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
            Assert.AreEqual(10, target[4]);
        }

        [TestMethod]
        public void ListWithDuplicatesTest10()
        {
            List<int> target = new List<int> { 10, 10, 10 };

            target.InsertSorted(10);

            Assert.AreEqual(4, target.Count);

            Assert.AreEqual(10, target[0]);
            Assert.AreEqual(10, target[1]);
            Assert.AreEqual(10, target[2]);
            Assert.AreEqual(10, target[3]);
        }
    }
}
