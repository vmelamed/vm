using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Indicates that an implementation service class will use message validation constraints. 
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | 
        AttributeTargets.Interface | 
        AttributeTargets.Method,
        Inherited = false, 
        AllowMultiple = false)]
    public sealed class ValidatingBehaviorAttribute : Attribute, IEndpointBehavior, IContractBehavior, IOperationBehavior
    {
        IEndpointBehavior _endpointBehavior;
        IContractBehavior _contractBehavior;
        IOperationBehavior _operationBehavior;

        /// <summary>
        /// Gets the ruleset.
        /// </summary>
        public string Ruleset { get; }

        #region ValidationAttribute Members
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatingBehaviorAttribute"/> class.
        /// </summary>
        public ValidatingBehaviorAttribute()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleset"></param>
        public ValidatingBehaviorAttribute(
            string ruleset)
        {
            Ruleset = ruleset;
            SetBehaviors();
        }

        private void SetBehaviors()
        {
            ValidatingBehavior validation = new ValidatingBehavior(true, false, Ruleset);

            _endpointBehavior  = (IEndpointBehavior)validation;
            _contractBehavior  = (IContractBehavior)validation;
            _operationBehavior = (IOperationBehavior)validation;
        }

        #endregion

        #region IEndpointBehavior Members

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
            _endpointBehavior.AddBindingParameters(endpoint, bindingParameters);
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
            _endpointBehavior.ApplyClientBehavior(endpoint, clientRuntime);
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(
            ServiceEndpoint endpoint,
            EndpointDispatcher endpointDispatcher)
        {
            _endpointBehavior.ApplyDispatchBehavior(endpoint, endpointDispatcher);
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(
            ServiceEndpoint endpoint)
        {
            _endpointBehavior.Validate(endpoint);
        }

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
            _contractBehavior.AddBindingParameters(contractDescription, endpoint, bindingParameters);
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
            _contractBehavior.ApplyClientBehavior(contractDescription, endpoint, clientRuntime);
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
            _contractBehavior.ApplyDispatchBehavior(contractDescription, endpoint, dispatchRuntime);
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
            _contractBehavior.Validate(contractDescription, endpoint);
        }

        #endregion

        #region IOperationBehavior Members

        /// <summary>
        /// Configures any binding elements to support the operation behavior.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation 
        /// description is modified, the results are undefined.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(
            OperationDescription operationDescription, 
            BindingParameterCollection bindingParameters)
        {
            _operationBehavior.AddBindingParameters(operationDescription, bindingParameters);
        }

        /// <summary>
        /// Implements a modification or extension of the client across an operation.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation 
        /// description is modified, the results are undefined.</param>
        /// <param name="clientOperation">The run-time object that exposes customization properties for the operation 
        /// described by <paramref name="operationDescription"/>.</param>
        public void ApplyClientBehavior(
            OperationDescription operationDescription, 
            ClientOperation clientOperation)
        {
            _operationBehavior.ApplyClientBehavior(operationDescription, clientOperation);
        }

        /// <summary>
        /// Implements a modification or extension of the service across an operation.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation 
        /// description is modified, the results are undefined.</param>
        /// <param name="dispatchOperation">The run-time object that exposes customization properties for the operation 
        /// described by <paramref name="operationDescription"/>.</param>
        public void ApplyDispatchBehavior(
            OperationDescription operationDescription, 
            DispatchOperation dispatchOperation)
        {
            _operationBehavior.ApplyDispatchBehavior(operationDescription, dispatchOperation);
        }

        /// <summary>
        /// Implement to confirm that the operation meets some intended criteria.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation 
        /// description is modified, the results are undefined.</param>
        public void Validate(
            OperationDescription operationDescription)
        {
            _operationBehavior.Validate(operationDescription);
        }

        #endregion
    }
}
