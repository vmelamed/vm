using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Unity;
using Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects;
using vm.Aspects.Facilities;

namespace vm.Aspects.Policies.Tests
{
    [TestClass]
    public class AssemblyInitialized
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            try
            {
                DIContainer.Initialize();

                lock (DIContainer.Root)
                {
                    var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                    DIContainer
                        .Root
                        .UnsafeRegister(Facility.Registrar, registrations, true)
                        ;

                    // enable interception and policy injection (AOP)
                    DIContainer
                        .Root
                        .Configure<Interception>()
                        .AddPolicy("testCallHandler")
                            .AddMatchingRule<TagAttributeMatchingRule>(
                                                new InjectionConstructor("testCallHandler", false))

                            .AddCallHandler<CallTraceCallHandler>(
                                    new ContainerControlledLifetimeManager())
                            ;

                    using (var writer = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        DIContainer.Root.Dump(writer);
                        Debug.Print(
$@"
Container registrations:
===============================
{writer.GetStringBuilder()}
===============================
");
                    }
                }
            }
            catch (Exception x)
            {
                context.WriteLine("{0}", x.DumpString());
                throw;
            }
        }
    }
}
