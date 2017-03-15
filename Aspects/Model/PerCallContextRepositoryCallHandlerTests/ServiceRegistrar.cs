using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Wcf.ServicePolicies;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public partial class Service
    {
        public const string PolicyName = "ServicePolicy";

        public static ContainerRegistrar Registrar { get; } = new ServiceRegistrar();

        class ServiceRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                InitializeObjectDumper();
                container
                    .UnsafeRegister(TestRepository.Registrar, registrations);

                var interception = container
                                        .UnsafeRegister(TestRepository.Registrar, registrations)
                                        .AddNewExtension<Interception>()
                                        .Configure<Interception>()
                                        ;

                interception
                    .AddPolicy(nameof(Service))
                    .AddMatchingRule<TagAttributeMatchingRule>(
                            new InjectionConstructor(nameof(Service), false))

                    .AddCallHandler<PerCallContextRepositoryCallHandler>()
                    ;

                if (!registrations.ContainsKey(new RegistrationLookup(typeof(InjectionPolicy), PolicyName)))
                    interception
                        .AddPolicy(PolicyName)
                        .AddMatchingRule<TagAttributeMatchingRule>(
                                new InjectionConstructor(PolicyName, false))

                        .AddCallHandler<ServiceExceptionHandlingCallHandler>(new ContainerControlledLifetimeManager())
                        .AddCallHandler<ServiceCallTraceCallHandler>(new ContainerControlledLifetimeManager())
                        .AddCallHandler<ServiceParameterValidatingCallHandler>(new ContainerControlledLifetimeManager())
                        .AddCallHandler<PerCallContextRepositoryCallHandler>()
                        ;
            }

            static void InitializeObjectDumper()
            {
                // initialize the ObjectDumper
                ClassMetadataRegistrar
                    .RegisterMetadata()
                    .Register<SqlException, SqlExceptionDumpMetadata>()
                    .Register<SqlError, SqlErrorDumpMetadata>()
                    .Register<WebException, WebExceptionDumpMetadata>()
                    ;

                DumpFormat.Delegate         = "{4}{0}.{3}";
                DumpFormat.Type             = "{0}";
                DumpFormat.SequenceTypeName = "";
                DumpFormat.TypeInfo         = "{1}.{0}";
            }

        }
    }
}
