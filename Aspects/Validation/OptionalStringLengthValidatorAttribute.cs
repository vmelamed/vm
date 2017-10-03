using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Describes a <see cref="OptionalStringLengthValidatorAttribute"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "May cause logical problems.")]
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class OptionalStringLengthValidatorAttribute : ValueValidatorAttribute
    {
        readonly int _lowerBound;
        readonly RangeBoundaryType _lowerBoundType;
        readonly int _upperBound;
        readonly RangeBoundaryType _upperBoundType;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="OptionalStringLengthValidatorAttribute"/> class with an upper bound constraint.
        /// </para>
        /// </summary>
        /// <param name="upperBound">The upper bound.</param>
        public OptionalStringLengthValidatorAttribute(int upperBound)
            : this(
                0,
                RangeBoundaryType.Ignore,
                upperBound,
                RangeBoundaryType.Inclusive)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="OptionalStringLengthValidatorAttribute"/> class with lower and 
        /// upper bound constraints.</para>
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        public OptionalStringLengthValidatorAttribute(
            int lowerBound,
            int upperBound)
            : this(
                lowerBound,
                RangeBoundaryType.Inclusive,
                upperBound,
                RangeBoundaryType.Inclusive)
        {
        }

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="OptionalStringLengthValidatorAttribute"/> class with fully specified bound constraints.
        /// </para>
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="lowerBoundType">The indication of how to perform the lower bound check.</param>
        /// <param name="upperBound">The upper bound.</param>
        /// <param name="upperBoundType">The indication of how to perform the upper bound check.</param>
        /// <seealso cref="RangeBoundaryType"/>
        public OptionalStringLengthValidatorAttribute(
            int lowerBound,
            RangeBoundaryType lowerBoundType,
            int upperBound,
            RangeBoundaryType upperBoundType)
        {
            _lowerBound = lowerBound;
            _lowerBoundType = lowerBoundType;
            _upperBound = upperBound;
            _upperBoundType = upperBoundType;
        }

        /// <summary>
        /// Creates the <see cref="OptionalStringLengthValidatorAttribute"/> described by the configuration object.
        /// </summary>
        /// <param name="targetType">The type of object that will be validated by the validator.</param>
        /// <returns>The created <see cref="Validator"/>.</returns>
        protected override Validator DoCreateValidator(
            Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            return new OptionalStringLengthValidator(
                _lowerBound,
                _lowerBoundType,
                _upperBound,
                _upperBoundType,
                GetMessageTemplate(),
                Negated);
        }
    }
}
