using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// A string validator which tests if they are not null, not empty and not filled with whitespace characters only.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    public class NonemptyGuidValidator : ValueValidator<Guid>
    {
        /// <summary>
        /// Initializes a new validator with default message template and a <see langword="null"/> tag.
        /// </summary>
        public NonemptyGuidValidator()
            : base(null, null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonemptyStringValidator"/> class.
        /// </summary>
        /// <param name="messageTemplate">The validation message template.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="negated">if set to <see langword="true" /> the validation rule will be negated.</param>
        public NonemptyGuidValidator(
            string messageTemplate,
            string tag,
            bool negated)
            : base(messageTemplate, tag, negated)
        {
        }

        /// <summary>
        /// Does the actual validation.
        /// </summary>
        /// <param name="objectToValidate">The object to be validated.</param>
        /// <param name="currentTarget">The object to which the element is related to.</param>
        /// <param name="key">Specifies how the result relates to the target.</param>
        /// <param name="validationResults">The list of validation results to which the current result should be added.</param>
        protected override void DoValidate(
            Guid objectToValidate,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            var valid = Guid.Empty != objectToValidate;

            if (Negated)
                valid = !valid;
            if (!valid)
                LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
        }

        /// <summary>
        /// Gets the default negated message template.
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.ExNotEmptyGuid;

        /// <summary>
        /// Gets the default non negated message template.
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.ExEmptyGuid;
    }
}
