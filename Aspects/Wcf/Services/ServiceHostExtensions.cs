using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Microsoft.Practices.ServiceLocation;
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
        None     = 0,
        /// <summary>
        /// Generate a help page
        /// </summary>
        HelpPage = 1,
        /// <summary>
        /// GET WSDL behavior
        /// </summary>
        Wsdl     = 2,
        /// <summary>
        /// IMetadataBehavior
        /// </summary>
        Mex      = 4,
        /// <summary>
        /// All of the above
        /// </summary>
        All      = 7,
    }

    /// <summary>
    /// Class ServiceHostExtensions. Adds several extension methods to <see cref="ServiceHost"/> for easier configuration.
    /// </summary>
    public static class ServiceHostExtensions
    {
        /// <summary>
        /// Sets the service identity.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or 
        /// <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identity">The identity.</param>
        /// <returns>ServiceHost.</returns>
        public static ServiceHost SetServiceIdentity(
            this ServiceHost host,
            ServiceIdentity identityType,
            string identity)
        {
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                           ||
                identityType == ServiceIdentity.Dns  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Rsa  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Upn  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Spn  &&  !string.IsNullOrWhiteSpace(identity),
                "Invalid combination of identity parameters.");
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

            if (identityType == ServiceIdentity.None)
                return host;

            // for each endpoint configure the binding according to the messaging pattern.
            foreach (var ep in host.Description.Endpoints)
                ep.Address = new EndpointAddress(
                                    ep.Address.Uri,
                                    EndpointIdentityFactory.CreateEndpointIdentity(identityType, identity),
                                    ep.Address.Headers,
                                    ep.Address.GetReaderAtMetadata(),
                                    ep.Address.GetReaderAtExtensions());

            return host;
        }

        /// <summary>
        /// Sets the service identity.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Certificate" /> or <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identifyingCertificate">The identifying certificate.</param>
        /// <returns>ServiceHost.</returns>
        public static ServiceHost SetServiceIdentity(
            this ServiceHost host,
            ServiceIdentity identityType,
            X509Certificate2 identifyingCertificate)
        {
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                                  ||
                identityType == ServiceIdentity.Certificate &&  identifyingCertificate!=null          ||
                identityType == ServiceIdentity.Rsa         &&  identifyingCertificate!=null,
                "Invalid combination of identity parameters.");
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

            if (identityType == ServiceIdentity.None)
                return host;

            // for each endpoint configure the binding according to the messaging pattern.
            foreach (var ep in host.Description.Endpoints)
                ep.Address = new EndpointAddress(
                                    ep.Address.Uri,
                                    EndpointIdentityFactory.CreateEndpointIdentity(identityType, identifyingCertificate),
                                    ep.Address.Headers,
                                    ep.Address.GetReaderAtMetadata(),
                                    ep.Address.GetReaderAtExtensions());

            return host;
        }

        /// <summary>
        /// Configures the bindings of a service's endpoints for particular messaging pattern.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="serviceContractType">Type of the service contract.</param>
        /// <param name="messagingPattern">The messaging pattern.</param>
        /// <returns>ServiceHost.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// host
        /// or
        /// serviceContractType
        /// </exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static ServiceHost ConfigureBindings(
            this ServiceHost host,
            Type serviceContractType,
            string messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Requires<ArgumentNullException>(serviceContractType != null, nameof(serviceContractType));
            Contract.Ensures(Contract.Result<ServiceHost>() == host);
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

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
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Ensures(Contract.Result<ServiceHost>() == host);
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

            Func<ServiceHost, ServiceEndpoint> AddMexEndpoint;

            foreach (var baseAddress in host.BaseAddresses)
                if (_mexEndpoints.TryGetValue(baseAddress.Scheme, out AddMexEndpoint))
                    AddMexEndpoint(host);

            return host;
        }

        /// <summary>
        /// Adds metadata behaviors to the service host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="features">The features.</param>
        /// <returns>ServiceHost.</returns>
        /// <exception cref="System.ArgumentNullException">host</exception>
        public static ServiceHost AddMetadataBehaviors(
            this ServiceHost host,
            MetadataFeatures features = MetadataFeatures.All)
        {
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Ensures(Contract.Result<ServiceHost>() == host);
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

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
                    serviceDebugBehavior.HttpHelpPageEnabled = 
                    serviceDebugBehavior.HttpsHelpPageEnabled = false;
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
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Ensures(Contract.Result<ServiceHost>() == host);
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

#if DEBUG
            // add include exception details:
            var serviceDebugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();

            if (serviceDebugBehavior == null)
                host.Description.Behaviors.Add(serviceDebugBehavior = new ServiceDebugBehavior());

            serviceDebugBehavior.IncludeExceptionDetailInFaults = true;
            //serviceDebugBehavior.HttpHelpPageEnabled = false;
#endif

            return host;
        }

        /// <summary>
        /// If any of the operations requires transaction, the method adds default (depending on the build mode) transaction timeout.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>ServiceHost.</returns>
        public static ServiceHost AddTransactionTimeout(
            this ServiceHost host)
        {
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Ensures(Contract.Result<ServiceHost>() == host);
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

            // transaction timeout
            if (host.Description.Endpoints.SelectMany(
                        ep => ep.Contract.Operations.SelectMany(
                            o => o.Behaviors.OfType<OperationBehaviorAttribute>()))
                                    .Any(ob => ob.TransactionScopeRequired))
            {
                ServiceBehaviorAttribute serviceBehavior = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();

                if (serviceBehavior == null)
                    host.Description.Behaviors.Add(serviceBehavior = new ServiceBehaviorAttribute());

                serviceBehavior.TransactionTimeout = Constants.DefaultTransactionTimeout;
            }

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
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Ensures(Contract.Result<ServiceHost>() == host);
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

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
    }
}
