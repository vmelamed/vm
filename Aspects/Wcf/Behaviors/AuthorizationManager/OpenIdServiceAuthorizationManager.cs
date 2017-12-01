using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.ServiceModel;
using Microsoft.IdentityModel.Tokens;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// Authorization manager that performs OpenID HTTP authentication/authorization.
    /// </summary>
    /// <seealso cref="ServiceAuthorizationManager" />
    public partial class OpenIdServiceAuthorizationManager : BaseHttpAuthorizationManager
    {
        readonly ICollection<Lazy<TokenValidationParameters>> _tokenValidationParameters;
        readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdServiceAuthorizationManager" /> class.
        /// </summary>
        /// <param name="tokenValidationParameters">The validation parameters.</param>
        /// <param name="wcfContext">The WCF context.</param>
        /// <exception cref="ArgumentNullException">tokenValidationParameters</exception>
        public OpenIdServiceAuthorizationManager(
            IEnumerable<Lazy<TokenValidationParameters>> tokenValidationParameters,
            IWcfContextUtilities wcfContext = null)
            : base(wcfContext)
        {
            if (tokenValidationParameters == null)
                throw new ArgumentNullException(nameof(tokenValidationParameters));

            _tokenValidationParameters = tokenValidationParameters.ToList();
        }

        /// <summary>
        /// Does the check access for the current operation.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns><c>true</c> if access to the current operation is allowed, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">operationContext</exception>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override bool DoCheckAccess(
            OperationContext operationContext)
        {
            if (operationContext == null)
                throw new ArgumentNullException(nameof(operationContext));

            // OpenID Connect is for WebHttpBinding (REST-ful) only, for the other bindings, WCF must've done it already.
            if (!WcfContext.HasWebOperationContext)
                return true;

            var jwt = operationContext.GetAuthorizationToken();

            if (jwt.IsNullOrEmpty())
            {
                var principal = AllowedUnauthenticated();

                if (principal == null)
                {
                    Facility.LogWriter.ExceptionError(new UnauthorizedAccessException("Invalid or missing JWT."));
                    return false;
                }

                operationContext.SetPrincipal(principal);
                return true;
            }

            var exceptions = new List<Exception>();
            SecurityToken token;

            foreach (var tvp in _tokenValidationParameters)
                try
                {
                    var claimsPrincipal = _tokenHandler.ValidateToken(jwt, tvp.Value, out token);

                    operationContext.SetPrincipal(claimsPrincipal);
                    return true;
                }
                catch (Exception x)
                {
                    exceptions.Add(x);
                }

            Facility.LogWriter.ExceptionError(new AggregateException("Errors validating the JWT with different validation parameters.", exceptions));
            return false;
        }
    }
}
