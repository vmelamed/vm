using System;
using System.Diagnostics.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class PositiveValidatorAttribute. Creates a validator which tests the target element if it is greater than 0, i.e. 
    /// if is a positive number. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class PositiveValidatorAttribute : ValueValidatorAttribute
    {
        /// <summary>
        /// Creates the validator.
        /// </summary>
        /// <param name="targetType">The type of the target element.</param>
        /// <returns>The validator object.</returns>
        /// <exception cref="ArgumentException">Thrown if the target element does not support <see cref="IComparable"/></exception>
        protected override Validator DoCreateValidator(
            Type targetType)
        {
            Contract.Ensures(Contract.Result<Validator>() != null);

            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            return new PositiveValidator(
                            ValidatorsConstants.GetZero(targetType),
                            GetMessageTemplate(),
                            Negated);
        }
    }
}
