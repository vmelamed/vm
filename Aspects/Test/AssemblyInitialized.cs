using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Validation.Tests
{
    [TestClass]
    public class AssemblyInitialized
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            try
            {
                ClassMetadataRegistrar.RegisterMetadata();
            }
            catch (Exception x)
            {
                context.WriteLine("{0}", x.DumpString());
                throw;
            }
        }
    }
}
