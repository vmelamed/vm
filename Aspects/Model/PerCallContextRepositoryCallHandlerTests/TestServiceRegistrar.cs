using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Policies;
using vm.Aspects.Wcf.ServicePolicies;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public partial class TestService
    {
        public const string PolicyName = "TestServicePolicy";

        public static ContainerRegistrar Registrar { get; } = new ServiceRegistrar();

        class ServiceRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                InitializeObjectDumper();

                container
                    .UnsafeRegister(TestRepository.Registrar, registrations)
                    ;

                var interception = container.Configure<Interception>();

                if (!registrations.ContainsKey(new RegistrationLookup(typeof(InjectionPolicy), PolicyName)))
                {
                    interception
                        .AddPolicy(PolicyName)
                        .AddMatchingRule<TagAttributeMatchingRule>(new InjectionConstructor(PolicyName, false))

                        .AddCallHandler<ActivityTracerCallHandler>(
                                            new ContainerControlledLifetimeManager())
                        .AddCallHandler<ServiceExceptionHandlingCallHandler>(
                                            new ContainerControlledLifetimeManager(),
                                            new InjectionProperty(
                                                nameof(ServiceExceptionHandlingCallHandler.ExceptionHandlingPolicyName),
                                                ServiceFaultFromExceptionHandlingPolicies.PolicyName))
                                                //ExceptionPolicyProvider.LogAndSwallowPolicyName))
                        .AddCallHandler<ServiceCallTraceCallHandler>(
                                            new ContainerControlledLifetimeManager(),
                                            new InjectionConstructor(Facility.LogWriter))
                        .AddCallHandler<ServiceParameterValidatingCallHandler>(
                                            new ContainerControlledLifetimeManager(),
                                            new InjectionConstructor())
                        .AddCallHandler<UnitOfWorkCallHandler>(
                                            new ContainerControlledLifetimeManager())
                        ;
                }
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
