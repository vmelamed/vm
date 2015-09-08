using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class NotBeforeValidatorAttribute. Creates a validator which tests if the target date and time element is not before the specified point of time.
    /// This class cannot be inherited.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(
        AttributeTargets.Method | 
        AttributeTargets.Property | 
        AttributeTargets.Field | 
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class NotBeforeValidatorAttribute : ValueValidatorAttribute
    {
        readonly DateTime _lowerBound;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotBeforeValidatorAttribute"/> class.
        /// </summary>
        /// <param name="lowerBound">The lower bound.</param>
        public NotBeforeValidatorAttribute(
            DateTime lowerBound)
        {
            _lowerBound = lowerBound;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotBeforeValidatorAttribute"/> class.
        /// </summary>
        /// <param name="lowerBound">The lower bound in ISO8601 format like "2006-01-20T00:03:20.0010000".</param>
        public NotBeforeValidatorAttribute(
            string lowerBound)
        {
            _lowerBound = DateTime.ParseExact(lowerBound, "o", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates the <see cref="T:Microsoft.Practices.EnterpriseLibrary.Validation.Validator" /> described by the attribute object providing validator specific
        /// information.
        /// </summary>
        /// <param name="targetType">The type of object that will be validated by the validator.</param>
        /// <returns>The created <see cref="T:Microsoft.Practices.EnterpriseLibrary.Validation.Validator" />.</returns>
        /// <exception cref="System.ArgumentNullException">targetType</exception>
        /// <remarks>This operation must be overridden by subclasses.</remarks>
        protected override Validator DoCreateValidator(
            Type targetType)
        {
            Contract.Ensures(Contract.Result<Validator>() != null);

            if (targetType == null)
                throw new ArgumentNullException("targetType");

            return new DateTimeRangeValidator(
                        _lowerBound,
                        RangeBoundaryType.Inclusive,
                        _lowerBound,
                        RangeBoundaryType.Ignore,
                        GetMessageTemplate(),
                        Negated);
        }
    }
}
