using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Wcf.Clients
{
    /// <summary>
    /// A base for lightweight WCF service client classes based on <see cref="T:System.ServiceModel.ChannelFactory{T}"/>. 
    /// This class encapsulates creating the channel factory. It also handles the graceful invoke of <see cref="M:IDisposable.Dispose"/>
    /// when the channel is faulted by aborting the channel instead of closing it.
    /// </summary>
    /// <typeparam name="TContract">The service interface.</typeparam>
    public abstract class LightClientBase<TContract> : IDisposable where TContract : class
    {
        /// <summary>
        /// Gets or sets the channel factory.
        /// </summary>
        /// <value>The channel factory.</value>
        public ChannelFactory<TContract> ChannelFactory { get; private set; }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LightClientBase{TContract}" /> class (creates the channel factory)
        /// from a remote address. The constructor will try to resolve the binding from the schema in the given remote address from the current DI container.
        /// </summary>
        /// <param name="remoteAddress">
        /// The remote address of the service.
        /// </param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClientBase(
            string remoteAddress,
            string messagingPattern = null)
            : this(null, remoteAddress, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(remoteAddress), "remoteAddress");
            Contract.Ensures(ChannelFactory != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LightClientBase{TContract}"/> class (creates the channel factory)
        /// from an endpoint configuration section given by the <paramref name="endpointConfigurationName"/> and a remote address.
        /// If <paramref name="endpointConfigurationName"/> is <see langword="null" />, empty or consist of whitespace characters only,
        /// the constructor will try to resolve the binding from the schema in the given remote address from the current DI container.
        /// </summary>
        /// <param name="endpointConfigurationName">
        /// The name of the endpoint section of the configuration files.
        /// If <see langword="null" /> the constructor will try to resolve the binding from the address' schema.
        /// </param>
        /// <param name="remoteAddress">
        /// The remote address. If the remote address is <see langword="null" /> or empty
        /// the constructor will try to use the address in the endpoint configuration.
        /// </param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClientBase(
            string endpointConfigurationName,
            string remoteAddress,
            string messagingPattern = null)
        {
            Contract.Requires<ArgumentException>(
                !string.IsNullOrWhiteSpace(endpointConfigurationName) ||
                !string.IsNullOrWhiteSpace(remoteAddress), "At least one of the parameters must be not null, empty or consist of whitespace characters only.");
            Contract.Ensures(ChannelFactory != null);

            var remoteEndpoint = string.IsNullOrWhiteSpace(remoteAddress)
                                        ? null
                                        : new EndpointAddress(remoteAddress);

            Binding binding;

            if (!string.IsNullOrWhiteSpace(endpointConfigurationName))
            {
                ChannelFactory = remoteEndpoint != null
                                        ? new ChannelFactory<TContract>(endpointConfigurationName, remoteEndpoint)
                                        : new ChannelFactory<TContract>(endpointConfigurationName);
                binding        = ChannelFactory.Endpoint.Binding;
            }
            else
            {
                Contract.Assert(remoteEndpoint != null);

                binding        = ServiceLocator.Current.GetInstance<Binding>(remoteEndpoint.Uri.Scheme);
                ChannelFactory = new ChannelFactory<TContract>(binding, remoteEndpoint);
            }

            ConfigureBinding(binding, messagingPattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClientBase{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or 
        /// <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identity">
        /// The identifier in the case of <see cref="ServiceIdentity.Dns" /> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.
        /// </param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClientBase(
            string remoteAddress,
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(remoteAddress), "The argument \"remoteAddress\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                           ||
                identityType == ServiceIdentity.Dns  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Rsa  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Upn  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Spn  &&  !string.IsNullOrWhiteSpace(identity),
                "Invalid combination of identity parameters.");
            Contract.Ensures(ChannelFactory != null);

            var messaging = typeof(TContract).GetCustomAttribute<MessagingPatternAttribute>(false);
            var remoteUri = new Uri(remoteAddress);
            var scheme    = remoteUri.Scheme;

            if (messaging!=null && messaging.Restful)
                scheme += ".rest";

            var binding   = ServiceLocator.Current.GetInstance<Binding>(scheme);

            if (binding is WebHttpBinding)
            {
                ChannelFactory = new WebChannelFactory<TContract>(binding, remoteUri);
                ChannelFactory.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior { DefaultOutgoingRequestFormat = WebMessageFormat.Json });
            }
            else
                ChannelFactory = new ChannelFactory<TContract>(
                                    binding,
                                    new EndpointAddress(remoteUri, EndpointIdentityFactory.CreateEndpointIdentity(identityType, identity)));

            ConfigureBinding(ChannelFactory.Endpoint.Binding, messagingPattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClientBase{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Certificate" />, <see cref="ServiceIdentity.Dns" /> or <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="certificate">The identifying certificate.</param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClientBase(
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(remoteAddress), "The argument \"remoteAddress\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                               ||
                identityType == ServiceIdentity.Certificate &&  certificate!=null  ||
                identityType == ServiceIdentity.Dns         &&  certificate!=null  ||
                identityType == ServiceIdentity.Rsa         &&  certificate!=null,
                "Invalid combination of identity parameters.");
            Contract.Ensures(ChannelFactory != null);

            var messaging = typeof(TContract).GetCustomAttribute<MessagingPatternAttribute>(false);
            var remoteUri = new Uri(remoteAddress);
            var scheme    = remoteUri.Scheme + (messaging!=null && messaging.Restful ? ".rest" : string.Empty);
            var binding   = ServiceLocator.Current.GetInstance<Binding>(scheme);

            if (binding is WebHttpBinding)
            {
                ChannelFactory = new WebChannelFactory<TContract>(binding, remoteUri);
                ChannelFactory.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior { DefaultOutgoingRequestFormat = WebMessageFormat.Json });
            }
            else
                ChannelFactory = new ChannelFactory<TContract>(
                                    binding,
                                    new EndpointAddress(remoteUri, EndpointIdentityFactory.CreateEndpointIdentity(identityType, certificate)));

            ConfigureBinding(ChannelFactory.Endpoint.Binding, messagingPattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClientBase{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or 
        /// <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identity">
        /// The identifier in the case of <see cref="ServiceIdentity.Dns" /> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.
        /// </param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClientBase(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(remoteAddress), "The argument \"remoteAddress\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                           ||
                identityType == ServiceIdentity.Dns  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Rsa  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Upn  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Spn  &&  !string.IsNullOrWhiteSpace(identity),
                "Invalid combination of identity parameters.");
            Contract.Ensures(ChannelFactory != null);

            var remoteUri = new Uri(remoteAddress);

            if (binding is WebHttpBinding)
            {
                ChannelFactory = new WebChannelFactory<TContract>(binding, remoteUri);
                ChannelFactory.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior { DefaultOutgoingRequestFormat = WebMessageFormat.Json });
            }
            else
                ChannelFactory = new ChannelFactory<TContract>(
                                    binding,
                                    new EndpointAddress(remoteUri, EndpointIdentityFactory.CreateEndpointIdentity(identityType, identity)));

            ConfigureBinding(ChannelFactory.Endpoint.Binding, messagingPattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClientBase{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Certificate" /> or <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="certificate">The identifying certificate.</param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClientBase(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(remoteAddress), "The argument \"remoteAddress\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                               ||
                identityType == ServiceIdentity.Certificate &&  certificate!=null  ||
                identityType == ServiceIdentity.Rsa         &&  certificate!=null,
                "Invalid combination of identity parameters.");
            Contract.Ensures(ChannelFactory != null);

            var remoteUri = new Uri(remoteAddress);

            if (binding is WebHttpBinding)
            {
                ChannelFactory = new WebChannelFactory<TContract>(binding, remoteUri);
                ChannelFactory.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior { DefaultOutgoingRequestFormat = WebMessageFormat.Json });
            }
            else
                ChannelFactory = new ChannelFactory<TContract>(
                                    binding,
                                    new EndpointAddress(remoteUri, EndpointIdentityFactory.CreateEndpointIdentity(identityType, certificate)));

            ConfigureBinding(ChannelFactory.Endpoint.Binding, messagingPattern);
        }
        #endregion

        void ConfigureBinding(
            Binding binding,
            string messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

            if (string.IsNullOrWhiteSpace(messagingPattern))
            {
                // try to get the messaging pattern from the client
                messagingPattern = GetType().GetMessagingPattern();

                if (string.IsNullOrWhiteSpace(messagingPattern))
                    messagingPattern = typeof(TContract).GetMessagingPattern();
            }

            // resolve the configurator
            var configurator = ServiceLocator.Current.GetInstance<BindingConfigurator>(messagingPattern);

            // configure the binding
            configurator.Configure(binding);
        }

        static EndpointIdentity GetIdentity(bool isUserPrincipalName, string name)
        {
            return isUserPrincipalName
                        ? EndpointIdentity.CreateUpnIdentity(name ?? string.Empty)
                        : EndpointIdentity.CreateSpnIdentity(name ?? string.Empty);
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag is being set when the object gets disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value would mean that the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="M:Dispose(bool)"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        private int _disposed;

        /// <summary>
        /// Returns <see langword="true"/> if the object has already been disposed, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsDisposed
        {
            get { return Volatile.Read(ref _disposed) != 0; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows the object to attempt to free resources and perform other cleanup operations before the Object is reclaimed by garbage collection. 
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(bool)"/>.</remarks>
        ~LightClientBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement).
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, it will try to release all managed resources 
        /// (usually aggregated objects which implement <see cref="IDisposable"/> as well) and then it will release all unmanaged resources if any.
        /// If the parameter is <see langword="false"/> then the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(
            bool disposing)
        {
            if (disposing)
                DisposeObjectGraph();
        }

        /// <summary>
        /// Disposes the objects associated with this object.
        /// </summary>
        protected virtual void DisposeObjectGraph()
        {
            ChannelFactory.DisposeCommunicationObject();
        }
        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        void ObjectInvariant()
        {
            Contract.Invariant(ChannelFactory != null);
        }
    }
}
