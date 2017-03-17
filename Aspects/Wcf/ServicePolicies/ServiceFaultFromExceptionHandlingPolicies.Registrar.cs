using Microsoft.Practices.Unity;
using System.Collections.Generic;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Wcf.ServicePolicies
{
    public partial class ServiceFaultFromExceptionHandlingPolicies
    {
        private class PolicyRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                container
                    .RegisterInstanceIfNot<IWcfContextUtilities>(registrations, new WcfContextUtilities())
                    .RegisterInstanceIfNot<IExceptionPolicyProvider>(registrations, "ServiceFaultFromExceptionHandlingPolicies", new ServiceFaultFromExceptionHandlingPolicies());
            }
        }

        /// <summary>
        /// Gets the WCF exception handling policies registrar.
        /// </summary>
        public static ContainerRegistrar Registrar { get; } = new PolicyRegistrar();
    }
}
