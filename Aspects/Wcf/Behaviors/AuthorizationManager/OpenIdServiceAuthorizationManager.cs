using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// Class Auth0ServiceAuthorizationManager.
    /// </summary>
    /// <seealso cref="ServiceAuthorizationManager" />
    public partial class OpenIdServiceAuthorizationManager : ServiceAuthorizationManager
    {
        readonly ICollection<Lazy<TokenValidationParameters>> _tokenValidationParameters;
        readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        readonly IWcfContextUtilities _wcfContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdServiceAuthorizationManager" /> class.
        /// </summary>
        /// <param name="tokenValidationParameters">The validation parameters.</param>
        public OpenIdServiceAuthorizationManager(
            IEnumerable<Lazy<TokenValidationParameters>> tokenValidationParameters)
        {
            Contract.Requires<ArgumentNullException>(tokenValidationParameters != null, nameof(tokenValidationParameters));

            _tokenValidationParameters = tokenValidationParameters.ToList();
            _wcfContext                = ServiceLocator.Current.GetInstance<IWcfContextUtilities>();
        }

        /// <summary>
        /// Checks authorization for the given operation context based on default policy evaluation.
        /// </summary>
        /// <param name="operationContext">The <see cref="OperationContext" /> for the current authorization request.</param>
        /// <returns>true if access is granted; otherwise, false. The default is true.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Accumulating all exceptions and throwing AggregateException")]
        protected override bool CheckAccessCore(
            OperationContext operationContext)
            => DoCheckAccess(operationContext);

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        bool DoCheckAccess(
            OperationContext operationContext)
            => Facility.ExceptionManager.Process(
                () =>
                {
                    var jwt = operationContext.GetAuthorizationToken();

                    if (string.IsNullOrEmpty(jwt))
                    {
                        var principal = AllowedUnauthenticated();

                        if (principal == null)
                            return false;

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
                },
                false,
                ExceptionPolicyProvider.LogAndSwallowPolicyName);

        IPrincipal AllowedUnauthenticated()
        {
            var isPreflight = _wcfContext.HasWebOperationContext                               &&
                              WebOperationContext.Current.IncomingRequest.Method == "OPTIONS"  &&
                              (_wcfContext.OperationAction?.EndsWith(Constants.PreflightSuffix, StringComparison.OrdinalIgnoreCase) == true);
            var allowUnauthenticated = isPreflight
                                            ? new AllowOpenIdUnauthenticatedAttribute()
                                            : _wcfContext.OperationMethodAllAttributes.OfType<AllowOpenIdUnauthenticatedAttribute>().FirstOrDefault();

            if (allowUnauthenticated == null)
                return null;

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, allowUnauthenticated.Name));
            if (!allowUnauthenticated.Name.IsNullOrWhiteSpace())
                claims.Add(new Claim(ClaimTypes.Role, allowUnauthenticated.Role));

            return new ClaimsPrincipal(
                        new ClaimsIdentity(claims));
        }
    }
}
