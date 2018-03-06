using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Claims;
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
    /// A base for lightweight WCF service client classes based on <see cref="ChannelFactory{T}"/>. 
    /// This class encapsulates creating the channel factory. It also handles the graceful invoke of <see cref="IDisposable.Dispose()"/>
    /// when the channel is faulted by aborting the channel instead of closing it.
    /// </summary>
    /// <typeparam name="TContract">The service interface.</typeparam>
    public class LightClient<TContract> : IDisposable, IIsDisposed where TContract : class
    {
        TContract _proxy;

        /// <summary>
        /// Gets or sets a value indicating whether the calls to the delegates passed in <see cref="Invoke"/> should be wrapped with new operation context scope. Required for services calling other services.
        /// </summary>
        public bool WrapWithOperationContextScope { get; set; }

        /// <summary>
        /// Gets or sets the channel factory.
        /// </summary>
        /// <value>The channel factory.</value>
        public ChannelFactory<TContract> ChannelFactory { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client invocations must be wrapped with a new operation context scope.
        /// </summary>
        /// <value><c>true</c> if [wrap with operation context scope]; otherwise, <c>false</c>.</value>
        public bool WrapWithOperationContextScope { get; set; }

        /// <summary>
        /// Gets or creates the proxy of the service.
        /// </summary>
        public TContract Proxy
        {
            get
            {
                if (_proxy == null)
                    _proxy = ChannelFactory.CreateChannel();

                return _proxy;
            }
        }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or 
        /// <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identityName">
        /// <list type="bullet">
        /// <item>
        /// If the identity type is <see cref="ServiceIdentity.Dns" /> - the identifier should be the DNS name specified in the service's certificate or machine.
        /// </item>
        /// <item>
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity;
        /// </item>
        /// <item>
        /// If the identity type is <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// </item>
        /// <item>
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.
        /// </item>
        /// </list>
        /// </param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClient(
            string remoteAddress,
            ServiceIdentity identityType = ServiceIdentity.None,
            string identityName = null,
            string messagingPattern = null)
        {
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            EndpointIdentityFactory.ValidateIdentityParameters(identityType, identityName);

            EnsureMessagingPattern(ref messagingPattern);
            BuildChannelFactory(remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityType, identityName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClient{TContract}"/> class (creates the channel factory)
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
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClient(
            string endpointConfigurationName,
            string remoteAddress,
            string messagingPattern = null)
        {
            if (endpointConfigurationName.IsNullOrWhiteSpace()  &&  remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("At least one of the parameters must be not null, not empty and not consist of whitespace characters only.");

            EnsureMessagingPattern(ref messagingPattern);
            if (!endpointConfigurationName.IsNullOrWhiteSpace())
            {
                ChannelFactory = !remoteAddress.IsNullOrWhiteSpace()
                                        ? new ChannelFactory<TContract>(endpointConfigurationName, new EndpointAddress(remoteAddress))
                                        : new ChannelFactory<TContract>(endpointConfigurationName);
                ConfigureBinding(ChannelFactory.Endpoint.Binding, messagingPattern);
                ConfigureChannelFactory(messagingPattern);
            }
            else
            {
                Debug.Assert(!remoteAddress.IsNullOrWhiteSpace());

                BuildChannelFactory(remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(ServiceIdentity.None, ""));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClient{TContract}" /> class (creates the channel factory).
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
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClient(
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
        {
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            EndpointIdentityFactory.ValidateIdentityParameters(identityType, certificate);

            EnsureMessagingPattern(ref messagingPattern);
            BuildChannelFactory(remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityType, certificate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClient(
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
        {
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));

            EnsureMessagingPattern(ref messagingPattern);
            BuildChannelFactory(remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityClaim));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClient{TContract}" /> class (creates the channel factory).
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
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClient(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            EndpointIdentityFactory.ValidateIdentityParameters(identityType, identity);

            EnsureMessagingPattern(ref messagingPattern);
            BuildChannelFactory(binding, remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityType, identity));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClient{TContract}" /> class (creates the channel factory).
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
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClient(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            EndpointIdentityFactory.ValidateIdentityParameters(identityType, certificate);

            EnsureMessagingPattern(ref messagingPattern);
            BuildChannelFactory(binding, remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityType, certificate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected LightClient(
            Binding binding,
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));

            EnsureMessagingPattern(ref messagingPattern);
            BuildChannelFactory(binding, remoteAddress, messagingPattern, EndpointIdentityFactory.CreateEndpointIdentity(identityClaim));
        }
        #endregion

        /// <summary>
        /// Builds a restful channel factory or at least builds and configures the binding derived from the address's scheme.
        /// </summary>
        /// <param name="remoteAddress">The remote address.</param>
        /// <param name="messagingPattern">The messaging pattern. Can be <see langword="null" /> or empty in which case it will pick-up the pattern from <see cref="MessagingPatternAttribute"/>.</param>
        /// <param name="identity">The server identity.</param>
        /// <returns>The built and configured binding, if the channel was not created, otherwise <see langword="null" />.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by Dispose().")]
        protected Binding BuildChannelFactory(
            string remoteAddress,
            string messagingPattern,
            EndpointIdentity identity)
        {
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            if (messagingPattern.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(messagingPattern));

            var remoteUri   = new Uri(remoteAddress);
            var resolveName = remoteUri.Scheme;

            if (remoteUri.Scheme == Uri.UriSchemeHttp  ||  remoteUri.Scheme == Uri.UriSchemeHttps)
            {
                var messagingAttribute = typeof(TContract).GetCustomAttribute<MessagingPatternAttribute>(false);

                if (messagingAttribute?.Restful == true  ||  !remoteAddress.EndsWith(".svc", StringComparison.OrdinalIgnoreCase))       // only rest-ful endpoints do not need .svc page
                    resolveName = string.Concat(remoteUri.Scheme, Constants.RestfulSchemeSuffix);
                else
                {
                    if (messagingAttribute?.BasicHttp == true)
                        resolveName = string.Concat(remoteUri.Scheme, Constants.BasicHttpSchemeSuffix);
                }
            }

            var binding = ServiceLocator.Current.GetInstance<Binding>(resolveName);

            return BuildChannelFactory(binding, remoteAddress, messagingPattern, identity);
        }

        /// <summary>
        /// Builds a restful channel factory or at least builds and configures the binding derived from the address's scheme.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteAddress">The remote address.</param>
        /// <param name="messagingPattern">The messaging pattern. Can be <see langword="null" /> or empty in which case it will pick-up the pattern from <see cref="MessagingPatternAttribute"/>.</param>
        /// <param name="identity">The server identity.</param>
        /// <returns>The built and configured binding, if the channel was not created, otherwise <see langword="null" />.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by Dispose().")]
        protected Binding BuildChannelFactory(
            Binding binding,
            string remoteAddress,
            string messagingPattern,
            EndpointIdentity identity)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            if (messagingPattern.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(messagingPattern));

            ConfigureBinding(binding, messagingPattern);

            if (binding is WebHttpBinding)
            {
                ChannelFactory = new WebChannelFactory<TContract>(binding, new Uri(remoteAddress));

                ChannelFactory.Endpoint.EndpointBehaviors.Add(
                    new WebHttpBehavior
                    {
                        DefaultOutgoingResponseFormat = WebMessageFormat.Json,
                        DefaultOutgoingRequestFormat  = WebMessageFormat.Json,
                    });
            }
            else
            {
                ChannelFactory = new ChannelFactory<TContract>(
                                        binding,
                                        new EndpointAddress(new Uri(remoteAddress), identity));
            }

            ConfigureChannelFactory(messagingPattern);

            return binding;
        }

        static void ConfigureBinding(
            Binding binding,
            string messagingPattern)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (messagingPattern.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(messagingPattern));

            // resolve the configurator
            var configurator = ServiceLocator.Current.GetInstance<BindingConfigurator>(messagingPattern);

            // configure the binding
            configurator.Configure(binding);
        }

        /// <summary>
        /// Gives the inheritors the chance to tweak the setting of the channel factory.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// protected override void ConfigureChannelFactory(
        ///     string messagingPattern)
        /// {
        ///     ChannelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
        /// }
        /// ]]>
        /// </example>
        protected virtual void ConfigureChannelFactory(
            string messagingPattern)
        {
            if (messagingPattern.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(messagingPattern));

            // here we do nothing but give the user the opportunity to tweak the channel factory, e.g.
            //
            // ChannelFactory.Credentials.UserName.UserName = _userId;
            // ChannelFactory.Credentials.UserName.Password = _password;
        }

        void EnsureMessagingPattern(ref string messagingPattern)
        {
            if (!messagingPattern.IsNullOrWhiteSpace())
                return;

            // try to get the messaging pattern from the client
            messagingPattern = GetType().GetMessagingPattern();

            if (messagingPattern.IsNullOrWhiteSpace())
                messagingPattern = typeof(TContract).GetMessagingPattern();
        }

        /// <summary>
        /// Creates a client operation context scope.
        /// Must be disposed, so call it within a <c>using</c> operator:
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using (var client = new LightClient<IMyInterface>())
        /// using (client.CreateOperationContextScope())
        ///     client.MyMethod();
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>OperationContextScope.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public OperationContextScope CreateOperationContextScope()
            => new OperationContextScope((IContextChannel)Proxy);

        /// <summary>
        /// This method can be used to wrap a synchronous call on the <see cref="Proxy"/> in a new <see cref="OperationContext"/> different from the current.
        /// This is useful when the call is made out from a WCF implemented service call on a WCF client of another service.
        /// In this scenario the call needs its own <see cref="OperationContext"/> different from the current one.
        /// </summary>
        /// <typeparam name="T">The type of the return value.</typeparam>
        /// <param name="proxyCall">The proxy call expressed as a lambda function.</param>
        /// <returns>T</returns>
        /// <example>
        /// <![CDATA[
        /// class Client : LightClient<IContract>, IContract
        /// {
        ///     //...
        ///     async Task<string> Method(int a, string b)
        ///         => await WrapOperation(() => Proxy.Method(a, b));
        /// }
        /// ]]>
        /// </example>
        public T Invoke<T>(Func<T> proxyCall)
        {
            if (WrapWithOperationContextScope)
                using (CreateOperationContextScope())
                    return proxyCall();
            else
                return proxyCall();
        }

        /// <summary>
        /// This method can be used to wrap a synchronous call on the <see cref="Proxy"/> in a new <see cref="OperationContext"/> different from the current.
        /// This is useful when the call is made out from a WCF implemented service call on a WCF client of another service.
        /// In this scenario the call needs its own <see cref="OperationContext"/> different from the current one.
        /// </summary>
        /// <param name="proxyCall">The proxy call expressed as a lambda function.</param>
        /// <example>
        /// <![CDATA[
        /// class Client : LightClient<IContract>, IContract
        /// {
        ///     //...
        ///     async Task<string> Method(int a, string b)
        ///         => await WrapOperation(() => Proxy.Method(a, b));
        /// }
        /// ]]>
        /// </example>
        public void Invoke(Action proxyCall)
        {
            if (WrapWithOperationContextScope)
                using (CreateOperationContextScope())
                    proxyCall();
            else
                proxyCall();
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag is being set when the object gets disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value would mean that the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose(bool)"/>.
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
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
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
            (_proxy as ICommunicationObject)?.DisposeCommunicationObject();
            ChannelFactory.DisposeCommunicationObject();
        }
        #endregion
    }
}
