using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Bindings;
using vm.Aspects.Wcf.ServicePolicies;

namespace vm.Aspects.Wcf.Services
{
    /// <summary>
    /// Class MessagingPatternServiceHostFactory. A host factory which configures the end points on the host according to the specified messaging pattern.
    /// </summary>
    /// <typeparam name="TContract">The type of the service contract.</typeparam>
    /// <remarks>
    /// This is the sequence of calls during the service creation and initialization:
    /// <code>
    /// RegisterDefaults 
    /// 	( 
    /// 		registers some Dump metadata, 
    /// 		initializes the DIContainer, 
    /// 		gets the registrations, 
    /// 		registers Facility-s, SEH-s, BindingConfigurator-s
    /// 	)
    /// 	DoRegisterDefaults
    /// 		(
    /// 			*** good method to override in order to add registration of other facilities, e.g. repositories, etc. ***
    /// 		)
    /// 	(
    /// 		registers the service with ServiceResolveName and ServiceLifetimeManager
    /// 	)
    /// DoCreateServiceHost
    /// 	(
    /// 		creates the host
    /// 	)
    /// 	AddEndpoints
    /// 		(
    /// 			does nothing, relies on the configuration to add endpoints
    /// 			*** good method to override in order to add programmaticly endpoints ***
    /// 		)
    /// 	(
    /// 		configures the bindings according to the MessagingPattern, transaction timeout, debug behaviors, metadata behavior,
    /// 		subscribes to host.Opening with InitializeHost and host.Closing with CleanupHost
    /// 		*** good method to override in order to add more stuff to the host ***
    /// 	)
    /// 	
    /// </code>
    /// when the service host is created it fires host.Opening
    /// <code>
    ///
    /// InitializeHost		
    /// 	(
    /// 		makes sure that the service is registered
    /// 		writes a message to the event log
    /// 		*** good method to override in order to add initialization tasks ***
    /// 	)
    /// HostInitialized
    ///     (
    ///         here it simply writes an entry in the event log that the service is up fully initialized and ready to process requests.
    ///     }
    /// 	
    /// ...
    /// 
    /// CleanupHost
    /// 	(
    /// 		writes a message to the event log.
    /// 		*** good method to call in order to add some cleanup tasks ***
    /// 	)
    /// </code>
    /// </remarks>
    public abstract class MessagingPatternServiceHostFactory<TContract> : ServiceHostFactory where TContract : class
    {
        readonly ServiceIdentity _identityType;
        readonly X509Certificate2 _identifyingCertificate;
        readonly string _identity;

        #region Properties
        /// <summary>
        /// Gets the binding pattern.
        /// </summary>
        public string MessagingPattern { get; }

        /// <summary>
        /// Gets the container resolve name of the service.
        /// </summary>
        public string ServiceResolveName { get; private set; }

        /// <summary>
        /// Gets a registration lifetime manager for a service.
        /// If not set explicitly in the inheriting classes, defaults to <see cref="TransientLifetimeManager"/> (per-call).
        /// </summary>
        /// <returns>Service's lifetime manager</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unity will dispose it.")]
        protected virtual LifetimeManager ServiceLifetimeManager => new TransientLifetimeManager();

        /// <summary>
        /// Gets or sets a value indicating whether all types and instances needed for this  this instance are registered.
        /// </summary>
        protected virtual bool AreRegistered { get; set; }

        /// <summary>
        /// Gets or sets the metadata features.
        /// </summary>
        /// <value>The metadata features.</value>
        protected virtual MetadataFeatures MetadataFeatures { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingPatternServiceHostFactory&lt;TContract&gt;"/> class.
        /// </summary>
        /// <param name="messagingPattern">
        /// The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="BindingConfigurator.MessagingPattern"/> registered values, 
        /// e.g. <c>RequestResponseConfigurator.MessagingPattern</c>. If <see langword="null"/> the host will try to resolve the messaging 
        /// pattern from the <see cref="MessagingPatternAttribute"/> applied to the contract (the interface).
        /// If the messaging pattern is not resolved yet, the host will assume that the binding is fully configured, e.g. from a config file.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected MessagingPatternServiceHostFactory(
            string messagingPattern = null)
        {
            MessagingPattern = string.IsNullOrWhiteSpace(messagingPattern)
                                    ? typeof(TContract).GetMessagingPattern()
                                    : messagingPattern;
            MetadataFeatures = MetadataFeatures.All;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingPatternServiceHostFactory&lt;TContract&gt;" /> class.
        /// </summary>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns"/>, <see cref="ServiceIdentity.Spn"/>, <see cref="ServiceIdentity.Upn"/> or <see cref="ServiceIdentity.Rsa"/>.
        /// </param>
        /// <param name="identity">
        /// The identifier in the case of <see cref="ServiceIdentity.Dns"/> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn"/> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn"/> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa"/> - use the RSA key.
        /// </param>
        /// <param name="messagingPattern">
        /// The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="BindingConfigurator.MessagingPattern"/> registered values, 
        /// e.g. <c>RequestResponseConfigurator.MessagingPattern</c>. If <see langword="null"/> the host will try to resolve the messaging 
        /// pattern from the <see cref="MessagingPatternAttribute"/> applied to the contract (the interface).
        /// If the messaging pattern is not resolved yet, the host will assume that the binding is fully configured, e.g. from a config file.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected MessagingPatternServiceHostFactory(
            ServiceIdentity identityType,
            string identity = null,
            string messagingPattern = null)
            : this(messagingPattern)
        {
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None || identityType == ServiceIdentity.Certificate ||
                                                 (identity!=null && identity.Length > 0 && identity.Any(c => !char.IsWhiteSpace(c))), "Invalid combination of identity parameters.");

            _identityType = identityType;
            _identity     = identity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingPatternServiceHostFactory&lt;TContract&gt;" /> class.
        /// </summary>
        /// <param name="identityType">Type of the identity should be either <see cref="ServiceIdentity.Certificate"/> or <see cref="ServiceIdentity.Rsa"/>.</param>
        /// <param name="certificate">Specifies the identifying certificate. Assumes that the identity type is <see cref="ServiceIdentity.Certificate" />.</param>
        /// <param name="messagingPattern">
        /// The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="BindingConfigurator.MessagingPattern"/> registered values, 
        /// e.g. <c>RequestResponseConfigurator.MessagingPattern</c>. If <see langword="null"/> the host will try to resolve the messaging 
        /// pattern from the <see cref="MessagingPatternAttribute"/> applied to the contract (the interface).
        /// If the messaging pattern is not resolved yet, the host will assume that the binding is fully configured, e.g. from a config file.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected MessagingPatternServiceHostFactory(
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern)
            : this(messagingPattern)
        {
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None  ||  (identityType == ServiceIdentity.Dns  ||
                                                           identityType == ServiceIdentity.Rsa  ||
                                                           identityType == ServiceIdentity.Certificate) && certificate!=null, "Invalid combination of identity parameters.");

            _identityType           = identityType;
            _identifyingCertificate = certificate;
        }
        #endregion

        /// <summary>
        /// Creates a <see cref="ServiceHost" /> for a specified type of service with a specific base address.
        /// </summary>
        /// <param name="serviceType">Specifies the type of service to host.</param>
        /// <param name="baseAddresses">The <see cref="Array" /> of type <see cref="Uri" /> that contains the base addresses for the service hosted.</param>
        /// <returns>A <see cref="ServiceHost" /> for the type of service specified with a specific base address.</returns>
        protected override ServiceHost CreateServiceHost(
            Type serviceType,
            Uri[] baseAddresses)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            RegisterDefaults(serviceType);

            return DoCreateServiceHost(serviceType, baseAddresses);
        }

        /// <summary>
        /// Creates a service host outside of WAS. Can be used for testing purposes when the service is created in a self-hosting environment.
        /// Here it does the following:
        /// <list type="number">
        /// <item><description>
        /// Calls <see cref="RegisterDefaults"/>, which initializes the unity container from code and/or configuration file (app/web.config or DIContainer.config).
        /// If the container is initialized from unity.config file it must be in the same directory as the main configuration file.
        /// </description></item><item><description>
        /// Then it calls <see cref="DoCreateServiceHost"/> which modifies the descriptions of all endpoints to the pattern specified in the constructor
        /// if there is a registered pattern binding factory in the current container.
        /// </description></item>
        /// </list>
        /// </summary>
        /// <param name="serviceType">Specifies the type of service to host.</param>
        /// <param name="baseAddresses">
        /// The <see cref="Array"/> of type <see cref="Uri"/> that contains the base addresses for the service hosted.
        /// </param>
        /// <returns>
        /// A <see cref="ServiceHost"/> for the type of service specified with a specific base address.
        /// </returns>
        public ServiceHost CreateHost(
            Type serviceType,
            params Uri[] baseAddresses)
        {
            Contract.Requires<ArgumentNullException>(serviceType != null, nameof(serviceType));
            Contract.Requires<ArgumentNullException>(baseAddresses != null, nameof(baseAddresses));

            return CreateServiceHost(serviceType, baseAddresses);
        }

        #region Overridables
        /// <summary>
        /// Registers the service's contract and implementation types in the DI container 
        /// as well as all the facilities needed for the normal work of the services from this framework.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>IUnityContainer.</returns>
        protected virtual IUnityContainer RegisterDefaults(
            Type serviceType)
        {
            Contract.Requires<ArgumentNullException>(serviceType != null, nameof(serviceType));

            if (AreRegistered)
                return DIContainer.Root;

            // container initialization:
            DIContainer.Initialize();

            lock (DIContainer.Root)
            {
                if (AreRegistered)
                    return DIContainer.Root;

                // ObjectDumper registrations
                ClassMetadataRegistrar.RegisterMetadata()
                    .Register<ArgumentValidationException, ArgumentValidationExceptionDumpMetadata>()
                    .Register<ValidationResult, ValidationResultDumpMetadata>()
                    .Register<ValidationResults, ValidationResultsDumpMetadata>()
                    .Register<ConfigurationErrorsException, ConfigurationErrorsExceptionDumpMetadata>()
                    ;

                ServiceResolveName = ObtainServiceResolveName(serviceType);

                var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                DIContainer.Root
                        .UnsafeRegister(Facility.Registrar, registrations)
                        .UnsafeRegister(ServiceExceptionHandlingPolicies.Registrar, registrations)
                        .UnsafeRegister(BindingConfigurator.Registrar, registrations)
                        ;

                // register the defaults of the super classes
                DoRegisterDefaults(serviceType, DIContainer.Root, registrations)
                    // register the service with the interceptor and the policy injection behavior, 
                    // unless it is already registered 
                    .RegisterTypeIfNot(
                        registrations,
                        typeof(TContract),
                        serviceType,
                        ServiceResolveName,
                        ServiceLifetimeManager,
                        new Interceptor<InterfaceInterceptor>(),
                        new InterceptionBehavior<PolicyInjectionBehavior>());

                AreRegistered = true;
                return DIContainer.Root;
            }
        }

        /// <summary>
        /// Registers the facilities needed for the normal work of the services from this framework.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="container">The container.</param>
        /// <param name="registrations">The registrations.</param>
        /// <returns>The current IUnityContainer.</returns>
        /// <exception cref="System.ArgumentNullException">registrations</exception>
        /// <remarks>This method should be called only from within the context of a lock:
        /// <code>
        /// lock (DIContainer.Root)
        /// {
        /// var registrations = DIContainer.Root.GetRegistrationDictionary();
        /// RegisterDefaults(serviceType, registrations);
        /// }
        /// </code></remarks>
        protected virtual IUnityContainer DoRegisterDefaults(
            Type serviceType,
            IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);
            Contract.Ensures(Contract.Result<IUnityContainer>() == container);

            return container;
        }

        /// <summary>
        /// Gets the resolve name of the service. The resolve name is obtained in the following order:
        /// <list type="number">
        /// <item><description>From the DIBehaviorAttribute on the service class if specified.</description></item>
        /// <item><description>From a ResolveNameAttribute on the service class if specified.</description></item>
        /// <item><description>Gets the service type full name, incl. the namespace.</description></item>
        /// </list>
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        protected virtual string ObtainServiceResolveName(
            Type serviceType)
        {
            Contract.Requires<ArgumentNullException>(serviceType != null, nameof(serviceType));

            return serviceType.GetServiceResolveName();
        }

        /// <summary>
        /// Creates the service host.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        /// <returns>ServiceHost.</returns>
        protected virtual ServiceHost DoCreateServiceHost(
            Type serviceType,
            Uri[] baseAddresses)
        {
            Contract.Requires<ArgumentNullException>(serviceType != null, nameof(serviceType));

            var host = base.CreateServiceHost(serviceType, baseAddresses);

            // Add the endpoints:
            host = AddEndpoints(host);

            // Add some common behaviors:
            if (_identity != null)
                host.SetServiceIdentity(_identityType, _identity);
            else
                host.SetServiceIdentity(_identityType, _identifyingCertificate);

            host.ConfigureBindings(typeof(TContract), MessagingPattern)
                .AddTransactionTimeout()
                .AddDebugBehaviors()
                .AddMetadataBehaviors(MetadataFeatures)
                .AddSecurityAuditBehavior()
                .EnableCorsBehavior()
                ;

            host.Opening += InitializeHost;
            host.Closing += CleanupHost;

            return host;
        }

        /// <summary>
        /// Gives opportunity to the service host factory to add programmaticly endpoints before configuring them all.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>ServiceHost.</returns>
        protected virtual ServiceHost AddEndpoints(
            ServiceHost host)
        {
            Contract.Requires<ArgumentNullException>(host != null, nameof(host));
            Contract.Ensures(Contract.Result<ServiceHost>() != null);

            return host;
        }

        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="sender">The sender (the host).</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void InitializeHost(
            object sender,
            EventArgs e)
        {
            Contract.Requires<ArgumentNullException>(sender != null, nameof(sender));

            var host = (ServiceHostBase)sender;
            var serviceType = host.Description.ServiceType;

            if (!DIContainer.Root.IsRegistered<TContract>(string.IsNullOrWhiteSpace(ServiceResolveName) ? null : ServiceResolveName))
            {
#if DEBUG
                // this IF is probably not needed but let's see if we ever get here
                Debug.Assert(DIContainer.Root.IsRegistered<TContract>(string.IsNullOrWhiteSpace(ServiceResolveName) ? null : ServiceResolveName),
                    "Oh well, we DO need to load the container configuration for a second time. "+
                    "Obviously the host factory and opening the host are run in different app.domains.");
                Debugger.Break();
#endif
                RegisterDefaults(serviceType);
            }

            HostInitialized(host, serviceType);
        }

        /// <summary>
        /// Signals that the host has been successfully initialized. Here it simply writes a message to the event log.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="serviceType">Type of the service.</param>
        protected virtual void HostInitialized(
            ServiceHostBase host,
            Type serviceType)
        {
            Contract.Requires<ArgumentNullException>(host        != null, nameof(host));
            Contract.Requires<ArgumentNullException>(serviceType != null, nameof(serviceType));

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.WriteLine("The service host factory for service {0} has been initialized successfully and is available at:");
                foreach (var ep in host.Description.Endpoints)
                    writer.WriteLine("    {0}", ep.Address.ToString());

                Facility.LogWriter
                        .EventLogInfo(
                            writer.GetStringBuilder().ToString(),
                            serviceType.Name);
            }
        }

        /// <summary>
        /// Cleans-up the host.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void CleanupHost(
            object sender,
            EventArgs e)
        {
            Contract.Requires<ArgumentNullException>(sender != null, nameof(sender));

            var host = (ServiceHostBase)sender;
            var serviceType = host.Description.ServiceType;

            Facility.LogWriter
                    .EventLogInfo(
                        "The service host for service {0} has been closed.",
                        serviceType.Name);
        }
        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(
                _identityType == ServiceIdentity.None  ||
                (_identityType != ServiceIdentity.None  && _identityType != ServiceIdentity.Certificate) && (_identity!=null && _identity.Length > 0 && _identity.Any(c => !char.IsWhiteSpace(c)))  ||
                (_identityType == ServiceIdentity.Certificate || _identityType == ServiceIdentity.Dns || _identityType == ServiceIdentity.Rsa) && _identifyingCertificate!=null);
        }
    }
}
