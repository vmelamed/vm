using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using CommonServiceLocator;
using vm.Aspects.Wcf.Behaviors;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Wcf.Services
{
    /// <summary>
    /// Enum MetadataFeatures lists service metadata features.
    /// </summary>
    [Flags]
    public enum MetadataFeatures
    {
        /// <summary>
        /// No metadata behaviors
        /// </summary>
        None = 0,
        /// <summary>
        /// Generate a help page
        /// </summary>
        HelpPage = 1,
        /// <summary>
        /// Generate a help page for WebHttpBinding endpoints.
        /// </summary>
        WebHttpHelpPage = 2,
        /// <summary>
        /// GET WSDL behavior
        /// </summary>
        Wsdl = 4,
        /// <summary>
        /// IMetadataBehavior
        /// </summary>
        Mex = 8,
        /// <summary>
        /// All of the above
        /// </summary>
        All = 15,
    }

    /// <summary>
    /// Class ServiceHostExtensions. Adds several extension methods to <see cref="ServiceHost"/> for easier configuration.
    /// </summary>
    public static class ServiceHostExtensions
    {
        /// <summary>
        /// Gets the existing or add a new service behavior.
        /// </summary>
        /// <typeparam name="T">The type of the behavior.</typeparam>
        /// <param name="host">The host.</param>
        /// <returns>The behavior.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static T GetOrAddServiceBehavior<T>(
            this ServiceHost host) where T : IServiceBehavior, new()
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var behavior = host.Description.Behaviors.Find<T>();

            if (behavior == null)
            {
                behavior = new T();
                host.Description.Behaviors.Add(behavior);
            }

            return behavior;
        }

        /// <summary>
        /// Sets the service identity.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="identity">The endpoint identity.</param>
        /// <returns>ServiceHost.</returns>
        public static ServiceHost SetServiceEndpointIdentity(
            this ServiceHost host,
            EndpointIdentity identity)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (identity != null)
                foreach (var ep in host.Description.Endpoints)
                    ep.Address = new EndpointAddress(
                                        ep.Address.Uri,
                                        identity,
                                        ep.Address.Headers,
                                        ep.Address.GetReaderAtMetadata(),
                                        ep.Address.GetReaderAtExtensions());

            return host;
        }

        /// <summary>
        /// Sets the service identity.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="certificate">The certificate.</param>
        /// <returns>ServiceHost.</returns>
        public static ServiceHost SetServiceCredentials(
            this ServiceHost host,
            X509Certificate2 certificate)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            host.Credentials.ServiceCertificate.Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            return host;
        }

        /// <summary>
        /// Configures the bindings of a service's endpoints for particular messaging pattern.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="serviceContractType">Type of the service contract.</param>
        /// <param name="messagingPattern">The messaging pattern.</param>
        /// <returns>ServiceHost.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static ServiceHost ConfigureBindings(
            this ServiceHost host,
            Type serviceContractType,
            string messagingPattern)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (serviceContractType == null)
                throw new ArgumentNullException(nameof(serviceContractType));

            // if we still don't know the pattern - try to get it from the service type.
            if (string.IsNullOrWhiteSpace(messagingPattern))
                messagingPattern = serviceContractType.GetMessagingPattern();

            // resolve the binding configurator
            var configurator = ServiceLocator.Current.GetInstance<BindingConfigurator>(messagingPattern);

            // for each endpoint configure the binding according to the messaging pattern.
            foreach (var ep in host.Description
                                    .Endpoints
                                    .Where(ep => ep.Contract.ContractType == serviceContractType))
            {
                ep.Binding = configurator.Configure(ep.Binding);

                // add the new streaming behavior if necessary
                if (ep.Binding.IsStreaming())
                    ep.EndpointBehaviors.Add(new DispatcherSynchronizationBehavior { AsynchronousSendEnabled = true });

                // create the queues if necessary
                if (ep.Binding is NetMsmqBinding)
                {
                    // create the queues if necessary
                    Msmq.Utilities.CreateQueue(ep.Address.Uri.ToString(), "Services");

                    var dlqAddress = Msmq.Utilities.CreateDeadLetterQueue(ep.Address.Uri.ToString(), "Services");

                    ((NetMsmqBinding)ep.Binding).CustomDeadLetterQueue = new Uri(dlqAddress);
                }
                else
                // add necessary behaviors for REST-ful services
                if (ep.Binding is WebHttpBinding)
                    ep.EndpointBehaviors.Add(
                        new WebHttpBehavior
                        {
                            AutomaticFormatSelectionEnabled = true,
                            DefaultOutgoingResponseFormat   = WebMessageFormat.Json,
                            HelpEnabled                     = true,
                        });
            }

            // transaction timeout
            if (host.Description.Endpoints.SelectMany(
                        ep => ep.Contract.Operations.SelectMany(
                            o => o.Behaviors.OfType<OperationBehaviorAttribute>()))
                                    .Any(ob => ob.TransactionScopeRequired))
                host.GetOrAddServiceBehavior<ServiceBehaviorAttribute>().TransactionTimeout = configurator.TransactionTimeout;

            return host;
        }

        static readonly Uri _mexUri = new Uri("mex", UriKind.Relative);
        static IDictionary<string, Func<ServiceHost, ServiceEndpoint>> _mexEndpoints = new SortedDictionary<string, Func<ServiceHost, ServiceEndpoint>>
        {
            { Uri.UriSchemeNetTcp,  h => h.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(),       _mexUri) },
            { Uri.UriSchemeHttp,    h => h.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(),      _mexUri) },
            { Uri.UriSchemeHttps,   h => h.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpsBinding(),     _mexUri) },
            { Uri.UriSchemeNetPipe, h => h.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexNamedPipeBinding(), _mexUri) },
        };

        /// <summary>
        /// Adds IMetadataExchange endpoints to the service's host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>ServiceHost.</returns>
        /// <exception cref="System.ArgumentNullException">host</exception>
        public static ServiceHost AddMexEndpoints(
            this ServiceHost host)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            foreach (var baseAddress in host.BaseAddresses)
                if (_mexEndpoints.TryGetValue(baseAddress.Scheme, out var AddMexEndpoint))
                    AddMexEndpoint(host);

            return host;
        }

        /// <summary>
        /// Adds metadata behaviors to the service host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="features">The features.</param>
        /// <returns>ServiceHost.</returns>
        /// <exception cref="ArgumentNullException">host</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static ServiceHost AddMetadataBehaviors(
            this ServiceHost host,
            MetadataFeatures features = MetadataFeatures.All)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));


            var serviceMetadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();

            // if no metadata is requested - remove the behavior altogether
            if (features == MetadataFeatures.None)
            {
                if (serviceMetadataBehavior != null)
                    host.Description.Behaviors.Remove(serviceMetadataBehavior);

                return host;
            }

            // add the metadata behavior, if it is not there yet
            if (serviceMetadataBehavior == null)
            {
                serviceMetadataBehavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(serviceMetadataBehavior);
            }

            // disable the help page if not requested or if we have REST-ful endpoints
            if (!features.HasFlag(MetadataFeatures.HelpPage) ||
                host.Description.Endpoints.Any(ep => ep.Binding is WebHttpBinding))
            {
                var serviceDebugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();

                if (serviceDebugBehavior != null)
                    serviceDebugBehavior.HttpHelpPageEnabled  =
                    serviceDebugBehavior.HttpsHelpPageEnabled = false;

                var ep = host.Description.Endpoints.Where(e => e.Binding is WebHttpBinding).FirstOrDefault();

                if (ep != null)
                {
                    if (ep.EndpointBehaviors.Where(b => b is WebHttpBehavior).FirstOrDefault() is WebHttpBehavior epb)
                        epb.HelpEnabled = false;
                }
            }

            // disable the Web HTTP (REST) help page
            if (!features.HasFlag(MetadataFeatures.WebHttpHelpPage))
            {
                var ep = host.Description.Endpoints.Where(e => e.Binding is WebHttpBinding).FirstOrDefault();

                if (ep != null)
                {
                    if (ep.EndpointBehaviors.Where(b => b is WebHttpBehavior).FirstOrDefault() is WebHttpBehavior epb)
                        epb.HelpEnabled = false;
                }
            }

            // add the GET WSDL behavior if requested
            if (features.HasFlag(MetadataFeatures.Wsdl))
            {
                serviceMetadataBehavior.HttpsGetEnabled = host.BaseAddresses.Any(a => a.Scheme == Uri.UriSchemeHttps);
#if DEBUG
                serviceMetadataBehavior.HttpGetEnabled  = host.BaseAddresses.Any(a => a.Scheme == Uri.UriSchemeHttp);
#endif

                Debug.WriteLineIf(
                    !serviceMetadataBehavior.HttpGetEnabled && !serviceMetadataBehavior.HttpsGetEnabled,
                    "\n**** If you need a GET service metadata (WSDL) behavior, define a base address with an http(s) scheme. ***\n");
            }
            else
            {
                serviceMetadataBehavior.HttpGetEnabled  =
                serviceMetadataBehavior.HttpsGetEnabled = false;
                serviceMetadataBehavior.HttpGetUrl  =
                serviceMetadataBehavior.HttpsGetUrl = null;
            }

            // if IMetadataExchange contract is requested - add it
            if (features.HasFlag(MetadataFeatures.Mex))
                host.AddMexEndpoints();

            return host;
        }

        /// <summary>
        /// Adds debug behaviors to the host: longer timeouts, exception details, etc.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>ServiceHost.</returns>
        /// <exception cref="System.ArgumentNullException">host</exception>
        public static ServiceHost AddDebugBehaviors(
            this ServiceHost host)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));


#if DEBUG
            // add include exception details:
            var serviceDebugBehavior = host.GetOrAddServiceBehavior<ServiceDebugBehavior>();

            serviceDebugBehavior.IncludeExceptionDetailInFaults = true;
            //serviceDebugBehavior.HttpHelpPageEnabled = false;
#endif

            return host;
        }

        /// <summary>
        /// Adds security audit behavior.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>ServiceHost.</returns>
        /// <exception cref="System.ArgumentNullException">host</exception>
        public static ServiceHost AddSecurityAuditBehavior(
            this ServiceHost host)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));


            // add security audit behavior
            host.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            host.Description.Behaviors.Add(
                new ServiceSecurityAuditBehavior
                {
                    AuditLogLocation                = AuditLogLocation.Default,
                    SuppressAuditFailure            = false,
                    MessageAuthenticationAuditLevel = AuditLevel.Failure,
                    ServiceAuthorizationAuditLevel  = AuditLevel.Failure,
                });

            return host;
        }

        /// <summary>
        /// Adds a <see cref="ServiceAuthorizationManager"/> to a <see cref="ServiceHost"/>.
        /// </summary>
        /// <param name="serviceHost"></param>
        /// <param name="serviceAuthorizationManager"></param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static ServiceHost AddServiceAuthorizationBehavior(
            this ServiceHost serviceHost,
            ServiceAuthorizationManager serviceAuthorizationManager)
        {
            if (serviceHost == null)
                throw new ArgumentNullException(nameof(serviceHost));
            if (serviceAuthorizationManager == null)
                throw new ArgumentNullException(nameof(serviceAuthorizationManager));

            serviceHost.Description.Behaviors.Remove<ServiceAuthorizationBehavior>();
            serviceHost.Description.Behaviors.Add(
                                                new ServiceAuthorizationBehavior
                                                {
                                                    PrincipalPermissionMode     = PrincipalPermissionMode.Custom,
                                                    ServiceAuthorizationManager = serviceAuthorizationManager,

                                                });

            return serviceHost;
        }

        /// <summary>
        /// Enables the CORS behavior. See https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>ServiceHost.</returns>
        public static ServiceHost EnableCorsBehavior(
            this ServiceHost host)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            // must have endpoint with WebHttpBinding and either the whole contract or any of the operations have EnableCorsAttribute
            foreach (var endpoint in host.Description
                                         .Endpoints
                                         .Where(ep => ep.Binding is WebHttpBinding))
            {
                var enableCorsAttribute = (EnableCorsAttribute)endpoint.Contract.ContractBehaviors.FirstOrDefault(cb => cb is EnableCorsAttribute);

                if (enableCorsAttribute != null)
                    enableCorsAttribute.AddPreflightOperationSelectors(endpoint);
            }

            return host;
        }
    }
}
