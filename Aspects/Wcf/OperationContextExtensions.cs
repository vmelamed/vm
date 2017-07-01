using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Extension methods for use with <see cref="OperationContext"/>.
    /// </summary>
    /// <remarks>Original author: Yehuda Graber</remarks>
    public static class OperationContextExtensions
    {
        const string Principal = "Principal";

        /// <summary>
        /// Get the authorization token as JWT from HTTP request.
        /// </summary>
        /// <param name="operationContext">The current operation context.</param>
        /// <returns>The JWT token from the operation context.</returns>
        public static string GetAuthorizationToken(
            this OperationContext operationContext)
        {
            Contract.Requires<ArgumentNullException>(operationContext?.RequestContext?.RequestMessage?.Properties != null, nameof(operationContext));

            object httpRequestMessageObject;
            string jwt = null;

            if (operationContext
                    .RequestContext
                    .RequestMessage
                    .Properties
                    .TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                var authorizationHeader = (httpRequestMessageObject as HttpRequestMessageProperty)?.Headers["Authorization"];

                if (!authorizationHeader.IsNullOrWhiteSpace()  &&
                    authorizationHeader.StartsWith("Bearer ", StringComparison.Ordinal))
                    jwt = authorizationHeader.Split(' ').ElementAtOrDefault(1);
            }

            return jwt;
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

            operationContext
                .ServiceSecurityContext
                .AuthorizationContext
                .Properties[Principal] = new ClaimsPrincipal(
                                                new ClaimsIdentity[]
                                                {
                                                    new ClaimsIdentity(claims)
                                                });
        }
    }
}
