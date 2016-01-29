using System;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class DIBehaviorAttribute. This class cannot be inherited. Should be applied to WCF service classes definitions that should not be created
    /// by WCF by using the <see cref="T:System.Activator"/> class but rather resolved from a DI container using the DI endpoint behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    [DebuggerDisplay("{GetType().Name, nq} {TargetContract!=null ? TargetContract.Name : string.Empty, nq}, resolve name: {ResolveName}")]
    public sealed class DIBehaviorAttribute : Attribute, IContractBehavior, IContractBehaviorAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DIBehaviorAttribute"/> class.
        /// </summary>
        public DIBehaviorAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DIBehaviorAttribute"/> class.
        /// </summary>
        /// <param name="resolveName">
        /// The resolution name to use when resolving the instance.
        /// Plays the same role as applying a separate <see cref="ResolveNameAttribute"/>.
        /// </param>
        public DIBehaviorAttribute(string resolveName)
            : this(resolveName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DIBehaviorAttribute"/> class.
        /// </summary>
        /// <param name="targetContract">
        /// The target contract for which the behavior will be applied. If <see langword="null"/> (the default) the behavior will be applied to all interfaces of the 
        /// service.
        /// </param>
        public DIBehaviorAttribute(Type targetContract)
            : this(null, targetContract)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DIBehaviorAttribute"/> class.
        /// </summary>
        /// <param name="resolveName">The resolution name to use when resolving the instance.</param>
        /// <param name="targetContract">
        /// The target contract for which the behavior will be applied. If <see langword="null"/> (the default) the behavior will be applied to all interfaces of the 
        /// service.
        /// </param>
        public DIBehaviorAttribute(
            string resolveName,
            Type targetContract)
        {
            if (targetContract != null && !targetContract.IsInterface)
                throw new ArgumentException("The target contract must be an interface.", "targetContract");

            ResolveName    = resolveName;
            TargetContract = targetContract;
        }

        /// <summary>
        /// Gets the DI container resolve name.
        /// </summary>
        public string ResolveName { get; }

        #region IContractBehaviorAttribute Members
        /// <summary>
        /// Gets the type of the contract to which the contract behavior is applicable.
        /// </summary>
        /// <value></value>
        /// <returns>The contract to which the contract behavior is applicable.</returns>
        public Type TargetContract { get; }
        #endregion

        #region IContractBehavior Members
        /// <summary>
        /// Configures any binding elements to support the contract behavior.
        /// </summary>
        /// <param name="contractDescription">The contract description to modify.</param>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across a contract.
        /// </summary>
        /// <param name="contractDescription">The contract description for which the extension is intended.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="clientRuntime">The client runtime.</param>
        public void ApplyClientBehavior(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across a contract.
        /// </summary>
        /// <param name="contractDescription">The contract description to be modified.</param>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="dispatchRuntime">The dispatch runtime that controls service execution.</param>
        public void ApplyDispatchBehavior(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            DispatchRuntime dispatchRuntime)
        {
            if (contractDescription==null)
                throw new ArgumentNullException("contractDescription");
            if (dispatchRuntime==null)
                throw new ArgumentNullException("dispatchRuntime");
            if (contractDescription.ContractType==null)
                throw new ArgumentException("The ContractType property cannot be null.", "contractDescription");

            dispatchRuntime.InstanceProvider = new DIInstanceProvider(contractDescription.ContractType, ResolveName);
        }

        /// <summary>
        /// Implement to confirm that the contract and endpoint can support the contract behavior.
        /// </summary>
        /// <param name="contractDescription">The contract to validate.</param>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
}
