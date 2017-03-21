using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Defines a few extension methods to the <see cref="PolicyDefinition"/>
    /// </summary>
    public static class PolicyDefinitionExtensions
    {
        /// <summary>
        /// Adds the common to all services call handlers.
        /// </summary>
        /// <param name="policyDefinition">The policy definition.</param>
        /// <returns>PolicyDefinition.</returns>
        public static PolicyDefinition AddSingletonCallHandler<T>(
            this PolicyDefinition policyDefinition)
            where T : ICallHandler
        {
            Contract.Requires<ArgumentNullException>(policyDefinition != null, nameof(policyDefinition));
            Contract.Ensures(Contract.Result<PolicyDefinition>() != null);

            return policyDefinition
                        .AddCallHandler<T>(new ContainerControlledLifetimeManager())
                        ;
        }
    }
}
