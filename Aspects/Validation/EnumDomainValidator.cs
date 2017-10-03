using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class EnumDomainValidator. Validates whether a (nullable) enum value has a value defined in the type (or is null).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    public class EnumDomainValidator<T> : ValueValidator<T>
    {
        /// <summary>
        /// Initializes a new validator with a message template, a tag and negated flag.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="negated">if set to <see langword="true"/> the validator is negated.</param>
        /// <exception cref="System.NotSupportedException">
        /// This validator can be applied to an enum or nullable enum types only.
        /// </exception>
        public EnumDomainValidator(
            string messageTemplate,
            string tag,
            bool negated)
            : base(messageTemplate, tag, negated)
        {
            // can be either Enum or Nullable enum
            if (!typeof(T).IsEnum  &&  (!typeof(T).IsGenericType  ||
                                        typeof(T).GetGenericTypeDefinition()!=typeof(Nullable<>)  ||
                                        !typeof(T).GetGenericArguments()[0].IsEnum))
                throw new NotSupportedException("This validator can be applied to enum or nullable enum types only.");
        }

        /// <summary>
        /// Does the actual validation.
        /// </summary>
        /// <param name="objectToValidate">The object to be validated.</param>
        /// <param name="currentTarget">The object to which the element is related to.</param>
        /// <param name="key">Specifies how the result relates to the target.</param>
        /// <param name="validationResults">The list of validation results to which the current result should be added.</param>
        protected override void DoValidate(
            T objectToValidate,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            bool valid = false;

            if (typeof(T).IsEnum)
                valid = Enum.IsDefined(typeof(T), objectToValidate);
            else
                if (typeof(T).IsGenericType &&
                    typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>) &&
                    typeof(T).GetGenericArguments()[0].IsEnum)
            {
                // if it is Nullable<Enum> check for null and for the defined Enum values:
                if (objectToValidate == null)
                    valid = true;
                else
                    valid = Enum.IsDefined(
                                    typeof(T).GetGenericArguments()[0],
                                    typeof(T).GetProperty("Value")
                                             .GetValue(objectToValidate, null));
            }

            if (Negated)
                valid = !valid;

            if (!valid)
                LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
        }

        /// <summary>
        /// Gets the default negated message template.
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.ExFromEnumDomain;

        /// <summary>
        /// Gets the default non negated message template.
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.ExNotFromEnumDomain;
    }
}
