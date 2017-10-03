using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class NonnegativeValidatorAttribute. Creates a validator which tests if the target element is a number equal to or greater than zero, i.e. 
    /// that it is non-negative. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class NonnegativeValidatorAttribute : ValueValidatorAttribute
    {
        /// <summary>
        /// Creates the validator.
        /// </summary>
        /// <param name="targetType">The type of the target element.</param>
        /// <returns>The created validator.</returns>
        /// <exception cref="ArgumentException">Thrown if the target does not support <see cref="IComparable"/></exception>
        protected override Validator DoCreateValidator(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            var zero = ValidatorsConstants.GetZero(targetType);

            return new NonnegativeValidator(
                            zero,
                            GetMessageTemplate(),
                            Tag,
                            Negated);
        }
    }
}
