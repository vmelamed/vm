using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Wcf.Clients
{
    /// <summary>
    /// A lightweight WCF service client class based on <see cref="ChannelFactory{T}"/>, includes interceptor behavior.
    /// </summary>
    /// <typeparam name="TContract">The service interface.</typeparam>
    public abstract class InterceptorLightClient<TContract> : LightClientBase<TContract>, ICallIntercept where TContract : class
    {
        #region ICallIntercept
        /// <summary>
        /// Invoked before the call.
        /// </summary>
        /// <param name="request">The request.</param>
        public virtual void PreInvoke(ref Message request)
        {
        }

        /// <summary>
        /// Invoked after the call.
        /// </summary>
        /// <param name="reply">The reply.</param>
        public virtual void PostInvoke(ref Message reply)
        {
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{T}"/> class
        /// from an endpoint configuration section given by the <paramref name="endpointConfigurationName"/> and a remote address.
        /// If <paramref name="endpointConfigurationName"/> is <see langword="null" />, empty or consist of whitespace characters
        /// the constructor will try to resolve the binding from the schema in the given remote address from the <see cref="DIContainer"/>.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint section of the configuration files.</param>
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
        protected InterceptorLightClient(
            string endpointConfigurationName,
            string remoteAddress,
            string messagingPattern = null)
            : base(endpointConfigurationName, remoteAddress, messagingPattern)
        {
            Contract.Requires<ArgumentException>(
                (endpointConfigurationName!=null && endpointConfigurationName.Length > 0 && endpointConfigurationName.Any(c => !char.IsWhiteSpace(c)))  ||
                (remoteAddress!=null && remoteAddress.Length > 0 && remoteAddress.Any(c => !char.IsWhiteSpace(c))), "At least one of the parameters must be not null, not empty and not consist of whitespace characters only.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
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
        protected InterceptorLightClient(
            string remoteAddress,
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null)
            : base(remoteAddress, identityType, identity, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None || identityType == ServiceIdentity.Certificate ||
                                                 (identity!=null && identity.Length > 0 && identity.Any(c => !char.IsWhiteSpace(c))), "Invalid combination of identity parameters.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
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
        protected InterceptorLightClient(
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
            : base(remoteAddress, identityType, certificate, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None  ||  (identityType == ServiceIdentity.Dns  ||
                                                                                            identityType == ServiceIdentity.Rsa  ||
                                                                                            identityType == ServiceIdentity.Certificate) && certificate!=null, "Invalid combination of identity parameters.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
            : base(remoteAddress, identityClaim, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(identityClaim != null, nameof(identityClaim));

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
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
        protected InterceptorLightClient(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityType, identity, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None || identityType == ServiceIdentity.Certificate ||
                                                 (identity!=null && identity.Length > 0 && identity.Any(c => !char.IsWhiteSpace(c))), "Invalid combination of identity parameters.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
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
        protected InterceptorLightClient(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityType, certificate, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None  ||  (identityType == ServiceIdentity.Dns  ||
                                                                                            identityType == ServiceIdentity.Rsa  ||
                                                                                            identityType == ServiceIdentity.Certificate) && certificate!=null, "Invalid combination of identity parameters.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            Binding binding,
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityClaim, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(identityClaim != null, nameof(identityClaim));

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// Disposes the object graph.
        /// </summary>
        protected override void DisposeObjectGraph()
        {
            (Proxy as ICommunicationObject)?.DisposeCommunicationObject();
            base.DisposeObjectGraph();
        }
        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        void ObjectInvariant()
        {
            Contract.Invariant(Proxy != null);
        }
    }
}
