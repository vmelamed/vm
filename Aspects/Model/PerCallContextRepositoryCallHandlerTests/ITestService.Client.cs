using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using vm.Aspects.Wcf;
using vm.Aspects.Wcf.Clients;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    /// <summary>
    /// WCF channel factory based client (proxy) for services implementing the contract ITestService.
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
            if (endpointConfigurationName.IsNullOrWhiteSpace()  &&  remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("At least one of the parameters must be not null, not empty and not consist of whitespace characters only.");
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
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            if (identityType != ServiceIdentity.None && identityType != ServiceIdentity.Certificate && identity.IsNullOrWhiteSpace())
                throw new ArgumentException("Invalid combination of identity parameters.");
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
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            if (identityType != ServiceIdentity.None  &&  !((identityType == ServiceIdentity.Dns  ||
                                                             identityType == ServiceIdentity.Rsa  ||
                                                             identityType == ServiceIdentity.Certificate) && certificate!=null))
                throw new ArgumentException("Invalid combination of identity parameters.");
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
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
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
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            if (identityType != ServiceIdentity.None &&
                identityType != ServiceIdentity.Certificate &&
                identity.IsNullOrWhiteSpace())
                throw new ArgumentException("Invalid combination of identity parameters.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TestServiceClient{TContract}" /> class.
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
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
            if (identityType != ServiceIdentity.None  &&  !((identityType == ServiceIdentity.Dns  ||
                                                             identityType == ServiceIdentity.Rsa  ||
                                                             identityType == ServiceIdentity.Certificate) && certificate!=null))
                throw new ArgumentException("Invalid combination of identity parameters.");
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
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (remoteAddress.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(remoteAddress));
        }
        #endregion

        #region ITestService implementation
        public void AddNewEntity() => Proxy.AddNewEntity();

        public void UpdateEntities() => Proxy.UpdateEntities();

        public int CountOfEntities() => Proxy.CountOfEntities();

        public int CountOfValues() => Proxy.CountOfValues();

        public ICollection<Entity> GetEntities(
            int skip,
            int take) => Proxy.GetEntities(skip, take);

        public EntitiesAndValuesCountsDto GetCounts() => Proxy.GetCounts();
        #endregion
    }
}
