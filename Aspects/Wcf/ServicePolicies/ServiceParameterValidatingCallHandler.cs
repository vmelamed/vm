using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
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
            Contract.Requires<ArgumentNullException>(validatorFactory != null, nameof(validatorFactory));
        }
        #endregion

        #region ICallHandler
        /// <summary>
        /// Implement this method to execute your handler processing.
        /// </summary>
        /// <param name="input">Inputs to the current call to the target.</param>
        /// <param name="getNext">Delegate to execute to get the next delegate in the handler
        /// chain.</param>
        /// <returns>Return value from the target.</returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "There is no real parameter.")]
        public override IMethodReturn Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (getNext == null)
                throw new ArgumentNullException(nameof(getNext));

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
                        return input.CreateExceptionMethodReturn(new InvalidOperationException("The expected custom context object (message header) is not present."));
                }
                else
                {
                    var validator = CreateValidator(contextType);
                    var results = validator.Validate(contextValue);

                    if (!results.IsValid)
                        return input.CreateExceptionMethodReturn(new ArgumentValidationException(results, "context object (message header)"));
                }
            }

            return base.Invoke(input, getNext);
        }
        #endregion
    }
}
