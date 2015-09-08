using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Performs validation on strings by comparing their lengths to the specified boundaries. 
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> is not logged as a failure.
    /// </remarks>
    [ConfigurationElementType(typeof(StringLengthValidatorData))]
    public sealed class OptionalStringLengthValidator : StringLengthValidator
    {
        /// <summary>
        /// <para>Initializes a new instance of the <see cref="OptionalStringLengthValidator"/> class with an upper bound constraint.</para>
        /// </summary>
        /// <param name="upperBound">The upper bound.</param>
        /// <remarks>
        /// No lower bound constraints will be checked by this instance, and the upper bound check will be <see cref="RangeBoundaryType.Inclusive"/>.
        /// </remarks>
        public OptionalStringLengthValidator(
            int upperBound)
            : base(upperBound)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="OptionalStringLengthValidator"/> class with an upper bound constraint.</para>
        /// </summary>
        /// <param name="upperBound">The upper bound.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        /// <remarks>
        /// No lower bound constraints will be checked by this instance, and the upper bound check will be <see cref="RangeBoundaryType.Inclusive"/>.
        /// </remarks>
        public OptionalStringLengthValidator(
            int upperBound,
            bool negated)
            : base(upperBound, negated)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="OptionalStringLengthValidator"/> class with lower and 
        /// upper bound constraints.</para>
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        /// <remarks>
        /// Both bound checks will be <see cref="RangeBoundaryType.Inclusive"/>.
        /// </remarks>
        public OptionalStringLengthValidator(
            int lowerBound,
            int upperBound)
            : base(lowerBound, upperBound)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="OptionalStringLengthValidator"/> class with lower and 
        /// upper bound constraints.</para>
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        /// <remarks>
        /// Both bound checks will be <see cref="RangeBoundaryType.Inclusive"/>.
        /// </remarks>
        public OptionalStringLengthValidator(
            int lowerBound,
            int upperBound,
            bool negated)
            : base(
                lowerBound,
                upperBound,
                negated)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="OptionalStringLengthValidator"/> class with fully specified
        /// bound constraints.</para>
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="lowerBoundType">The indication of how to perform the lower bound check.</param>
        /// <param name="upperBound">The upper bound.</param>
        /// <param name="upperBoundType">The indication of how to perform the upper bound check.</param>
        /// <seealso cref="RangeBoundaryType"/>
        public OptionalStringLengthValidator(
            int lowerBound,
            RangeBoundaryType lowerBoundType,
            int upperBound,
            RangeBoundaryType upperBoundType)
            : base(
                lowerBound,
                lowerBoundType,
                upperBound,
                upperBoundType)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="OptionalStringLengthValidator"/> class with fully specified
        /// bound constraints.</para>
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="lowerBoundType">The indication of how to perform the lower bound check.</param>
        /// <param name="upperBound">The upper bound.</param>
        /// <param name="upperBoundType">The indication of how to perform the upper bound check.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        /// <seealso cref="RangeBoundaryType"/>
        public OptionalStringLengthValidator(
            int lowerBound,
            RangeBoundaryType lowerBoundType,
            int upperBound,
            RangeBoundaryType upperBoundType,
            bool negated)
            : base(
                lowerBound,
                lowerBoundType,
                upperBound,
                upperBoundType,
                negated)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalStringLengthValidator"/> class with fully specified
        /// bound constraints and a message template.
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="lowerBoundType">The indication of how to perform the lower bound check.</param>
        /// <param name="upperBound">The upper bound.</param>
        /// <param name="upperBoundType">The indication of how to perform the upper bound check.</param>
        /// <param name="messageTemplate">The message template to use when logging results.</param>
        /// <seealso cref="RangeBoundaryType"/>
        public OptionalStringLengthValidator(
            int lowerBound,
            RangeBoundaryType lowerBoundType,
            int upperBound,
            RangeBoundaryType upperBoundType,
            string messageTemplate)
            : base(
                lowerBound,
                lowerBoundType,
                upperBound,
                upperBoundType,
                messageTemplate)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="OptionalStringLengthValidator"/> class with fully specified
        /// bound constraints and a message template.</para>
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="lowerBoundType">The indication of how to perform the lower bound check.</param>
        /// <param name="upperBound">The upper bound.</param>
        /// <param name="upperBoundType">The indication of how to perform the upper bound check.</param>
        /// <param name="messageTemplate">The message template to use when logging results.</param>
        /// <param name="negated">True if the validator must negate the result of the validation.</param>
        /// <seealso cref="RangeBoundaryType"/>
        public OptionalStringLengthValidator(
            int lowerBound,
            RangeBoundaryType lowerBoundType,
            int upperBound,
            RangeBoundaryType upperBoundType,
            string messageTemplate,
            bool negated)
            : base(
                lowerBound,
                lowerBoundType,
                upperBound,
                upperBoundType,
                messageTemplate,
                negated)
        {
        }

        /// <summary>
        /// Validates by comparing the length for <paramref name="objectToValidate"/> with the constraints
        /// specified for the validator.
        /// </summary>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <param name="currentTarget">The object on the behalf of which the validation is performed.</param>
        /// <param name="key">The key that identifies the source of <paramref name="objectToValidate"/>.</param>
        /// <param name="validationResults">The validation results to which the outcome of the validation should be stored.</param>
        /// <remarks>
        /// <see langword="null"/> is <b>not</b> considered a failed validation.
        /// </remarks>
        protected override void DoValidate(
            string objectToValidate,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            if (objectToValidate != null)
                base.DoValidate(objectToValidate, currentTarget, key, validationResults);
            else
                if (Negated)
                    LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
        }

    }
}
