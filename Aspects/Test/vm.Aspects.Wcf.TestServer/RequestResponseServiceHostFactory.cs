using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.ServicePolicies;
using vm.Aspects.Wcf.Services;

namespace vm.Aspects.Wcf.TestServer
{
    public class RequestResponseServiceHostFactory : MessagingPatternServiceHostFactory<IRequestResponse, RequestResponseService>
    {
        AddressBinding[] _addressesAndBindings;

        public RequestResponseServiceHostFactory(
            AddressBinding[] addressesAndBindings,
            string messagingPattern = null)
            : base(messagingPattern)
        {
            _addressesAndBindings = addressesAndBindings;
        }

        public RequestResponseServiceHostFactory(
            AddressBinding[] addressesAndBindings,
            Claim identityClaim,
            string messagingPattern)
            : base(identityClaim, messagingPattern)
        {
            _addressesAndBindings = addressesAndBindings;
        }

        public RequestResponseServiceHostFactory(
            AddressBinding[] addressesAndBindings,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern)
            : base(identityType, certificate, messagingPattern)
        {
            _addressesAndBindings = addressesAndBindings;
        }

        public RequestResponseServiceHostFactory(
            AddressBinding[] addressesAndBindings,
            ServiceIdentity identityType,
            string identity = null,
            string messagingPattern = null)
            : base(identityType, identity, messagingPattern)
        {
            _addressesAndBindings = addressesAndBindings;
        }

        protected override IUnityContainer DoRegisterDefaults(
            IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations)
        {
            ClassMetadataRegistrar
                .RegisterMetadata()
                .Register<WebException, WebExceptionDumpMetadata>()
                ;

            DumpFormat.Delegate         = "{4}{0}.{3}";
            DumpFormat.Type             = "{0}";
            DumpFormat.SequenceTypeName = "";
            DumpFormat.TypeInfo         = "{1}.{0}";

            var interception = container
                                    .AddNewExtension<Interception>()
                                    .Configure<Interception>()
                                    ;

            if (!registrations.ContainsKey(new RegistrationLookup(typeof(InjectionPolicy), "ServicePolicy")))
                interception
                    .AddPolicy("ServicePolicy")
                    .AddMatchingRule<TagAttributeMatchingRule>(
                            new InjectionConstructor("ServicePolicy", false))

                    .AddCallHandler<ServiceCallTraceCallHandler>(
                            new ContainerControlledLifetimeManager(),
                            new InjectionConstructor(Facility.LogWriter))

                    .AddCallHandler<ServiceParameterValidatingCallHandler>(
                            new ContainerControlledLifetimeManager(),
                            new InjectionConstructor())
                    ;

            return container;
        }

        protected override ServiceHost AddEndpoints(
            ServiceHost host)
        {
            foreach (var ab in _addressesAndBindings)
                host.AddServiceEndpoint(
                        typeof(IRequestResponse),
                        ab.BindingFactory(),
                        ab.Address);

            return base.AddEndpoints(host);
        }
    }
}
