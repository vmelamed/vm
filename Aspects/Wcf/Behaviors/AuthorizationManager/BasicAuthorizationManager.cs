using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// Authorization manager that performs Basic HTTP authentication/authorization.
    /// </summary>
    /// <seealso cref="ServiceAuthorizationManager" />
    public class BasicAuthorizationManager : BaseHttpAuthorizationManager
    {
        /// <summary>
        /// The prefix of the basic authentication header value.
        /// </summary>
        public const string BasicPrefix = "Basic";

        readonly string _realm = "vm.Aspects";

        readonly IBasicAuthenticate _authentication;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthorizationManager" /> class.
        /// </summary>
        /// <param name="wcfContext">The WCF context.</param>
        /// <param name="authentication">The authentication interface.</param>
        /// <param name="realm">The realm of authentication.</param>
        /// <exception cref="ArgumentException">The argument cannot be null, empty string or consist of whitespace characters only. - realm</exception>
        /// <exception cref="ArgumentNullException">authentication</exception>
        public BasicAuthorizationManager(
            IWcfContextUtilities wcfContext,
            IBasicAuthenticate authentication,
            string realm = null)
            : base(wcfContext)
        {
            if (realm.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(realm));

            // if not injected by DI, try the CSL - if still not defined, throw exception
            if (authentication == null)
                authentication = ServiceLocator.Current.GetInstance<IBasicAuthenticate>();
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));

            if (!realm.IsNullOrWhiteSpace())
                _realm = realm;
        }

        /// <summary>
        /// Does the check access for the current operation.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns><c>true</c> if access to the current operation is allowed, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">operationContext</exception>
        /// <exception cref="WebFaultException"></exception>
        protected override bool DoCheckAccess(
            OperationContext operationContext)
        {
            if (operationContext == null)
                throw new ArgumentNullException(nameof(operationContext));

            // This manager is for WebHttpBinding (REST-ful) only, for the other bindings, WCF must've done it already.
            if (!WcfContext.HasWebOperationContext)
                return true;

            var authorizationHeader = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Authorization];

            if (authorizationHeader.IsNullOrWhiteSpace())
            {
                var principal = AllowedUnauthenticated();

                if (principal != null)
                {
                    operationContext.SetPrincipal(principal);
                    return true;
                }
                else
                {
                    WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.WwwAuthenticate, $"Basic realm=\"{_realm}\"");
                    Facility.LogWriter.LogError($"Basic authentication credentials not provided.");
                    throw new WebFaultException(HttpStatusCode.Unauthorized);
                }
            }

            var headerParts = authorizationHeader.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (!headerParts[0].Equals(BasicPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            var credentials = Encoding.ASCII.GetString(
                                                Convert.FromBase64String(authorizationHeader.Substring(6)))
                                                    .Split(':');
            var userName = credentials[0];
            var password = credentials[1];
            var authenticated = _authentication.Authenticate(userName, password);

            if (authenticated)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, userName),
                };

                operationContext.SetPrincipal(
                    new ClaimsPrincipal(
                        new ClaimsIdentity(claims)));

                return true;
            }

            Facility.LogWriter.LogError($"Basic authentication for user-name '{userName}' failed.");
            return false;
        }
    }
}
