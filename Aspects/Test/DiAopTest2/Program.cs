using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using CommonServiceLocator;

using Microsoft.Practices.EnterpriseLibrary.Validation;

using Unity;
using Unity.Injection;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.PolicyInjection.MatchingRules;
using Unity.Lifetime;

using vm.Aspects;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Policies;

namespace DiAopTest2
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeContainer();

            var tracker = ServiceLocator.Current.GetInstance<ITestCalls>(BaseTestCalls.Track);

            tracker.Test1();

            Debug.WriteLine(Facility.LogWriter.GetTestLogText());
        }

        static void InitializeContainer()
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

                var container = DIContainer.Initialize();

                lock (container)
                {
                    var registrations = container.GetRegistrationsSnapshot();
                    var interception = container.Configure<Interception>();

                    // add AOP policies
                    interception
                        // Track policy
                        .AddPolicy(BaseTestCalls.Track)
                        .AddMatchingRule<TagAttributeMatchingRule>(
                                            new InjectionConstructor(BaseTestCalls.Track, false))
                        .AddCallHandler<TrackCallHandler>(
                                            new ContainerControlledLifetimeManager())
                        ;

                    interception
                        // Trace policy
                        .AddPolicy(BaseTestCalls.Trace)
                        .AddMatchingRule<TagAttributeMatchingRule>(
                                            new InjectionConstructor(BaseTestCalls.Trace, false))
                        .AddCallHandler<CallTraceCallHandler>()
                        ;

                    // register types to test
                    container

                        // register the facilities
                        .UnsafeRegister(Facility.Registrar, registrations)

                        // object with track policy
                        .RegisterTypeIfNot<ITestCalls, TrackTestCalls>(
                            registrations,
                            BaseTestCalls.Track,
                            DIContainer.PolicyInjection())

                        // object with trace policy
                        .RegisterTypeIfNot<ITestCalls, TraceTestCalls>(
                            registrations,
                            BaseTestCalls.Trace,
                            DIContainer.PolicyInjection())
                        ;

                    // dump the container
                    using (var writer = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        DIContainer.Root.Dump(writer);
                        Debug.WriteLine(
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
                Debug.WriteLine(x.DumpString());
                Console.WriteLine(x.DumpString());
                throw;
            }
        }
    }
}
