using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class ValidatingParameterInspector. Implements <see cref="IParameterInspector"/> using the 
    /// Enterprise Library validation application block.
    /// </summary>
    public class ValidatingParameterInspector : IParameterInspector
    {
        /// <summary>
        /// Gets the input validators.
        /// </summary>
        public IList<Validator> InputValidators { get; }

        /// <summary>
        /// Gets or sets the input validator parameter names.
        /// </summary>
        List<string> InputValidatorParameterNames { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatingParameterInspector"/> class.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="ruleset">The ruleset.</param>
        /// <exception cref="System.ArgumentNullException">operation</exception>
        public ValidatingParameterInspector(
            OperationDescription operation,
            string ruleset)
        {
            Contract.Requires<ArgumentNullException>(operation != null, nameof(operation));

            var methodInfo = operation.SyncMethod ?? operation.BeginMethod ?? operation.TaskMethod;

            if (methodInfo == null)
                throw new ArgumentException("There is no method in the operation.", nameof(operation));

            InputValidators = new List<Validator>();
            InputValidatorParameterNames = new List<string>();

            foreach (ParameterInfo param in methodInfo.GetParameters())
                switch (param.Attributes)
                {
                case ParameterAttributes.Out:
                case ParameterAttributes.Retval:
                    break;

                default:
                    InputValidators.Add(CreateInputParameterValidator(param, ruleset));
                    InputValidatorParameterNames.Add(param.Name);
                    break;
                }
        }

        /// <summary>
        /// Creates the input parameter validator.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="ruleset">The ruleset.</param>
        /// <returns>Validator.</returns>
        static Validator CreateInputParameterValidator(
            ParameterInfo param,
            string ruleset)
        {
            Contract.Requires<ArgumentNullException>(param != null, nameof(param));

            return new AndCompositeValidator(
                            ParameterValidatorFactory.CreateValidator(param),
                            Facility.ValidatorFactory.CreateValidator(param.ParameterType, ruleset));
        }

        /// <summary>
        /// Called before client calls are sent and after service responses are returned.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="inputs">The objects passed to the method by the client.</param>
        /// <returns>The correlation state that is returned as the <c>correlationState</c> parameter in 
        /// <see cref="M:System.ServiceModel.Dispatcher.IParameterInspector.AfterCall(System.String,System.Object[],System.Object,System.Object)" />. 
        /// Return null if you do not intend to use correlation state.</returns>
        /// <exception cref="System.ArgumentNullException">inputs</exception>
        /// <exception cref="FaultException{ValidationFault}"></exception>
        /// <exception cref="InvalidObjectFault"></exception>
        public object BeforeCall(
            string operationName,
            object[] inputs)
        {
            Contract.Ensures(Contract.Result<object>() == null);

            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));

            var results = new ValidationResults();

            for (var i = 0; i<InputValidators.Count(); i++)
                InputValidators[i].DoValidate(inputs[i], inputs[i], InputValidatorParameterNames[i], results);

            if (results.IsValid)
                return null;

            var validationFault = new InvalidObjectFault();

            if (WebOperationContext.Current != null)
                throw new WebFaultException<InvalidObjectFault>(
                            AddFaultDetails(validationFault, results), validationFault.HttpStatusCode);
            else
                throw new FaultException<InvalidObjectFault>(
                                AddFaultDetails(validationFault, results));
        }

        /// <summary>
        /// Called after client calls are returned and before service responses are sent.
        /// </summary>
        /// <param name="operationName">The name of the invoked operation.</param>
        /// <param name="outputs">Any output objects.</param>
        /// <param name="returnValue">The return value of the operation.</param>
        /// <param name="correlationState">Any correlation state returned from the <see cref="M:System.ServiceModel.Dispatcher.IParameterInspector.BeforeCall(System.String,System.Object[])" /> method, or null.</param>
        public void AfterCall(
            string operationName,
            object[] outputs,
            object returnValue,
            object correlationState)
        {
            // We don't need to do anything after the call
        }

        /// <summary>
        /// Adds the fault details.
        /// </summary>
        /// <param name="fault">The fault.</param>
        /// <param name="results">The results.</param>
        /// <returns>ValidationFault.</returns>
        static InvalidObjectFault AddFaultDetails(
            InvalidObjectFault fault,
            ValidationResults results)
        {
            Contract.Requires<ArgumentNullException>(results != null, nameof(results));
            Contract.Requires<ArgumentNullException>(fault != null, nameof(fault));

            if (!results.IsValid)
                foreach (ValidationResult result in results)
                    fault.Add(CreateValidationDetail(result));

            return fault;
        }

        static ValidationFaultElement CreateValidationDetail(ValidationResult result)
        {
            Contract.Requires<ArgumentNullException>(result != null, nameof(result));
            Contract.Ensures(Contract.Result<ValidationFaultElement>() != null);

            return new ValidationFaultElement { Message = result.Message, Key = result.Key, };
        }
    }
}
