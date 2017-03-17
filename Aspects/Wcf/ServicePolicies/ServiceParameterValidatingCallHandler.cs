using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
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
            Contract.Requires<ArgumentNullException>(validatorFactory != null, nameof(validatorFactory));
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
            var attributes = input.MethodBase
                                           .GetCustomAttributes<CustomDataContextTypeAttribute>(true)
                                           .Union(
                                                input.MethodBase
                                                     .DeclaringType
                                                     .GetCustomAttributes<CustomDataContextTypeAttribute>(true)).ToList();

            // validate the custom context if necessary
            foreach (var attribute in attributes)
            {
                var contextType = typeof(CustomDataContext<>).MakeGenericType(attribute.CustomDataContextType);
                var contextValue = contextType.GetProperty("Current").GetValue(null, null);

                if (contextValue == null)
                {
                    if (!attribute.Optional &&
                        !attributes.Any(a => a.CustomDataContextType==attribute.CustomDataContextType && a.Optional))
                        return input.CreateExceptionMethodReturn(new InvalidOperationException($"The expected {attribute.CustomDataContextType.Name} custom context object (message header) is not present."));
                }
                else
                {
                    var validator = CreateValidator(contextType);
                    var results = validator.Validate(contextValue);

                    if (!results.IsValid)
                        return input.CreateExceptionMethodReturn(new ArgumentValidationException(results, $"{attribute.CustomDataContextType.Name} context object (message header)"));
                }
            }

            return base.PreInvoke(input, callData);
        }
    }
}
