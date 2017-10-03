using System;
using vm.Aspects.Exceptions;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class IValidatableExtensions adds extension methods to <see cref="IValidatable"/>.
    /// </summary>
    public static class IValidatableExtensions
    {
        /// <summary>
        /// Tests whether this instance is valid according to the validation properties and methods.
        /// </summary>
        /// <param name="validatable">The object to which the method is applied.</param>
        /// <param name="ruleset">The ruleset to test the validity against.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidObjectException"></exception>
        /// <remarks>Based upon the Validation Application Block from Microsoft Enterprise Library</remarks>
        public static IValidatable ConfirmValid(
            this IValidatable validatable,
            string ruleset = "")
        {
            if (validatable == null)
                throw new ArgumentNullException(nameof(validatable));

            var results = validatable.Validate(ruleset);

            if (!results.IsValid)
                throw new InvalidObjectException(results);

            return validatable;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid according to the validation properties and methods.
        /// </summary>
        /// <param name="validatable">The object to which the method is applied.</param>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <returns><see langword="true" /> if the object is valid, otherwise <see langword="false" />.</returns>
        /// <remarks>Based upon the Validation Application Block from Microsoft Enterprise Library</remarks>
        public static bool IsValid(
            this IValidatable validatable,
            string ruleset = "")
        {
            if (validatable == null)
                throw new ArgumentNullException(nameof(validatable));

            return validatable.Validate(ruleset).IsValid;
        }

    }
}
