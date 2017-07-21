using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Practices.Unity;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
#pragma warning disable CS3015 // Type has no accessible constructors which use only CLS-compliant types
    /// <summary>
    /// The marked class, interface or method with this attribute will be injected with WCF <see cref="ServiceAuthorizationManager"/> based on the Open ID Connect protocol.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Method,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class OpenIdAuthorizationAttribute : Attribute, IServiceBehavior
    {
        /// <summary>
        /// Gets the DI resolve names of the token validation parameters that should be tried to validate the JWT token.
        /// </summary>
        public ICollection<string> TokenValidationParametersResolveNames { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdAuthorizationAttribute"/> class.
        /// </summary>
        /// <param name="tokenValidationParametersResolveNames">The DI resolve names of the token validation parameters. Cannot be null or empty but can contain at most one null or empty resolve name.</param>
        public OpenIdAuthorizationAttribute(
            params string[] tokenValidationParametersResolveNames)
        {
            Contract.Requires<ArgumentNullException>(tokenValidationParametersResolveNames != null, nameof(tokenValidationParametersResolveNames));

            TokenValidationParametersResolveNames = tokenValidationParametersResolveNames.ToList();
        }

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            var manager = new OpenIdServiceAuthorizationManager(GetTokenValidationParameters());

            serviceHostBase.Authorization.ServiceAuthorizationManager = manager;
        }

        IEnumerable<Lazy<TokenValidationParameters>> GetTokenValidationParameters()
        {
            foreach (var tokenValidationParametersResolveName in TokenValidationParametersResolveNames)
                yield return DIInstanceProvider
                                .CurrentContainer
                                .Resolve<Lazy<TokenValidationParameters>>(tokenValidationParametersResolveName);
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
        }
    }
#pragma warning restore CS3015
}
