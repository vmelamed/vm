using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
                container
                    // the default messaging pattern is the ConfiguredBindingConfigurator - assumes that the binding is fully configured already.
                    .RegisterTypeIfNot<BindingConfigurator, ConfiguredBindingConfigurator>(registrations)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseConfigurator>(registrations, RequestResponseConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseNoSecurityConfigurator>(registrations, RequestResponseNoSecurityConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, RequestResponseTxConfigurator>(registrations, RequestResponseTxConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, StreamingConfigurator>(registrations, StreamingConfigurator.PatternName)
                    .RegisterTypeIfNot<BindingConfigurator, FireAndForgetConfigurator>(registrations, FireAndForgetConfigurator.PatternName)

                    .RegisterTypeIfNot<Binding, BasicHttpBinding>(registrations, "http", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, WebHttpBinding>(registrations, "http.rest", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, WSHttpBinding>(registrations, "https", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, WebHttpBinding>(registrations, "https.rest", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, NetTcpBinding>(registrations, "net.tcp", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, NetMsmqBinding>(registrations, "net.msmq", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, NetNamedPipeBinding>(registrations, "net.pipe", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, NetTcpBinding>(registrations, "net.tcp.rest", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, NetMsmqBinding>(registrations, "net.msmq.rest", new InjectionConstructor())
                    .RegisterTypeIfNot<Binding, NetNamedPipeBinding>(registrations, "net.pipe.rest", new InjectionConstructor())
                    ;
            }
        }

        static BindingConfigurationsRegistrar _registrar = new BindingConfigurationsRegistrar();

        /// <summary>
        /// Gets the registrar.
        /// </summary>
        public static ContainerRegistrar Registrar
        {
            get
            {
                Contract.Ensures(Contract.Result<ContainerRegistrar>() != null);

                return _registrar;
            }
        }
    }
}
