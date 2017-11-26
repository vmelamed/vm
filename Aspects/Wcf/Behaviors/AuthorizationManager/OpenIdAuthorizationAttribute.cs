using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
#pragma warning disable CS3015 // Type has no accessible constructors which use only CLS-compliant types
    /// <summary>
    /// The class, interface or method marked with this attribute will be injected with WCF <see cref="ServiceAuthorizationManager"/> based on the Open ID Connect protocol.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class OpenIdAuthorizationAttribute : CustomAuthorizationBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdAuthorizationAttribute"/> class.
        /// </summary>
        /// <param name="tokenValidationParametersResolveNames">The DI resolve names of the token validation parameters. Cannot be null or empty but can contain at most one null or empty resolve name.</param>
        public OpenIdAuthorizationAttribute(
            params string[] tokenValidationParametersResolveNames)
        {
            if (tokenValidationParametersResolveNames == null)
                throw new ArgumentNullException(nameof(tokenValidationParametersResolveNames));

            TokenValidationParametersResolveNames = tokenValidationParametersResolveNames.ToList();
        }

        /// <summary>
        /// Gets the DI resolve names of the token validation parameters that should be tried to validate the JWT token.
        /// </summary>
        public ICollection<string> TokenValidationParametersResolveNames { get; }

        /// <summary>
        /// Gets the concrete authorization manager.
        /// </summary>
        /// <returns>ServiceAuthorizationManager.</returns>
        protected override ServiceAuthorizationManager CustomAuthorizationManager
            => new OpenIdServiceAuthorizationManager(TokenValidationParameters);

        IEnumerable<Lazy<TokenValidationParameters>> TokenValidationParameters
        {
            get
            {
                foreach (var tokenValidationParametersResolveName in TokenValidationParametersResolveNames)
                    yield return ServiceLocator.Current.GetInstance<Lazy<TokenValidationParameters>>(tokenValidationParametersResolveName);
            }
        }
    }
#pragma warning restore CS3015
}
