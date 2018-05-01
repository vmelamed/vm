using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CommonServiceLocator;

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
                    ;

                DIContainer
                    .Root
                    .RegisterTypeIfNot<ITestCalls, TraceTestCalls>(
                        registrations,
                        BaseTestCalls.Trace,
                        DIContainer.PolicyInjection())
                    ;


                using (var writer = new StringWriter(CultureInfo.InvariantCulture))
                {
                    DIContainer.Root.Dump(writer);
                    testContext.WriteLine(
$@"
Container registrations:
===============================
{writer.GetStringBuilder()}
===============================
");
                }
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
            string regexExpected)
        {
            string actual;

            try
            {
                var target = ServiceLocator.Current.GetInstance<ITestCalls>(resolveName);

                test(target);

                actual = string.Join("\r\n", TestTraceListener.Messages);

                TestTraceListener.Reset();
                TestContext.WriteLine($"Actual: <{actual}>\nRegex expected: <{regexExpected}>");
                Assert.IsTrue(Regex.IsMatch(actual, regexExpected), $"The actual result does not match the regular expression for the expected result.");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch
            {
                actual = string.Join("\r\n", TestTraceListener.Messages);

                TestTraceListener.Reset();
                TestContext.WriteLine($"Actual when throwing exception: <{actual}>\n");
                throw;
            }
        }

        async Task RunTest(
            Func<ITestCalls, Task> test,
            string resolveName,
            string regexExpected)
        {
            string actual;

            try
            {
                var target = ServiceLocator.Current.GetInstance<ITestCalls>(resolveName);

                await test(target);

                actual = string.Join("\r\n", TestTraceListener.Messages);

                TestTraceListener.Reset();
                TestContext.WriteLine($"Actual: <{actual}>\nRegex expected: <{regexExpected}>");
                Assert.IsTrue(Regex.IsMatch(actual, regexExpected), $"The actual result does not match the regular expression for the expected result.");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch
            {
                actual = string.Join("\r\n", TestTraceListener.Messages);

                TestTraceListener.Reset();
                TestContext.WriteLine($"Actual when throwing exception: <{actual}>\n");
                throw;
            }
        }

        [TestMethod]
        public void TrackTest11()
        {
            RunTest(t => t.Test1(), BaseTestCalls.Track, string.Format(TrackTestRegexTemplate, "1"));
        }

        [TestMethod]
        public void TrackTest12()
        {
            RunTest(t => t.Test2(), BaseTestCalls.Track, string.Format(TrackTestRegexTemplate, "2"));
        }

        [TestMethod]
        public void TrackTest13()
        {
            RunTest(t => t.Test3("13"), BaseTestCalls.Track, string.Format(TrackTestRegexTemplate, "3"));
        }



        [TestMethod]
        public void TrackTest21()
        {
            try
            {
                RunTest(t => t.AsyncTest1(), BaseTestCalls.Track, string.Format(TrackAsyncTestRegexTemplate, "1", "")).Wait();
            }
            catch (AggregateException ax)
            {
                if (ax.InnerExceptions.Count == 1)
                    throw ax.InnerExceptions[0];

                throw;
            }
        }

        [TestMethod]
        public void TrackTest22()
        {
            try
            {
                RunTest(t => t.AsyncTest2(), BaseTestCalls.Track, string.Format(TrackAsyncTestRegexTemplate, "2", "<TResult>")).Wait();
            }
            catch (AggregateException ax)
            {
                if (ax.InnerExceptions.Count == 1)
                    throw ax.InnerExceptions[0];

                throw;
            }
        }

        [TestMethod]
        public void TrackTest23()
        {
            try
            {
                RunTest(t => t.AsyncTest3("23"), BaseTestCalls.Track, string.Format(TrackAsyncTestRegexTemplate, "3", "<TResult>")).Wait();
            }
            catch (AggregateException ax)
            {
                if (ax.InnerExceptions.Count == 1)
                    throw ax.InnerExceptions[0];

                throw;
            }
        }

        [TestMethod]
        public void TraceTest11()
        {
            RunTest(t => t.Test1(), BaseTestCalls.Trace, string.Format(TraceTestRegexTemplate, "", "1", "", ""));
        }

        [TestMethod]
        public void TraceTest12()
        {
            RunTest(t => t.Test2(), BaseTestCalls.Trace, string.Format(TraceTestRegexTemplate, "", "2", "", "\r\n  RETURN VALUE: 12"));
        }

        [TestMethod]
        public void TraceTest13()
        {
            RunTest(t => t.Test3("13"), BaseTestCalls.Trace, string.Format(TraceTestRegexTemplate, "", "3", "\r\n      String i = 13", "\r\n  RETURN VALUE: 13"));
        }

        [TestMethod]
        public void TraceTest21()
        {
            try
            {
                RunTest(t => t.AsyncTest1(), BaseTestCalls.Trace, string.Format(TraceTestRegexTemplate, "Async", "1", "", "")).Wait();
            }
            catch (AggregateException ax)
            {
                if (ax.InnerExceptions.Count == 1)
                    throw ax.InnerExceptions[0];

                throw;
            }
        }

        [TestMethod]
        public void TraceTest22()
        {
            try
            {
                RunTest(t => t.AsyncTest2(), BaseTestCalls.Trace, string.Format(TraceTestRegexTemplate, "Async", "2", "", "\r\n  RETURN VALUE: 22")).Wait();
            }
            catch (AggregateException ax)
            {
                if (ax.InnerExceptions.Count == 1)
                    throw ax.InnerExceptions[0];

                throw;
            }
        }

        [TestMethod]
        public void TraceTest23()
        {
            try
            {
                RunTest(t => t.AsyncTest3("23"), BaseTestCalls.Trace, string.Format(TraceTestRegexTemplate, "Async", "3", "\r\n      String i = 23", "\r\n  RETURN VALUE: 23")).Wait();
            }
            catch (AggregateException ax)
            {
                if (ax.InnerExceptions.Count == 1)
                    throw ax.InnerExceptions[0];

                throw;
            }
        }

        const string TrackTestRegexTemplate = @"Trace Information: 0 : Timestamp: .+
Message: Test{0}: Prepare
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
Trace Information: 0 : Timestamp: .+
Message: Test{0}: Pre-invoke
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
Trace Information: 0 : Timestamp: .+
Message: Test{0}: Do-invoke
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
Trace Information: 0 : Timestamp: .+
Message: Test{0}: Post-invoke
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
";
        const string TrackAsyncTestRegexTemplate = @"Trace Information: 0 : Timestamp: .+
Message: AsyncTest{0}: Prepare
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
Trace Information: 0 : Timestamp: .+
Message: AsyncTest{0}: Pre-invoke
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
Trace Information: 0 : Timestamp: .+
Message: AsyncTest{0}: Do-invoke
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
Trace Information: 0 : Timestamp: .+
Message: AsyncTest{0}: ContinueWith
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
Trace Information: 0 : Timestamp: .+
Message: AsyncTest{0}: Post-invoke
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
Trace Information: 0 : Timestamp: .+
Message: AsyncTest{0}: Task{1} DoContinueWith
Category: Trace
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: .+
Process Name: .+
Thread Name: 
Win32 ThreadId:.+
Extended Properties: 
";
        const string TraceTestRegexTemplate = @"Call End Information: \d+ : Timestamp: .+
Message: 
  Caller Identity: 
    GenericIdentity \(System\.Security\.Principal\.GenericIdentity, mscorlib, Version=[\d\.]+, Culture=neutral, PublicKeyToken=[a-fA-F0-9]+\): 
      AuthenticationType       = 
      Name                     = 
      IsAuthenticated          = False
      Claims                   = <get_Claims>d__51\[\]: \(System\.Security\.Claims\.ClaimsIdentity\+<get_Claims>d__51, mscorlib, Version=[\d\.]+, Culture=neutral, PublicKeyToken=[a-fA-F0-9]+\)
        Claim \(System.Security.Claims.Claim, mscorlib, Version=[\d\.]+, Culture=neutral, PublicKeyToken=[a-fA-F0-9]+\): 
          Type                     = http://schemas\.xmlsoap\.org/ws/2005/05/identity/claims/name
          Value                    = 
          ValueType                = http://www\.w3\.org/2001/XMLSchema#string
      ExternalClaims           = Collection<IEnumerable<Claim>>\[0\]: \(System\.Collections\.ObjectModel\.Collection`1\[\[System\.Collections\.Generic\.IEnumerable`1\[\[System\.Security\.Claims\.Claim, mscorlib, Version=[\d\.]+, Culture=neutral, PublicKeyToken=[a-fA-F0-9]+\]\], mscorlib, Version=[\d\.]+, Culture=neutral, PublicKeyToken=[a-fA-F0-9]+\]\], mscorlib, Version=[\d\.]+, Culture=neutral, PublicKeyToken=[a-fA-F0-9]+\)
  TraceTestCalls.{0}Test{1}\({2}\);{3}
  Call duration: [\.0-9]+

Category: Call End
Priority: -1
EventId: 0
Severity: Information
Title:
Machine: .+
App Domain: UnitTestAdapter: Running test
ProcessId: \d+
Process Name: .+
Thread Name: 
Win32 ThreadId:\d+
Extended Properties: 
";
    }
}
