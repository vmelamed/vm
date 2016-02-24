using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Wcf.FaultContracts;
using vm.Aspects.Wcf.Properties;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// The behavior class that set up the validation contract behavior
    /// for implementing the validation process.
    /// </summary>
    public class ValidatingBehavior : IEndpointBehavior, IContractBehavior, IOperationBehavior
    {
        #region ValidatingBehavior Members

        /// <summary>
        /// Internal use initializer that set the client validation flag.
        /// </summary>
        /// <param name="enabled">if set to <see langword="true"/> [enabled].</param>
        /// <param name="enableClientValidation">if set to <see langword="true"/> enables client validation.</param>
        /// <param name="ruleset"></param>
        internal ValidatingBehavior(
            bool enabled,
            bool enableClientValidation,
            string ruleset)
        {
            Enabled                = enabled;
            EnableClientValidation = enableClientValidation;
            Ruleset                = ruleset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ValidatingBehavior"/> class.
        /// The <see cref="Enabled"/> property will be set as 'true'.
        /// </summary>
        public ValidatingBehavior()
            : this(true, false, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ValidatingBehavior"/> class.
        /// The <see cref="Enabled"/> property will be set to 'true'.
        /// </summary>
        /// <param name="ruleset">The name of the validation ruleset to apply.</param>
        public ValidatingBehavior(string ruleset)
            : this(true, false, ruleset)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ValidatingBehavior"/> class.
        /// </summary>
        /// <param name="enabled">if set to <see langword="true"/> [enabled].</param>
        public ValidatingBehavior(bool enabled)
            : this(enabled, enabled, string.Empty)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:ValidatingBehavior"/> is enabled.
        /// </summary>
        /// <value><see langword="true"/> if enabled; otherwise, <see langword="false"/>. The default value is true.</value>
        public bool Enabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the client validation is enabled.
        /// </summary>
        bool EnableClientValidation { get; }

        /// <summary>
        /// Gets or sets the ruleset.
        /// </summary>
        string Ruleset { get; }

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
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            AddBindingParameters(endpoint.Contract, endpoint, bindingParameters);
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
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            ApplyClientBehavior(endpoint.Contract, endpoint, clientRuntime);
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
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if (endpointDispatcher == null)
                throw new ArgumentNullException(nameof(endpointDispatcher));

            ApplyDispatchBehavior(endpoint.Contract, endpoint, endpointDispatcher.DispatchRuntime);
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            Validate(endpoint.Contract, endpoint);
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
            if (contractDescription == null)
                throw new ArgumentNullException(nameof(contractDescription));
            if (clientRuntime == null)
                throw new ArgumentNullException(nameof(clientRuntime));

            if (!Enabled || !EnableClientValidation)
                return;

            // perform validation on client side
            foreach (ClientOperation clientOperation in clientRuntime.Operations)
                ApplyClientBehavior(
                    contractDescription.Operations.Find(clientOperation.Name),
                    clientOperation);
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
            if (contractDescription == null)
                throw new ArgumentNullException(nameof(contractDescription));
            if (dispatchRuntime == null)
                throw new ArgumentNullException(nameof(dispatchRuntime));

            if (!Enabled)
                return;

            // perform validation on server side.
            // add the fault description and validation parameters
            foreach (DispatchOperation dispatchOperation in dispatchRuntime.Operations)
                ApplyDispatchBehavior(
                    contractDescription.Operations.Find(dispatchOperation.Name),
                    dispatchOperation);
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
            if (contractDescription == null)
                throw new ArgumentNullException(nameof(contractDescription));

            // by pass validation if this behavior is disabled
            if (!Enabled)
                return;

            // check of all operations with validators has the FaultContract attribute with 
            // a ValidationFault type
            foreach (OperationDescription operation in contractDescription.Operations)
                Validate(operation);
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
            // nothing to do
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
            if (clientOperation == null)
                throw new ArgumentNullException(nameof(clientOperation));

            clientOperation.ParameterInspectors.Add(new ValidatingParameterInspector(operationDescription, Ruleset));
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
            if (operationDescription == null)
                throw new ArgumentNullException(nameof(operationDescription));
            if (dispatchOperation == null)
                throw new ArgumentNullException(nameof(dispatchOperation));

            dispatchOperation.ParameterInspectors.Add(new ValidatingParameterInspector(operationDescription, Ruleset));
        }

        /// <summary>
        /// Implement to confirm that the operation meets some intended criteria.
        /// </summary>
        /// <param name="operationDescription">The operation being examined. Use for examination only. If the operation 
        /// description is modified, the results are undefined.</param>
        public void Validate(
            OperationDescription operationDescription)
        {
            if (operationDescription == null)
                throw new ArgumentNullException(nameof(operationDescription));

            if (HasValidationAssertions(operationDescription) &&
                !IsOneWay(operationDescription) &&
                !HasFaultDescription(operationDescription))
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.MissingFaultDescription,
                        operationDescription.Name));
        }

        #endregion

        static bool HasValidationAssertions(
            OperationDescription operation)
        {
            Contract.Requires<ArgumentNullException>(operation != null, nameof(operation));

            var methodInfo = operation.SyncMethod ?? operation.BeginMethod ?? operation.TaskMethod;

            if (methodInfo == null)
                throw new ArgumentException("There is no method in the operation?");

            return methodInfo.GetCustomAttributes(typeof(ValidatorAttribute), false).Length > 0 ||
                   HasParametersWithValidationAssertions(methodInfo.GetParameters());
        }

        static bool HasFaultDescription(
            OperationDescription operation)
        {
            Contract.Requires<ArgumentNullException>(operation != null, nameof(operation));

            return operation.Faults.Any(f => f.DetailType==typeof(ValidationFault));
        }

        static bool IsOneWay(
            OperationDescription operation)
        {
            Contract.Requires<ArgumentNullException>(operation != null, nameof(operation));

            var methodInfo = operation.SyncMethod ?? operation.BeginMethod ?? operation.TaskMethod;

            if (methodInfo == null)
                throw new ArgumentException("There is no method in the operation?");

            var operationContractAttribute = methodInfo.GetCustomAttribute<OperationContractAttribute>(false);

            return operationContractAttribute.IsOneWay;
        }

        static bool HasParametersWithValidationAssertions(
            ParameterInfo[] parameters)
        {
            Contract.Requires<ArgumentNullException>(parameters != null, nameof(parameters));

            return parameters.Any(p => p.GetCustomAttributes(typeof(ValidatorAttribute), false).Length > 0);
        }
    }
}
