using System;
using System.Reflection;

using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Unity.Interception.PolicyInjection.Pipeline;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// An <see cref="ICallHandler"/> that runs validation of a call's parameters
    /// before calling the target.
    /// </summary>
    public class ValidationCallHandler : ICallHandler
    {
        /// <summary>
        /// Gets or sets the order in which the handler will be executed.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets the rule set for this call handler.
        /// </summary>
        /// <value>The rule set name.</value>
        public string Ruleset { get; }

        /// <summary>
        /// Gets the factory used to build validators.
        /// </summary>
        /// <value>The validator factory.</value>
        public ValidatorFactory ValidatorFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationCallHandler"/> class that uses the given
        /// rule set and <see cref="SpecificationSource"/> to get the validation rules.
        /// </summary>
        /// <param name="ruleset">The validation rule set specified in the configuration.</param>
        /// <param name="specificationSource">A value that indicates whether the validation should come from the configuration, from attributes, or from both sources.</param>
        public ValidationCallHandler(
            string ruleset,
            SpecificationSource specificationSource)
            : this(ruleset, specificationSource, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationCallHandler"/> class that uses the given
        /// rule set, <see cref="SpecificationSource"/> to get the validation rules, and handler order.
        /// </summary>
        /// <param name="ruleset">The validation rule set specified in the configuration.</param>
        /// <param name="specificationSource">A value that indicates whether the validation should come from the configuration, from attributes, or from both sources.</param>
        /// <param name="handlerOrder">The order of the handler.</param>
        public ValidationCallHandler(
            string ruleset,
            SpecificationSource specificationSource,
            int handlerOrder)
            : this(ruleset, GetValidatorFactory(specificationSource), handlerOrder)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationCallHandler"/> class that uses the given
        /// rule set, <see cref="ValidatorFactory"/> to get the validation rules.
        /// </summary>
        /// <param name="ruleset">The validation rule set specified in the configuration.</param>
        /// <param name="validatorFactory">The <see cref="ValidatorFactory"/> to use when building the validator for the 
        /// type of a parameter, or <see langword="null"/> if no such validator is desired.</param>
        /// <param name="handlerOrder">The order of the handler.</param>
        public ValidationCallHandler(
            string ruleset,
            ValidatorFactory validatorFactory,
            int handlerOrder)
        {
            Ruleset          = ruleset;
            ValidatorFactory = validatorFactory;
            Order            = handlerOrder;
        }

        /// <summary>
        /// Runs the call handler. This does validation on the parameters, and if validation
        /// passes it calls the handler. It throws <see cref="ArgumentValidationException"/>
        /// if validation fails.
        /// </summary>
        /// <param name="input">The <see cref="IMethodInvocation"/> that contains the details of the current call.</param>
        /// <param name="getNext">The delegate to call to get the next handler in the pipeline.</param>
        /// <returns>The return value from the target.</returns>
        public IMethodReturn Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (getNext == null)
                throw new ArgumentNullException("getNext");

            for (int index = 0; index < input.Inputs.Count; ++index)
            {
                var inputParameter = input.Inputs.GetParameterInfo(index);
                var validator = CreateValidator(inputParameter);

                var parameterValue = input.Inputs[index];
                var results = validator.Validate(parameterValue);

                if (!results.IsValid)
                    return input.CreateExceptionMethodReturn(new ArgumentValidationException(results, inputParameter.Name));
            }

            return getNext().Invoke(input, getNext);
        }

        Validator CreateValidator(
            ParameterInfo parameter)
        {
            var typeValidator      = CreateValidator(parameter.ParameterType);
            var parameterValidator = ParameterValidatorFactory.CreateValidator(parameter);

            if (typeValidator != null)
                return new AndCompositeValidator(typeValidator, parameterValidator);

            return parameterValidator;
        }

        Validator CreateValidator(Type type) => ValidatorFactory?.CreateValidator(type, Ruleset);

        static ValidatorFactory GetValidatorFactory(SpecificationSource specificationSource)
        {
            if (specificationSource == SpecificationSource.ParameterAttributesOnly)
                return null;

            switch (specificationSource)
            {
            case SpecificationSource.Both:
                return ValidationFactory.DefaultCompositeValidatorFactory;

            case SpecificationSource.Attributes:
                return new AttributeValidatorFactory();

            case SpecificationSource.Configuration:
                return ValidationFactory.DefaultConfigurationValidatorFactory;

            default:
                return null;
            }
        }
    }
}
