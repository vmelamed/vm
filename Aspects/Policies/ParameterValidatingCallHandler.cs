using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Unity.Attributes;
using Unity.Interception.PolicyInjection.Pipeline;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Class ParameterValidatingCallHandler performs parameter validation based on validation attributes applied to the methods' parameters.
    /// </summary>
    public class ParameterValidatingCallHandler : BaseCallHandler<bool>
    {
        readonly string _ruleSet;
        readonly ValidatorFactory _validatorFactory;

        #region Constructors
        /// <summary>
        /// Creates a <see cref="ParameterValidatingCallHandler"/> that uses the default rule-set and
        /// attributes configuration source to get the validation rules.
        /// </summary>
        [InjectionConstructor]
        public ParameterValidatingCallHandler()
            : this("", SpecificationSource.Attributes, 0)
        {
        }

        /// <summary>
        /// Creates a <see cref="ParameterValidatingCallHandler"/> that uses the given <paramref name="ruleset"/> and
        /// <paramref name="specificationSource"/> to get the validation rules.
        /// </summary>
        /// <param name="ruleset">Validation rule-set as specified in configuration.</param>
        /// <param name="specificationSource">Should the validation come from configuration file, attributes, or both?</param>
        public ParameterValidatingCallHandler(
            string ruleset,
            SpecificationSource specificationSource)
            : this(ruleset, specificationSource, 0)
        {
        }

        /// <summary>
        /// Creates a <see cref="ParameterValidatingCallHandler"/> that uses the given <paramref name="ruleset"/> and
        /// <paramref name="specificationSource"/> to get the validation rules.
        /// </summary>
        /// <param name="ruleset">Validation ruleset as specified in configuration.</param>
        /// <param name="specificationSource">Should the validation come from configuration file, attributes, or both?</param>
        /// <param name="handlerOrder">Order of the handler in the handlers' pipeline.</param>
        public ParameterValidatingCallHandler(
            string ruleset,
            SpecificationSource specificationSource,
            int handlerOrder)
            : this(ruleset, CreateValidatorFactory(specificationSource), handlerOrder)
        {
        }

        /// <summary>
        /// Creates a <see cref="ParameterValidatingCallHandler"/> that uses the given <paramref name="ruleset"/>,
        /// <paramref name="validatorFactory"/> and <paramref name="handlerOrder"/> to get the validation rules.
        /// </summary>
        /// <param name="ruleset">Validation ruleset as specified in configuration.</param>
        /// <param name="validatorFactory">
        /// An instance of <see cref="ValidatorFactory"/> to use when building the validator for the 
        /// type of a parameter, or <see langword="null"/> if no such validator is desired.
        /// </param>
        /// <param name="handlerOrder">Order of the handler in the handlers' pipeline.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ParameterValidatingCallHandler(
            string ruleset,
            ValidatorFactory validatorFactory,
            int handlerOrder)
        {
            _ruleSet          = ruleset;
            _validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
            Order             = handlerOrder;
        }
        #endregion

        /// <summary>
        /// Creates the validator factory.
        /// </summary>
        /// <param name="specificationSource">The specification source.</param>
        /// <returns>A validator factory object.</returns>
        protected static ValidatorFactory CreateValidatorFactory(
            SpecificationSource specificationSource)
        {
            using (var configurationSource = ConfigurationSourceFactory.Create())
                switch (specificationSource)
                {
                case SpecificationSource.Both:
                    return new CompositeValidatorFactory(
                                    new AttributeValidatorFactory(),
                                    new ConfigurationValidatorFactory(configurationSource));

                case SpecificationSource.Attributes:
                    return new AttributeValidatorFactory();

                case SpecificationSource.Configuration:
                    return new ConfigurationValidatorFactory(configurationSource);

                case SpecificationSource.ParameterAttributesOnly:
                default:
                    throw new InvalidOperationException("Invalid specification source.");
                }
        }

        /// <summary>
        /// Process the input and the context before the control is passed down the aspects pipeline.
        /// For various reasons it may cut the pipeline short by returning non-<see langword="null" />, e.g. due to an invalid parameter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="callData">The per-call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected override IMethodReturn PreInvoke(
            IMethodInvocation input,
            bool callData)
        {
            // validate the parameters
            for (int index = 0; index < input.Inputs.Count; ++index)
            {
                var inputParameter = input.Inputs.GetParameterInfo(index);
                var parameterValue = input.Inputs[index];

                if (inputParameter.ParameterType.IsNullable() &&
                    parameterValue == null)
                    continue;

                var results = CreateValidator(inputParameter, parameterValue)
                                    .Validate(parameterValue);

                if (!results.IsValid)
                    return input.CreateExceptionMethodReturn(new ArgumentValidationException(results, inputParameter.Name));
            }

            return null;
        }

        /// <summary>
        /// Creates a validator for the parameter represented by <paramref name="parameterInfo"/>.
        /// </summary>
        /// <param name="parameterInfo">The parameter reflection information.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>The created validator.</returns>
        Validator CreateValidator(
            ParameterInfo parameterInfo,
            object parameterValue)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            // get the validators defined on the parameter itself
            var parameterValidator = CreateParameterValidator(parameterInfo, parameterValue);

            Validator typeValidator = null;
            var parameterType = parameterValue?.GetType();

            // get the parameters defined on the parameter's type
            if (parameterType != null &&
                parameterType != parameterInfo.ParameterType &&
                !IsBasicType(parameterType))
            {
                // if there are type validators - compose them with the parameter validators
                typeValidator = CreateValidator(parameterValue.GetType());

                if (typeValidator != null)
                    return new AndCompositeValidator(typeValidator, parameterValidator);
            }

            return parameterValidator;
        }

        /// <summary>
        /// Creates parameter validators.
        /// </summary>
        /// <param name="parameterInfo">The parameter info of the parameter for which to create the validator.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>The created validator.</returns>
        /// <remarks>
        /// The parameter validator must know the actual type of the parameter value. 
        /// EL has a bug where they get the type of the parameter from the parameter info instead of the parameter itself - 
        /// the parameter may be of a derived type of the formal parameter.
        /// </remarks>
        static Validator CreateParameterValidator(
            ParameterInfo parameterInfo,
            object parameterValue)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            var parameterElement = new MetadataValidatedParameterElement();

            // gets the validators metadata
            parameterElement.UpdateFlyweight(parameterInfo);

            // compositions all validators
            var compositeBuilder = new CompositeValidatorBuilder(parameterElement);

            foreach (var descriptor in parameterElement.GetValidatorDescriptors())
                // get the type of the parameter either from the parameter value or from the parameter info if the value is null
                compositeBuilder.AddValueValidator(
                    descriptor.CreateValidator(
                        parameterValue != null
                            ? parameterValue.GetType()
                            : parameterInfo.ParameterType,
                        null,
                        null,
                        ValidationFactory.DefaultCompositeValidatorFactory));

            // returns all validators
            return compositeBuilder.GetValidator();
        }

        /// <summary>
        /// Creates the validator - <see langword="null"/> handling validator factory wrapper.
        /// </summary>
        /// <param name="type">The type to create the validator for.</param>
        /// <returns>The created validator.</returns>
        protected Validator CreateValidator(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return _validatorFactory.CreateValidator(type, _ruleSet);
        }

        /// <summary>
        /// Determines whether <paramref name="type"/> is a basic type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><see langword="true" /> if <paramref name="type"/> is a basic type; otherwise, <see langword="false" />.</returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        static bool IsBasicType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return
                type.IsEnum                    ||
                type.IsPrimitive               ||
                type == typeof(string)         ||
                type == typeof(decimal)        ||
                type == typeof(Guid)           ||
                type == typeof(Uri)            ||
                type == typeof(DateTime)       ||
                type == typeof(TimeSpan)       ||
                type == typeof(DateTimeOffset) ||
                type == typeof(DBNull);
        }
    }
}
