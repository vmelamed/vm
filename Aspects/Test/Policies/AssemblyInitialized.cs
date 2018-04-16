using System;
using System.Configuration;
using System.Globalization;
using System.IO;

using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
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
                ClassMetadataRegistrar
                    .RegisterMetadata()
                    .Register<ArgumentValidationException, ArgumentValidationExceptionDumpMetadata>()
                    .Register<ValidationResult, ValidationResultDumpMetadata>()
                    .Register<ValidationResults, ValidationResultsDumpMetadata>()
                    .Register<ConfigurationErrorsException, ConfigurationErrorsExceptionDumpMetadata>()
                    ;

                DIContainer
                    .Initialize();

                lock (DIContainer.Root)
                {
                    var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                    DIContainer
                        .Root
                        .UnsafeRegister(Facility.Registrar, registrations, true)
                        ;

                    // add AOP policies
                    DIContainer.Root.Configure<Interception>()
                        .AddPolicy(BaseTestCalls.Track)
                        .AddMatchingRule<TagAttributeMatchingRule>(
                                            new InjectionConstructor(BaseTestCalls.Track, true))
                        .AddCallHandler<TrackCallHandler>(
                                            new ContainerControlledLifetimeManager())
                        ;

                    DIContainer.Root.Configure<Interception>()
                        .AddPolicy(BaseTestCalls.Trace)
                        .AddMatchingRule<TagAttributeMatchingRule>(
                                            new InjectionConstructor(BaseTestCalls.Trace, true))
                        .AddCallHandler<CallTraceCallHandler>(
                                            new InjectionConstructor(Facility.LogWriter))
                        ;

                    using (var writer = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        DIContainer.Root.Dump(writer);
                        context.WriteLine(
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
