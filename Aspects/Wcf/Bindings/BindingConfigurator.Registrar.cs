using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.Practices.Unity;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class BindingConfigurator.
    /// </summary>
    public abstract partial class BindingConfigurator
    {
        /// <summary>
        /// Class BindingConfigurationsRegistrar. Registers the existing binding configurators.
        /// </summary>
        class BindingConfigurationsRegistrar : ContainerRegistrar
        {
            /// <summary>
            /// Does the actual work of the registration.
            /// The method is called from a synchronized context, i.e. does not need to be thread safe.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                container
                    // the default messaging pattern is the ConfiguredBindingConfigurator - assumes that the binding is fully configured already.
                    .RegisterTypeIfNot<BindingConfigurator, ConfiguredBindingConfigurator>(registrations)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseConfigurator>(registrations, RequestResponseConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseNoSecurityConfigurator>(registrations, RequestResponseNoSecurityConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseBasicAuthenticationConfigurator>(registrations, RequestResponseBasicAuthenticationConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseTransportConfigurator>(registrations, RequestResponseTransportConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseTransportClientWindowsAuthenticationConfigurator>(registrations, RequestResponseTransportClientWindowsAuthenticationConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseTransportClientCertificateAuthenticationConfigurator>(registrations, RequestResponseTransportClientCertificateAuthenticationConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseMessageConfigurator>(registrations, RequestResponseMessageConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseMessageClientWindowsAuthenticationConfigurator>(registrations, RequestResponseMessageClientWindowsAuthenticationConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseMessageClientCertificateAuthenticationConfigurator>(registrations, RequestResponseMessageClientCertificateAuthenticationConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseTxConfigurator>(registrations, RequestResponseTxConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, StreamingConfigurator>(registrations, StreamingConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, StreamingNoSecurityConfigurator>(registrations, StreamingNoSecurityConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, FireAndForgetConfigurator>(registrations, FireAndForgetConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, FireAndForgetNoSecurityConfigurator>(registrations, FireAndForgetNoSecurityConfigurator.PatternName)

                    .RegisterTypeIfNot<Binding, WSHttpBinding>(registrations, Uri.UriSchemeHttp, new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, WSHttpBinding>(registrations, Uri.UriSchemeHttps, new InjectionConstructor())

                    .RegisterTypeIfNot<Binding, BasicHttpBinding>(registrations, Uri.UriSchemeHttp+Constants.BasicHttpSchemeSuffix, new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, BasicHttpsBinding>(registrations, Uri.UriSchemeHttps+Constants.BasicHttpSchemeSuffix, new InjectionConstructor())

                    .RegisterTypeIfNot<Binding, WebHttpBinding>(registrations, Uri.UriSchemeHttp+Constants.RestfulSchemeSuffix, new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, WebHttpBinding>(registrations, Uri.UriSchemeHttps+Constants.RestfulSchemeSuffix, new InjectionConstructor())

                    .RegisterTypeIfNot<Binding, NetTcpBinding>(registrations, Uri.UriSchemeNetTcp, new InjectionConstructor())

                    .RegisterTypeIfNot<Binding, NetMsmqBinding>(registrations, Constants.UriSchemeNetMsmq, new InjectionConstructor())

                    .RegisterTypeIfNot<Binding, NetNamedPipeBinding>(registrations, Uri.UriSchemeNetPipe, new InjectionConstructor())
                    ;
            }
        }

        /// <summary>
        /// Gets the registrar.
        /// </summary>
        public static ContainerRegistrar Registrar { get; } = new BindingConfigurationsRegistrar();
    }
}
