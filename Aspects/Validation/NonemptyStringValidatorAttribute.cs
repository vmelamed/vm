using System;
using System.Diagnostics.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Creates a <see cref="NonemptyStringValidator"/> to validate that the target string element is not <see langword="null"/>, 
    /// empty or consists of whitespace characters only.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class NonemptyStringValidatorAttribute : ValueValidatorAttribute
    {
        /// <summary>
        /// Creates the validator.
        /// </summary>
        /// <param name="targetType">The type of the target to be validated. Must be <see cref="string"/>.</param>
        /// <returns>The created validator.</returns>
        /// <exception cref="ArgumentException">Thrown if the type of the target is not <see cref="string"/>.</exception>
        protected override Validator DoCreateValidator(
            Type targetType)
        {
            Contract.Ensures(Contract.Result<Validator>() != null);

            if (targetType == null)
                throw new ArgumentNullException("targetType");

            if (targetType != typeof(string))
                throw new ArgumentException(Resources.ExNotStringType, "targetType");

            return new NonemptyStringValidator(GetMessageTemplate(), Tag, Negated);
        }
    }
}
