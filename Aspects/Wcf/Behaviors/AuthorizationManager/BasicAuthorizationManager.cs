using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.Practices.ServiceLocation;

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
            Contract.Requires<ArgumentNullException>(realm != null, nameof(realm));
            Contract.Requires<ArgumentException>(realm.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(realm)+" cannot be empty string or consist of whitespace characters only.");

            // if not injected by DI, try the CSL - if not defined it'll throw exception
            if (authentication == null)
                authentication = ServiceLocator.Current.GetInstance<IBasicAuthenticate>();
            if (wcfContext == null)
                wcfContext = ServiceLocator.Current.GetInstance<IWcfContextUtilities>();

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
                WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.WwwAuthenticate, $"Basic realm=\"{_realm}\"");
                throw new WebFaultException(HttpStatusCode.Unauthorized);
            }

            var headerParts = authorizationHeader.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (!headerParts[0].Equals(BasicPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            var credentials = Encoding.ASCII.GetString(
                                        Convert.FromBase64String(authorizationHeader.Substring(6)))
                                            .Split(':');

            return _authentication.Authenticate(credentials[0], credentials[1]);
        }
    }
}
