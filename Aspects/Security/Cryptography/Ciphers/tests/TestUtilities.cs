using System;
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

        public static Stream CreateNonReadableStream() => new FileStream("Readme.txt", FileMode.Open, FileAccess.Write, FileShare.ReadWrite);

        public static Stream CreateNonWritableStream() => new MemoryStream(new byte[10], false);

        public static void AsyncTestWrapper(
            TestContext testContext,
            Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

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
