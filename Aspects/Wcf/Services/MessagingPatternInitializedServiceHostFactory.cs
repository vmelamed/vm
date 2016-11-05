using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Bindings;

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
    /// 			*** good method to override in order to add programmatically endpoints ***
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
    /// See also <seealso cref="MessagingPatternServiceHostFactory{TContract}"/>.
    /// </remarks>
    public abstract class MessagingPatternInitializedServiceHostFactory<TContract, TInitializer> : MessagingPatternServiceHostFactory<TContract>
        where TContract : class
        where TInitializer : IInitializeService
    {
        IInitializeService _serviceInitializer;

        /// <summary>
        /// Gets or sets the resolve name of the initializer.
        /// </summary>
        public string InitializerResolveName { get; protected set; }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MessagingPatternInitializedServiceHostFactory&lt;TContract&gt;" /> class.
        /// </summary>
        /// <param name="messagingPattern">
        /// The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="BindingConfigurator.MessagingPattern"/> registered values, 
        /// e.g. <c>RequestResponseConfigurator.MessagingPattern</c>. If <see langword="null"/> the host will try to resolve the messaging 
        /// pattern from the <see cref="MessagingPatternAttribute"/> applied to the contract (the interface).
        /// If the messaging pattern is not resolved yet, the host will assume that the binding is fully configured, e.g. from a config file.
        /// </param>
        /// <param name="initializerResolveName">The resolve name of the initializer.</param>
        public MessagingPatternInitializedServiceHostFactory(
            string messagingPattern = null,
            string initializerResolveName = null)
            : base(messagingPattern)
        {
            InitializerResolveName = initializerResolveName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MessagingPatternInitializedServiceHostFactory&lt;TContract&gt;" /> class.
        /// </summary>
        /// <param name="identityType">Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" /> or <see cref="ServiceIdentity.Rsa" />.</param>
        /// <param name="identity">The identifier in the case of <see cref="ServiceIdentity.Dns" /> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.</param>
        /// <param name="messagingPattern">
        /// The binding pattern to be applied to all descriptions of end points in the service host.
        /// Must be one of the <see cref="BindingConfigurator.MessagingPattern"/> registered values, 
        /// e.g. <c>RequestResponseConfigurator.MessagingPattern</c>. If <see langword="null"/> the host will try to resolve the messaging 
        /// pattern from the <see cref="MessagingPatternAttribute"/> applied to the contract (the interface).
        /// If the messaging pattern is not resolved yet, the host will assume that the binding is fully configured, e.g. from a config file.
        /// </param>
        /// <param name="initializerResolveName">The resolve name of the initializer.</param>
        public MessagingPatternInitializedServiceHostFactory(
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null,
            string initializerResolveName = null)
            : base(identityType, identity, messagingPattern)
        {
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None || identityType == ServiceIdentity.Certificate ||
                                                 (identity!=null && identity.Length > 0 && identity.Any(c => !char.IsWhiteSpace(c))), "Invalid combination of identity parameters.");

            InitializerResolveName = initializerResolveName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MessagingPatternInitializedServiceHostFactory&lt;TContract&gt;" /> class.
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
        /// <param name="initializerResolveName">The resolve name of the initializer.</param>
        public MessagingPatternInitializedServiceHostFactory(
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null,
            string initializerResolveName = null)
            : base(identityType, certificate, messagingPattern)
        {
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None  ||  (identityType == ServiceIdentity.Dns  ||
                                                           identityType == ServiceIdentity.Rsa  ||
                                                           identityType == ServiceIdentity.Certificate) && certificate!=null, "Invalid combination of identity parameters.");

            InitializerResolveName = initializerResolveName;
        }
        #endregion

        /// <summary>
        /// Initializes the DI container with all necessary registrations. The overloads should always call this method last.
        /// </summary>
        /// <param name="serviceType">Type of the service.
        /// Can be <see langword="null"/>, in which case the service type must be registered in the DI container against <typeparamref name="TContract"/>, probably in an override of <see cref="DoRegisterDefaults"/>.
        /// </param>
        /// <param name="container">The container.</param>
        /// <param name="registrations">The registrations.</param>
        /// <returns>The current IUnityContainer.</returns>
        /// <remarks>This method should be called only from within the context of a lock:
        /// <code>
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationDictionary();
        ///     RegisterDefaults(serviceType, registrations);
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
        /// <param name="serviceType">Type of the service.
        /// Can be <see langword="null"/>, in which case the service type must be registered in the DI container against <typeparamref name="TContract"/>, probably in <see cref="DoRegisterDefaults"/>.
        /// </param>
        /// <returns>the resolve name of the initializer</returns>
        protected virtual string ObtainInitializerResolveName(
            Type serviceType)
        {
            if (InitializerResolveName == null)
                InitializerResolveName = ServiceResolveName;

            return InitializerResolveName;
        }

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
                        _serviceInitializer = ServiceLocator.Current.GetInstance<IInitializeService>(InitializerResolveName);
                    }
                    catch (ActivationException x)
                    {
                        // log and swallow - there is no registered initializer
                        Facility.LogWriter
                                .ExceptionError(x);
                    }
                    catch (ResolutionFailedException x)
                    {
                        // log and swallow - there is no registered initializer
                        Facility.LogWriter
                                .ExceptionError(x);
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
        protected virtual LifetimeManager ServiceInitializerLifetimeManager => new TransientLifetimeManager();

        /// <summary>
        /// Gets a value indicating whether the created host is initialized yet.
        /// </summary>
        public override Task<bool> InitializeHostTask => _initializeHostTask;

        Task<bool> _initializeHostTask = Task.FromResult(false);

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
                Facility.LogWriter
                        .EventLogError("The DI container was not properly initialized. Retrying.");

                Debugger.Break();
#endif
                RegisterDefaults(serviceType);
                ObtainInitializerResolveName(serviceType);
            }

            var initializer = ServiceInitializer;

            if (initializer != null)
                // start initialization on another thread and return immediately
                _initializeHostTask = initializer
                                            .InitializeAsync(host, MessagingPattern, 0)
                                            .ContinueWith(
                                                t =>
                                                {
                                                    if (t.IsCompleted  &&  t.Result)
                                                        HostInitialized(host, serviceType);
                                                    else
                                                        throw new InvalidOperationException(
                                                                    $"{GetType().Name} could not initialize the host.");

                                                    return t.Result;
                                                });
            else
                Facility.LogWriter
                        .EventLogError(
                            $"The service host factory for service {serviceType.Name} was not initialized: could not find an initializer in the container. If you do not need one - use BindingPatternServiceHostFactory instead.");
        }
    }
}
