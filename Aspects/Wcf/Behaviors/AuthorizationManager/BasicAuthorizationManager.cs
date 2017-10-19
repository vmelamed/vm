using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// Authorization manager that performs Basic HTTP authentication/authorization.
    /// </summary>
    /// <seealso cref="ServiceAuthorizationManager" />
    public class BasicAuthorizationManager : ServiceAuthorizationManager
    {
        const string BasicPrefix = "Basic";
        readonly string _realm;

        readonly IWcfContextUtilities _wcfContext;
        readonly IBasicAuthenticate _authentication;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthorizationManager" /> class.
        /// </summary>
        /// <param name="wcfContext">The WCF context.</param>
        /// <param name="authentication">The authentication interface.</param>
        /// <param name="realm">The realm of authentication.</param>
        public BasicAuthorizationManager(
            IWcfContextUtilities wcfContext,
            IBasicAuthenticate authentication,
            string realm = "vm.Aspects")
        {
            if (realm.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(realm));

            // if not injected by DI, try the CSL - if not defined it'll throw exception
            if (wcfContext == null)
                wcfContext = ServiceLocator.Current.GetInstance<IWcfContextUtilities>();
            if (authentication == null)
                authentication = ServiceLocator.Current.GetInstance<IBasicAuthenticate>();

            _wcfContext     = wcfContext;
            _authentication = authentication;
            _realm          = realm;
        }

        /// <summary>
        /// Checks authorization for the given operation context based on default policy evaluation.
        /// </summary>
        /// <param name="operationContext">The <see cref="OperationContext" /> for the current authorization request.</param>
        /// <returns>true if access is granted; otherwise, false. The default is true.</returns>
        /// <exception cref="WebFaultException"></exception>
        protected override bool CheckAccessCore(
            OperationContext operationContext)
        {
            // This manager is for WebHttpBinding (REST-ful) only, for the other bindings, WCF must've done it already.
            if (!_wcfContext.HasWebOperationContext)
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

            var headerParts = authorizationHeader.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);

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
                var claims = new List<Claim>();

                claims.Add(new Claim(ClaimTypes.NameIdentifier, userName));

                operationContext.SetPrincipal(
                    new ClaimsPrincipal(
                        new ClaimsIdentity(claims)));

                return true;
            }

            Facility.LogWriter.LogError($"Basic authentication for user-name '{userName}' failed.");
            return false;
        }

        IPrincipal AllowedUnauthenticated()
        {
            var isPreflight = WebOperationContext.Current.IncomingRequest.Method == "OPTIONS"  &&
                              (_wcfContext.OperationAction?.EndsWith(Constants.PreflightSuffix, StringComparison.OrdinalIgnoreCase) == true);
            var allowUnauthenticated = isPreflight
                                            ? new AllowUnauthenticatedAttribute()
                                            : _wcfContext.OperationMethodAllAttributes.OfType<AllowUnauthenticatedAttribute>().FirstOrDefault();

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
