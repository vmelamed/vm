using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// The class, interface or method marked with this attribute will be injected with WCF <see cref="ServiceAuthorizationManager"/> based on the Open ID Connect protocol.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class BasicAuthenticationAttribute : Attribute, IServiceBehavior
    {
        /// <summary>
        /// Gets the realm of the authenticated identities.
        /// </summary>
        public string Realm { get; }

        /// <summary>
        /// Gets the resolve name of the basic authentication provider.
        /// </summary>
        public string BasicAuthenticationResolveName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthenticationAttribute"/> class.
        /// </summary>
        /// <param name="realm">The realm of the authenticated identities.</param>
        /// <param name="basicAuthenticationResolveName">The resolve name of the basic authentication provider.</param>
        public BasicAuthenticationAttribute(
            string realm = null,
            string basicAuthenticationResolveName = null)
        {
            Realm                          = realm;
            BasicAuthenticationResolveName = basicAuthenticationResolveName;
        }

        #region IServiceBehavior
        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers,
        /// message or parameter interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var auth = ServiceLocator.Current.GetInstance<IBasicAuthenticate>(BasicAuthenticationResolveName);
            var ctx  = ServiceLocator.Current.GetInstance<IWcfContextUtilities>();

            serviceHostBase.Authorization.ServiceAuthorizationManager = new BasicAuthorizationManager(ctx, auth, Realm);
            serviceHostBase.Authorization.PrincipalPermissionMode     = PrincipalPermissionMode.Custom;
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Validate(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
        }
        #endregion
    }
}
