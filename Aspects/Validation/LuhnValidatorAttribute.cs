using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Creates a <see cref="LuhnValidator"/> to validate the target element if it is a valid sequence of digits, e.g. credit card number.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.ReturnValue |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class LuhnValidatorAttribute : ValueValidatorAttribute
    {
        /// <summary>
        /// Creates the validator.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>Validator.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        protected override Validator DoCreateValidator(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (targetType != typeof(string))
                throw new ArgumentException(Resources.ExNotStringType, nameof(targetType));

            return new LuhnValidator(GetMessageTemplate(), Tag, Negated);
        }
    }
}
