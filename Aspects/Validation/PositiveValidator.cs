using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Tests the target element if it is greater than 0, i.e. if is a positive number.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    public sealed class PositiveValidator : RangeValidator
    {
        /// <summary>
        /// Initializes the validator.
        /// </summary>
        public PositiveValidator(
            string messageTemplate,
            bool negated)
            : base(
                0, RangeBoundaryType.Exclusive,
                0, RangeBoundaryType.Ignore,
                messageTemplate,
                negated)
        {
        }

        /// <summary>
        /// Initializes the validator.
        /// </summary>
        public PositiveValidator(
            IComparable zero,
            string messageTemplate,
            bool negated)
            : base(
                zero, RangeBoundaryType.Exclusive,
                zero, RangeBoundaryType.Ignore,
                messageTemplate,
                negated)
        {
        }
    }
}
