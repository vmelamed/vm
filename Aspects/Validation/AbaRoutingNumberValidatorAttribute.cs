using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using System;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Creates a <see cref="AbaRoutingNumberValidator"/> to validate the target element if it is a valid ABA routing number.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.ReturnValue |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class AbaRoutingNumberValidatorAttribute : ValueValidatorAttribute
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

            return new AbaRoutingNumberValidator(GetMessageTemplate(), Tag, Negated);
        }
    }
}
