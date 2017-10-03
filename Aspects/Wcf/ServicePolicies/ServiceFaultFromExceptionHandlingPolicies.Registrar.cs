using System;
using System.Collections.Generic;
using Unity;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Wcf.ServicePolicies
{
    public partial class ServiceFaultFromExceptionHandlingPolicies
    {
        class PolicyRegistrar : ContainerRegistrar
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
                    .RegisterInstanceIfNot<IExceptionPolicyProvider>(registrations, ServiceFaultFromExceptionHandlingPolicies.RegistrationName, new ServiceFaultFromExceptionHandlingPolicies());
            }
        }

        /// <summary>
        /// Gets the WCF exception handling policies registrar.
        /// </summary>
        public static ContainerRegistrar Registrar { get; } = new PolicyRegistrar();
    }
}
