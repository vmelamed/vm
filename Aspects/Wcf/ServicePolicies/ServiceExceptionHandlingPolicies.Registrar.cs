using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Class WcfExceptionHandlingPolicies. Defines a registrar and implements <see cref="IExceptionPolicyProvider"/> which add a number of mappings of exceptions to faults 
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
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                container
                    .RegisterInstanceIfNot<IWcfContextUtilities>(registrations, new WcfContextUtilities())
                    .RegisterInstanceIfNot<IExceptionPolicyProvider>(registrations, PolicyName, new ServiceExceptionHandlingPolicies());
            }
        }

        /// <summary>
        /// Gets the WCF exception handling policies registrar.
        /// </summary>
        public static ContainerRegistrar Registrar { get; } = new WcfExceptionHandlingPoliciesRegistrar();
    }
}
