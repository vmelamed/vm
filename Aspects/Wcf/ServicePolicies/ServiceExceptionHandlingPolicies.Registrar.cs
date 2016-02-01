using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Class WcfExceptionHandlingPolicies. Defines a registrar and implements <see cref="T:IExceptionPolicyProvider"/> which add a number of mappings of exceptions to faults 
    /// which will be used by the WCF exception shielding mechanism. 
    /// </summary>
    public partial class ServiceExceptionHandlingPolicies : IExceptionPolicyProvider
    {
        private class WcfExceptionHandlingPoliciesRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                container
                    .RegisterInstanceIfNot<IWcfContextUtilities>(registrations, new WcfContextUtilities())
                    .RegisterInstanceIfNot<IExceptionPolicyProvider>(registrations, "vm.Aspects.Wcf.ServicePolicies", new ServiceExceptionHandlingPolicies());
            }
        }

        static readonly WcfExceptionHandlingPoliciesRegistrar _registrar = new WcfExceptionHandlingPoliciesRegistrar();

        /// <summary>
        /// Gets the WCF exception handling policies registrar.
        /// </summary>
        public static ContainerRegistrar Registrar
        {
            get
            {
                Contract.Ensures(Contract.Result<ContainerRegistrar>() != null);

                return _registrar;
            }
        }
    }
}
