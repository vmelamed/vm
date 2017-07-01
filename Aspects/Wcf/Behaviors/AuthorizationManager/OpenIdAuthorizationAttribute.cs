using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// The marked class, interface or method with this attribute will be injected with WCF <see cref="ServiceAuthorizationManager"/> based on the Open ID Connect protocol.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Method,
        AllowMultiple = false,
        Inherited = false)]
    public class OpenIdAuthorizationAttribute : Attribute, IServiceBehavior
    {
        string _tokenValidationParametersResolveName;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdAuthorizationAttribute"/> class.
        /// </summary>
        /// <param name="tokenValidationParametersResolveName">The DI resolve name of the token validation parameters. Can be null or empty.</param>
        public OpenIdAuthorizationAttribute(
            string tokenValidationParametersResolveName)
        {
            _tokenValidationParametersResolveName = tokenValidationParametersResolveName;
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
            serviceHostBase.Authorization.ServiceAuthorizationManager = new OpenIdServiceAuthorizationManager(ServiceLocator.Current.GetInstance<Lazy<TokenValidationParameters>>(_tokenValidationParametersResolveName));
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
}
