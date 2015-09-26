using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public static class TestUtilities
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            ClassMetadataRegistrar.RegisterMetadata();
        }

        public static Stream CreateNonReadableStream()
        {
            return new FileStream("Readme.txt", FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
        }

        public static Stream CreateNonWritableStream()
        {
            return new MemoryStream(new byte[10], false);
        }

        public static void AsyncTestWrapper(
            TestContext testContext,
            Action action)
        {
            Contract.Requires<ArgumentNullException>(action != null, "action");

            try
            {
                action();
            }
            catch (AggregateException x)
            {
                testContext.WriteLine("{0}", x.DumpString());
                throw x.InnerExceptions.First();
            }
        }
    }
}
