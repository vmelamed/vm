using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Services
{
    /// <summary>
    /// Class MessagingPatternInitializedServiceHostFactory. A host factory which configures the end points on the host according to the specified messaging pattern and
    /// uses <see cref="IInitializeService"/> implementing initializer.
    /// </summary>
    /// <typeparam name="TContract">The type of the t contract.</typeparam>
    /// <typeparam name="TInitializer">The type of the t initializer.</typeparam>
    /// <remarks>
    /// This is the sequence of calls during the service creation and initialization:
    /// <code>
    /// RegisterDefaults 
    /// 	( 
    /// 		registers some Dump metadata, 
    /// 		initializes the DIContainer, 
    /// 		gets the registrations, 
    /// 		registers Facility-s, ExceptionHandler-s, BindingConfigurator-s
    /// 	)
    /// 	DoRegisterDefaults
    /// 		(
    /// 			[ MessagingPatternInitializedServiceHostFactory override:
    /// 				ObtainInitializerResolveName,
    /// 				registers the initializer with InitializerResolveName and ServiceInitializerLifetimeManager
    /// 			]
    /// 			
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
    /// 		configures the host with bindings according to the MessagingPattern, transaction timeout, debug behaviors, metadata behavior,
    /// 		subscribes to host.Opening with InitializeHost and host.Closing with CleanupHost
    /// 		*** good method to override in order to add more stuff to the host ***
    /// 	)
    /// 	
    /// when the service host is created it fires host.Opening
    /// InitializeHost		
    /// 	(
    /// 		makes sure that the service is registered
    /// 		[  MessagingPatternInitializedServiceHostFactory
    /// 			if it resolves the service initializer - calls it asynchronously
    /// 		]
    /// 		writes a message to the event log
    /// 		*** good method to override in order to add service specific initialization tasks, e.g. data access layer initialization, etc. ***
    /// 	)
    /// 	
    /// ...
    /// 
    /// CleanupHost
    /// 	(
    /// 		writes a message to the event log.
    /// 		*** good method in order to call to add some cleanup tasks ***
    /// 	)
    /// </code>
    /// See also <seealso cref="MessagingPatternServiceHostFactory{TContract}"/>.
    /// </remarks>
    public class MessagingPatternInitializedServiceHostFactory<TContract, TInitializer> : MessagingPatternServiceHostFactory<TContract>
        where TContract : class
        where TInitializer : IInitializeService, new()
    {
        /// <summary>
        /// Gets or sets the resolve name of the initializer.
        /// </summary>
        public string InitializerResolveName { get; protected set; }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MessagingPatternInitializedServiceHostFactory&lt;TContract&gt;"/> class
        /// with a messaging pattern set on the interface with <see cref="T:MessagingPatternAttribute"/> or the default and default initializer resolve name (<see langword="null"/>).
        /// </summary>
        public MessagingPatternInitializedServiceHostFactory()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MessagingPatternInitializedServiceHostFactory&lt;TContract&gt;"/> class
        /// with default initializer resolve name.
        /// </summary>
        /// <param name="messagingPattern">
        /// The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="P:BindingConfigurator.MessagingPattern"/> registered values, e.g. <c>RequestResponseConfigurator.MessagingPattern</c>.
        /// </param>
        public MessagingPatternInitializedServiceHostFactory(
            string messagingPattern)
            : this(messagingPattern, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MessagingPatternInitializedServiceHostFactory&lt;TContract&gt;" /> class.
        /// </summary>
        /// <param name="messagingPattern">The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="P:BindingConfigurator.MessagingPattern" /> registered values, e.g. <c>RequestResponseConfigurator.MessagingPattern</c>.
        /// </param>
        /// <param name="initializerResolveName">The resolve name of the initializer.</param>
        public MessagingPatternInitializedServiceHostFactory(
            string messagingPattern,
            string initializerResolveName)
            : base(messagingPattern)
        {
            InitializerResolveName = initializerResolveName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MessagingPatternInitializedServiceHostFactory&lt;TContract&gt;" /> class.
        /// </summary>
        /// <param name="messagingPattern">The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="P:BindingConfigurator.MessagingPattern" /> registered values,
        /// e.g. <c>RequestResponseConfigurator.MessagingPattern</c> or it can be <see langword="null" /> in which case
        /// the host will try to resolve it from the <see cref="T:MessagingPatternAttribute" /> applied to the service's interface if present.</param>
        /// <param name="identityType">Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" /> or <see cref="ServiceIdentity.Rsa" />.</param>
        /// <param name="identity">The identifier in the case of <see cref="ServiceIdentity.Dns" /> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.</param>
        /// <param name="initializerResolveName">The resolve name of the initializer.</param>
        public MessagingPatternInitializedServiceHostFactory(
            string messagingPattern,
            ServiceIdentity identityType,
            string identity,
            string initializerResolveName = null)
            : base(messagingPattern, identityType, identity)
        {
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                           ||
                identityType == ServiceIdentity.Dns  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Rsa  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Upn  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Spn  &&  !string.IsNullOrWhiteSpace(identity),
                "Invalid combination of identity parameters.");

            InitializerResolveName = initializerResolveName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MessagingPatternInitializedServiceHostFactory&lt;TContract&gt;" /> class.
        /// </summary>
        /// <param name="messagingPattern">The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="P:BindingConfigurator.MessagingPattern" /> registered values,
        /// e.g. <c>RequestResponseConfigurator.MessagingPattern</c> or it can be <see langword="null" /> in which case
        /// the host will try to resolve it from the <see cref="T:MessagingPatternAttribute" /> applied to the service's interface if present.</param>
        /// <param name="identityType">Type of the identity should be either <see cref="ServiceIdentity.Certificate"/> or <see cref="ServiceIdentity.Rsa"/>.</param>
        /// <param name="certificate">Specifies the identifying certificate. Assumes that the identity type is <see cref="ServiceIdentity.Certificate" />.</param>
        /// <param name="initializerResolveName">The resolve name of the initializer.</param>
        public MessagingPatternInitializedServiceHostFactory(
            string messagingPattern,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string initializerResolveName = null)
            : base(messagingPattern, identityType, certificate)
        {
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                               ||
                identityType == ServiceIdentity.Certificate &&  certificate!=null  ||
                identityType == ServiceIdentity.Rsa         &&  certificate!=null,
                "Invalid combination of identity parameters.");

            InitializerResolveName = initializerResolveName;
        }
        #endregion

        /// <summary>
        /// Initializes the DI container with all necessary registrations. The overloads should always call this method last.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="container">The container.</param>
        /// <param name="registrations">The registrations.</param>
        /// <returns>The current IUnityContainer.</returns>
        /// <remarks>This method should be called only from within the context of a lock:
        /// <code>
        /// lock (DIContainer.Root)
        /// {
        /// var registrations = DIContainer.Root.GetRegistrationDictionary();
        /// RegisterDefaults(serviceType, registrations);
        /// }
        /// </code></remarks>
        protected override IUnityContainer DoRegisterDefaults(
            Type serviceType,
            IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations)
        {
            ObtainInitializerResolveName(serviceType);

            return container.RegisterTypeIfNot<IInitializeService, TInitializer>(
                                                    registrations,
                                                    InitializerResolveName,
                                                    ServiceInitializerLifetimeManager);
        }

        /// <summary>
        /// Determines the resolve name of the initializer.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>the resolve name of the initializer</returns>
        protected virtual string ObtainInitializerResolveName(
            Type serviceType)
        {
            if (InitializerResolveName == null)
                InitializerResolveName = ServiceResolveName;

            return InitializerResolveName;
        }

        IInitializeService _serviceInitializer;

        /// <summary>
        /// Gets the service initializer.
        /// </summary>
        protected IInitializeService ServiceInitializer
        {
            get
            {
                if (_serviceInitializer == null)
                    try
                    {
                        _serviceInitializer = ServiceLocator.Current.GetInstance<IInitializeService>();
                    }
                    catch (ActivationException)
                    {
                        // swallow it - there is no registered initializer
                    }

                return _serviceInitializer;
            }
        }

        /// <summary>
        /// Gets a registration lifetime manager for the service initializer.
        /// If not set explicitly in the inheriting classes, defaults to <see cref="TransientLifetimeManager"/> (per-call).
        /// </summary>
        /// <returns>Service's lifetime manager</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unity will dispose it.")]
        protected virtual LifetimeManager ServiceInitializerLifetimeManager
        {
            get { return new TransientLifetimeManager(); }
        }

        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="sender">The sender (the host).</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void InitializeHost(
            object sender,
            EventArgs e)
        {
            var host = sender as ServiceHost;
            var serviceType = host.Description.ServiceType;

            if (!DIContainer.IsInitialized)
            {
#if DEBUG
                // this IF is probably not needed but let's see if we ever get here
                Contract.Assume(DIContainer.IsInitialized, "Oh well, we DO need to load the container configuration for a second time. Obviously these are different app.domains.");
                Debugger.Break();
#endif
                RegisterDefaults(serviceType);
                ObtainInitializerResolveName(serviceType);
            }

            HostInitialized(host, serviceType);

            if (ServiceInitializer != null)
                // start initialization on another thread and return immediately
                ServiceInitializer.InitializeAsync(host, 0).ConfigureAwait(false);
            else
                Facility.LogWriter
                        .EventLogError(
                            "The service host factory for service {0} was not initialized: could not find an initializer in the container. "+
                            "If you do not need one - use BindingPatternServiceHostFactory instead.",
                            serviceType.Name);
        }
    }
}
