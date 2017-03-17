using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using vm.Aspects.Wcf;
using vm.Aspects.Wcf.Clients;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    /// <summary>
    /// WCF channel factory based client (proxy) for services implementing the contract IService.
    /// </summary>
    /// <seealso cref="LightClient{ITestService}" />
    /// <seealso cref="ITestService" />
    public class TestServiceClient : LightClient<ITestService>, ITestService
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceClient" /> class (creates the channel factory)
        /// from an endpoint configuration section given by the <paramref name="endpointConfigurationName" /> and service address.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address. If the remote address is <see langword="null" /> or empty
        /// the constructor will try to use the address in the endpoint configuration.</param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        public TestServiceClient(
            string endpointConfigurationName,
            string remoteAddress,
            string messagingPattern = null)
            : base(endpointConfigurationName, remoteAddress, messagingPattern)
        {
            Contract.Requires<ArgumentException>(
                (endpointConfigurationName!=null && endpointConfigurationName.Length > 0 && endpointConfigurationName.Any(c => !char.IsWhiteSpace(c)))  ||
                (remoteAddress!=null && remoteAddress.Length > 0 && remoteAddress.Any(c => !char.IsWhiteSpace(c))), "At least one of the parameters must be not null, not empty and not consist of whitespace characters only.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceClient" /> class (creates the channel factory).
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
        public TestServiceClient(
            string remoteAddress,
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null)
            : base(remoteAddress, identityType, identity, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(identityType == ServiceIdentity.None || identityType == ServiceIdentity.Certificate ||
                                                 (identity!=null && identity.Length > 0 && identity.Any(c => !char.IsWhiteSpace(c))), "Invalid combination of identity parameters.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceClient" /> class.
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
        public TestServiceClient(
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceClient" /> class.
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        public TestServiceClient(
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
            : base(remoteAddress, identityClaim, messagingPattern)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress!=null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(remoteAddress.Length > 0, "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(remoteAddress.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(remoteAddress)+" cannot be empty or consist of whitespace characters only.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceClient" /> class (creates the channel factory).
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
        public TestServiceClient(
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ServiceClient{TContract}" /> class.
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
        public TestServiceClient(
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceClient" /> class.
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        public TestServiceClient(
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
        }
        #endregion

        #region ITestService implementation
        public void AddNewEntity(
            ) => Proxy.AddNewEntity();

        public void UpdateEntities(
            ) => Proxy.UpdateEntities();

        public int CountOfEntities(
            ) => Proxy.CountOfEntities();

        public ICollection<Entity> GetEntities(
            int skip,
            int take) => Proxy.GetEntities(skip, take);
        #endregion
    }
}
