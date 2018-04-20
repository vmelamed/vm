using System;
using System.ServiceModel;

using CommonServiceLocator;

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
    public sealed class BasicAuthenticationAttribute : CustomAuthorizationBaseAttribute
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

        /// <summary>
        /// Gets the concrete authorization manager.
        /// </summary>
        /// <returns>ServiceAuthorizationManager.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override ServiceAuthorizationManager CustomAuthorizationManager
            => new BasicAuthorizationManager(
                        ServiceLocator.Current.GetInstance<IWcfContextUtilities>(),
                        ServiceLocator.Current.GetInstance<IBasicAuthenticate>(BasicAuthenticationResolveName),
                        Realm);
    }
}
