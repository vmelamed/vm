using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Extension methods for use with <see cref="OperationContext"/>.
    /// </summary>
    /// <remarks>Original author: Yehuda Graber</remarks>
    public static class OperationContextExtensions
    {
        const string BearerPrefix        = "Bearer";
        const string Principal           = "Principal";

        /// <summary>
        /// Get the authorization token as JWT from HTTP request.
        /// </summary>
        /// <param name="operationContext">The current operation context.</param>
        /// <returns>The JWT token from the operation context.</returns>
        public static string GetAuthorizationToken(
            this OperationContext operationContext)
        {
            Contract.Requires<ArgumentNullException>(operationContext != null, nameof(operationContext));

            var authorizationHeader = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Authorization];

            if (authorizationHeader.IsNullOrWhiteSpace())
                return null;

            var parts = authorizationHeader.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts[0].Equals(BearerPrefix, StringComparison.OrdinalIgnoreCase))
                return parts[1];

            return null;
        }

        /// <summary>
        /// Set <see cref="ClaimsPrincipal"/> with <see cref="Claim"/>s in <see cref="OperationContext"/>'s <see cref="AuthorizationContext"/> properties.
        /// </summary>
        /// <param name="operationContext">The current operation context.</param>
        /// <param name="claims">The claims to be set in the current operation context</param>
        public static void SetClaimsPrincipal(
            this OperationContext operationContext,
            IEnumerable<Claim> claims)
        {
            Contract.Requires<ArgumentNullException>(operationContext?.ServiceSecurityContext?.AuthorizationContext?.Properties != null, nameof(operationContext));
            Contract.Requires<ArgumentNullException>(claims != null, nameof(claims));

            operationContext.SetPrincipal(new ClaimsPrincipal(new ClaimsIdentity[] { new ClaimsIdentity(claims) }));
        }

        /// <summary>
        /// Set <see cref="IPrincipal"/> object in <see cref="OperationContext"/>'s <see cref="AuthorizationContext"/> properties.
        /// </summary>
        /// <param name="operationContext">The current operation context.</param>
        /// <param name="principal">The principal to be set in the current operation context</param>
        public static void SetPrincipal(
            this OperationContext operationContext,
            IPrincipal principal)
        {
            Contract.Requires<ArgumentNullException>(operationContext?.ServiceSecurityContext?.AuthorizationContext?.Properties != null, nameof(operationContext));
            Contract.Requires<ArgumentNullException>(principal != null, nameof(principal));

            var properties = operationContext.ServiceSecurityContext.AuthorizationContext.Properties;

            // upsert the principal in the context
            if (properties.ContainsKey(Principal))
                operationContext.ServiceSecurityContext.AuthorizationContext.Properties[Principal] = principal;
            else
                properties.Add(Principal, principal);
        }
    }
}
