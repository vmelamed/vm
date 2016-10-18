using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IdentityModel.Claims;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Wcf.Clients
{
    /// <summary>
    /// A base for lightweight WCF service client classes based on <see cref="T:System.ServiceModel.ChannelFactory{T}"/>. 
    /// This class encapsulates creating the channel factory. It also handles the graceful invoke of <see cref="M:IDisposable.Dispose"/>
    /// when the channel is faulted by aborting the channel instead of closing it.
    /// </summary>
    /// <typeparam name="TContract">The service interface.</typeparam>
    public abstract class LightClientBase<TContract> : IDisposable, IIsDisposed where TContract : class
    {
        /// <summary>
        /// Gets or sets the channel factory.
        /// </summary>
        /// <value>The channel factory.</value>
        public ChannelFactory<TContract> ChannelFactory { get; private set; }

        /// <summary>
        /// Gets the proxy of the service.
        /// </summary>
        public TContract Proxy { get; protected set; }

        #region Constructors
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
            ServiceIdentity identityType = ServiceIdentity.None,
            string identity = null,
            string messagingPattern = null)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None || identityType == ServiceIdentity.Certificate ||
                                                 (identity!=null && identity.Length > 0 && identity.Any(c => !char.IsWhiteSpace(c))), "Invalid combination of identity parameters.");
            Contract.Ensures(ChannelFactory != null);

            BuildChannelFactory(remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityType, identity));
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
                                (endpointConfigurationName!=null && endpointConfigurationName.Length > 0 && endpointConfigurationName.Any(c => !char.IsWhiteSpace(c)))  ||
                                (remoteAddress!=null && remoteAddress.Length > 0 && remoteAddress.Any(c => !char.IsWhiteSpace(c))), "At least one of the parameters must be not null, not empty and not consist of whitespace characters only.");
            Contract.Ensures(ChannelFactory != null);

            if (!string.IsNullOrWhiteSpace(endpointConfigurationName))
            {
                ChannelFactory = !string.IsNullOrWhiteSpace(remoteAddress)
                                        ? new ChannelFactory<TContract>(endpointConfigurationName, new EndpointAddress(remoteAddress))
                                        : new ChannelFactory<TContract>(endpointConfigurationName);
                ConfigureBinding(ChannelFactory.Endpoint.Binding, messagingPattern ?? typeof(TContract).GetCustomAttribute<MessagingPatternAttribute>(false)?.Name);
            }
            else
            {
                Contract.Assume(remoteAddress!=null && remoteAddress.Length > 0 && remoteAddress.Any(c => !char.IsWhiteSpace(c)));

                BuildChannelFactory(remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(ServiceIdentity.None, ""));
            }
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
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None  ||  (identityType == ServiceIdentity.Dns  ||
                                                                                            identityType == ServiceIdentity.Rsa  ||
                                                                                            identityType == ServiceIdentity.Certificate) && certificate!=null, "Invalid combination of identity parameters.");
            Contract.Ensures(ChannelFactory != null);

            BuildChannelFactory(remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityType, certificate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClientBase{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClientBase(
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Ensures(ChannelFactory != null);

            BuildChannelFactory(remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityClaim));
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
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None || identityType == ServiceIdentity.Certificate ||
                                                 (identity!=null && identity.Length > 0 && identity.Any(c => !char.IsWhiteSpace(c))), "Invalid combination of identity parameters.");
            Contract.Ensures(ChannelFactory != null);

            BuildChannelFactory(binding, remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityType, identity));
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
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None  ||  (identityType == ServiceIdentity.Dns  ||
                                                           identityType == ServiceIdentity.Rsa  ||
                                                           identityType == ServiceIdentity.Certificate) && certificate!=null, "Invalid combination of identity parameters.");
            Contract.Ensures(ChannelFactory != null);

            BuildChannelFactory(binding, remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityType, certificate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClientBase{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClientBase(
            Binding binding,
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Ensures(ChannelFactory != null);

            BuildChannelFactory(binding, remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityClaim));
        }
        #endregion

        /// <summary>
        /// Builds a restful channel factory or at least builds and configures the binding derived from the address's scheme.
        /// </summary>
        /// <param name="remoteAddress">The remote address.</param>
        /// <param name="messagingPattern">The messaging pattern.</param>
        /// <param name="identity">The server identity.</param>
        /// <returns>The built and configured binding, if the channel was not created, otherwise <see langword="null" />.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by Dispose().")]
        protected Binding BuildChannelFactory(
            string remoteAddress,
            string messagingPattern,
            EndpointIdentity identity)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Ensures(ChannelFactory != null);
            Contract.Ensures(Contract.Result<Binding>() != null);

            var remoteUri          = new Uri(remoteAddress);
            var scheme             = remoteUri.Scheme;
            var messagingAttribute = typeof(TContract).GetCustomAttribute<MessagingPatternAttribute>(false);

            if ((scheme == "http"  ||  scheme == "https") &&
                ((messagingAttribute?.Restful).GetValueOrDefault()  ||  !remoteAddress.EndsWith(".svc", StringComparison.OrdinalIgnoreCase)))   // only rest-ful endpoints do not need .svc page
                scheme = string.Concat(remoteUri.Scheme, ".rest");

            var binding = ServiceLocator.Current.GetInstance<Binding>(scheme);

            return BuildChannelFactory(binding, remoteAddress, messagingPattern, identity);
        }

        /// <summary>
        /// Builds a restful channel factory or at least builds and configures the binding derived from the address's scheme.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteAddress">The remote address.</param>
        /// <param name="messagingPattern">The messaging pattern.</param>
        /// <param name="identity">The server identity.</param>
        /// <returns>The built and configured binding, if the channel was not created, otherwise <see langword="null" />.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by Dispose().")]
        protected Binding BuildChannelFactory(
            Binding binding,
            string remoteAddress,
            string messagingPattern,
            EndpointIdentity identity)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");

            Contract.Ensures(Contract.Result<Binding>() != null);
            Contract.Ensures(ChannelFactory != null);

            ConfigureBinding(binding, messagingPattern ?? typeof(TContract).GetCustomAttribute<MessagingPatternAttribute>(false)?.Name);

            if (binding is WebHttpBinding)
            {
                ChannelFactory = new WebChannelFactory<TContract>(binding, new Uri(remoteAddress));
                ChannelFactory.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior { DefaultOutgoingRequestFormat = WebMessageFormat.Json });
            }
            else
                ChannelFactory = new ChannelFactory<TContract>(
                                        binding,
                                        new EndpointAddress(new Uri(remoteAddress), identity));

            return binding;
        }

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
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

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
