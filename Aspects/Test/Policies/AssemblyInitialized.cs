using System;
using System.Configuration;

using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Unity;
using Unity.Injection;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.PolicyInjection.MatchingRules;
using Unity.Lifetime;

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
