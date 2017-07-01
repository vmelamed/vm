using System;
using System.Diagnostics.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.ServiceModel;
using Microsoft.IdentityModel.Tokens;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// Class Auth0ServiceAuthorizationManager.
    /// </summary>
    /// <seealso cref="ServiceAuthorizationManager" />
    public partial class OpenIdServiceAuthorizationManager : ServiceAuthorizationManager
    {
        readonly Lazy<TokenValidationParameters> _tokenValidationParameters;

        TokenValidationParameters TokenValidationParameters => _tokenValidationParameters.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdServiceAuthorizationManager" /> class.
        /// </summary>
        /// <param name="tokenValidationParameters">The validation parameters.</param>
        public OpenIdServiceAuthorizationManager(
            Lazy<TokenValidationParameters> tokenValidationParameters)
        {
            Contract.Requires<ArgumentNullException>(tokenValidationParameters != null, nameof(tokenValidationParameters));

            _tokenValidationParameters = tokenValidationParameters;
        }

        /// <summary>
        /// Checks authorization for the given operation context.
        /// </summary>
        /// <param name="operationContext">The <see cref="OperationContext" />.</param>
        /// <returns>true if access is granted; otherwise; otherwise false. The default is true.</returns>
        public override bool CheckAccess(
            OperationContext operationContext)
            => Facility.ExceptionManager.Process(
                () =>
                {
                    var jwt = operationContext.GetAuthorizationToken();

                    if (string.IsNullOrEmpty(jwt))
                        return false;

                    var tokenHandler = new JwtSecurityTokenHandler();

                    SecurityToken validatedToken;

                    var claims = tokenHandler.ValidateToken(jwt, TokenValidationParameters, out validatedToken).Claims;

                    operationContext.SetClaimsPrincipal(claims);
                    return true;
                },
                false,
                ExceptionPolicyProvider.LogAndSwallowPolicyName);
    }
}
