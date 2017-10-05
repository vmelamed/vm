using System;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Policies;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Class ServiceParameterValidatingCallHandler. Call handler which validates the parameters of the service call.
    /// </summary>
    public sealed class ServiceParameterValidatingCallHandler : ParameterValidatingCallHandler
    {
        #region Constructors
        /// <summary>
        /// Creates a <see cref="ServiceParameterValidatingCallHandler"/> that uses the default ruleset and
        /// attributes configuration source to get the validation rules.
        /// </summary>
        [InjectionConstructor]
        public ServiceParameterValidatingCallHandler()
            : base("", SpecificationSource.Attributes, 0)
        {
        }

        /// <summary>
        /// Creates a <see cref="ServiceParameterValidatingCallHandler"/> that uses the given <paramref name="ruleset"/> and
        /// <paramref name="specificationSource"/> to get the validation rules.
        /// </summary>
        /// <param name="ruleset">Validation ruleset as specified in configuration.</param>
        /// <param name="specificationSource">Should the validation come from configuration file, attributes, or both?</param>
        public ServiceParameterValidatingCallHandler(
            string ruleset,
            SpecificationSource specificationSource)
            : base(ruleset, specificationSource, 0)
        {
        }

        /// <summary>
        /// Creates a <see cref="ServiceParameterValidatingCallHandler"/> that uses the given <paramref name="ruleset"/> and
        /// <paramref name="specificationSource"/> to get the validation rules.
        /// </summary>
        /// <param name="ruleset">Validation ruleset as specified in configuration.</param>
        /// <param name="specificationSource">Should the validation come from configuration file, attributes, or both?</param>
        /// <param name="handlerOrder">Order of the handler in the handlers' pipeline.</param>
        public ServiceParameterValidatingCallHandler(
            string ruleset,
            SpecificationSource specificationSource,
            int handlerOrder)
            : base(ruleset, CreateValidatorFactory(specificationSource), handlerOrder)
        {
        }

        /// <summary>
        /// Creates a <see cref="ServiceParameterValidatingCallHandler"/> that uses the given <paramref name="ruleset"/> and
        /// <c>specificationSource</c> to get the validation rules.
        /// </summary>
        /// <param name="ruleset">Validation ruleset as specified in configuration.</param>
        /// <param name="validatorFactory">
        /// An instance of <see cref="ValidatorFactory"/> to use when building the validator for the 
        /// type of a parameter, or <see langword="null"/> if no such validator is desired.
        /// </param>
        /// <param name="handlerOrder">Order of the handler in the handlers' pipeline.</param>
        public ServiceParameterValidatingCallHandler(
            string ruleset,
            ValidatorFactory validatorFactory,
            int handlerOrder)
            : base(ruleset, validatorFactory, handlerOrder)
        {
            if (validatorFactory == null)
                throw new ArgumentNullException(nameof(validatorFactory));
        }
        #endregion

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
            var typeAttribute = input.MethodBase.DeclaringType.GetCustomAttribute<CustomDataContextTypeAttribute>(true);
            var methodAttribute = input.MethodBase.GetCustomAttribute<CustomDataContextTypeAttribute>(true);

            if (typeAttribute == null  &&  methodAttribute == null)
                return base.PreInvoke(input, callData);

            var type = methodAttribute?.CustomDataContextType ?? typeAttribute?.CustomDataContextType;

            if (type == null)
                return input.CreateExceptionMethodReturn(new InvalidOperationException($"CustomDataContextTypeAttribute was specified but the type of the context/header was not."));

            var isOptional = (methodAttribute?.IsOptional) ?? (typeAttribute?.IsOptional).GetValueOrDefault();

            // validate the custom context if necessary
            var contextType = typeof(CustomDataContext<>).MakeGenericType(type);
            var contextValue = contextType.GetProperty("Current").GetValue(null);

            if (contextValue == null)
            {
                if (!isOptional)
                    return input.CreateExceptionMethodReturn(new InvalidOperationException($"The expected {type.Name} custom context object (message header) is not present."));
            }
            else
            {
                var validator = CreateValidator(contextType);
                var results   = validator.Validate(contextValue);

                if (!results.IsValid)
                    return input.CreateExceptionMethodReturn(new ArgumentValidationException(results, $"{type.Name} context object (message header)"));
            }

            return base.PreInvoke(input, callData);
        }
    }
}
