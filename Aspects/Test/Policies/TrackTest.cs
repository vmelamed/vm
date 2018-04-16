using System;
using System.Threading.Tasks;

using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using vm.Aspects.Facilities;

namespace vm.Aspects.Policies.Tests
{
    /// <summary>
    /// Summary description for TrackTest
    /// </summary>
    [TestClass]
    public class TrackTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        /// <summary>
        /// ClassInitialize runs code before running the first test in the class.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            lock (DIContainer.Root)
            {
                var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                DIContainer
                    .Root
                    .RegisterTypeIfNot<ITestCalls, TrackTestCalls>(
                        registrations,
                        BaseTestCalls.Track,
                        DIContainer.PolicyInjection())

                    .RegisterTypeIfNot<ITestCalls, TraceTestCalls>(
                        registrations,
                        BaseTestCalls.Trace,
                        DIContainer.PolicyInjection())
                    ;
            }
        }

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

        void RunTest(
            Action<ITestCalls> test,
            string resolveName,
            string expected)
        {
            var target = DIContainer.Root.Resolve<ITestCalls>(resolveName);

            test(target);

            var actual = string.Join("\r\n", TestTraceListener.Messages);

            TestTraceListener.Reset();
            Assert.AreEqual(expected, actual);
        }

        async Task RunTest(
            Func<ITestCalls, Task> test,
            string resolveName,
            string expected)
        {
            await test(ServiceLocator.Current.GetInstance<ITestCalls>(resolveName));

            var actual = string.Join("\r\n", TestTraceListener.Messages);

            TestTraceListener.Reset();
            TestContext.WriteLine($"Actual: <{actual}>\nExpected: <{expected}>");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TrackTest11()
        {
            RunTest(t => t.Test1(), BaseTestCalls.Track, @"");
        }

        [TestMethod]
        public void TrackTest12()
        {
            RunTest(t => t.Test2(), BaseTestCalls.Track, @"");
        }

        [TestMethod]
        public void TrackTest13()
        {
            RunTest(t => t.Test3("13"), BaseTestCalls.Track, @"");
        }

        [TestMethod]
        public void TrackTest21()
        {
            RunTest(t => t.AsyncTest1().Wait(), BaseTestCalls.Track, @"");
        }

        [TestMethod]
        public void TrackTest22()
        {
            RunTest(t => t.AsyncTest2().Wait(), BaseTestCalls.Track, @"");
        }

        [TestMethod]
        public void TrackTest23()
        {
            RunTest(t => t.AsyncTest3("23").Wait(), BaseTestCalls.Track, @"");
        }
    }
}
