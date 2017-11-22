using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// Class BaseHttpAuthorizationManager.
    /// </summary>
    /// <seealso cref="System.ServiceModel.ServiceAuthorizationManager" />
    public abstract class BaseHttpAuthorizationManager : ServiceAuthorizationManager
    {
        /// <summary>
        /// Gets the WCF context based interface.
        /// </summary>
        protected IWcfContextUtilities WcfContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseHttpAuthorizationManager"/> class.
        /// </summary>
        /// <param name="wcfContext">The WCF context.</param>
        /// <exception cref="System.ArgumentNullException">wcfContext</exception>
        protected BaseHttpAuthorizationManager(
            IWcfContextUtilities wcfContext)
        {
            if (wcfContext == null)
                wcfContext = ServiceLocator.Current.GetInstance<IWcfContextUtilities>();
            if (wcfContext == null)
                throw new ArgumentNullException(nameof(wcfContext));

            WcfContext = wcfContext;
        }

        /// <summary>
        /// Checks authorization for the given operation context based on default policy evaluation.
        /// </summary>
        /// <param name="operationContext">The <see cref="OperationContext" /> for the current authorization request.</param>
        /// <returns>true if access is granted; otherwise, false. The default is true.</returns>
        /// <exception cref="WebFaultException"></exception>
        protected override bool CheckAccessCore(
            OperationContext operationContext)
            => Facility.ExceptionManager.Process(
                () => DoCheckAccess(operationContext),
                false,
                ExceptionPolicyProvider.LogAndSwallowPolicyName);

        /// <summary>
        /// Determines if a service or method allows unauthenticated calls.
        /// </summary>
        /// <returns>IPrincipal if the call is allowed to be unauthenticated, otherwise <see langword="null"/>.</returns>
        protected IPrincipal AllowedUnauthenticated()
        {
            var isPreflight = WebOperationContext.Current.IncomingRequest.Method == "OPTIONS"  &&
                              (WcfContext.OperationAction?.EndsWith(Constants.PreflightSuffix, StringComparison.OrdinalIgnoreCase) == true);
            var allowUnauthenticated = isPreflight
                                            ? new AllowUnauthenticatedAttribute()
                                            : WcfContext.OperationMethodAllAttributes.OfType<AllowUnauthenticatedAttribute>().FirstOrDefault();

            if (allowUnauthenticated?.UnauthenticatedCallsAllowed != true)
                return null;

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, allowUnauthenticated.Name));

            if (!allowUnauthenticated.Role.IsNullOrWhiteSpace())
                claims.Add(new Claim(ClaimTypes.Role, allowUnauthenticated.Role));

            return new ClaimsPrincipal(
                        new ClaimsIdentity(claims));
        }

        /// <summary>
        /// Does the check access for the current operation.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns><c>true</c> if access to the current operation is allowed, <c>false</c> otherwise.</returns>
        protected abstract bool DoCheckAccess(
            OperationContext operationContext);
    }
}
