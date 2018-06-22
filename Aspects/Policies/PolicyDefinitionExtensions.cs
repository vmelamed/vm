using System;
using System.Diagnostics.CodeAnalysis;

using Unity.Interception.ContainerIntegration;
using Unity.Interception.PolicyInjection.Pipeline;
using Unity.Lifetime;

namespace vm.Aspects.Policies
{
#pragma warning disable CS3024 // Constraint type is not CLS-compliant
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unity will do it")]
        public static PolicyDefinition AddSingletonCallHandler<T>(
            this PolicyDefinition policyDefinition)
            where T : ICallHandler
        {
            if (policyDefinition == null)
                throw new ArgumentNullException(nameof(policyDefinition));

            return policyDefinition
                        .AddCallHandler<T>(new ContainerControlledLifetimeManager())
                        ;
        }
    }
#pragma warning restore CS3024 // Constraint type is not CLS-compliant
}
