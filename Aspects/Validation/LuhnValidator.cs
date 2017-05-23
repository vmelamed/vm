using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Validates the target element if it is a valid sequence of digits, e.g. credit card number.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    public class LuhnValidator : ValueValidator<string>
    {
        readonly Regex _numberExpression = new Regex("\\d+");

        /// <summary>
        /// Initializes a new instance of the <see cref="LuhnValidator" /> class.
        /// </summary>
        /// <param name="messageTemplate">The validation message template.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="negated">if set to <see langword="true" /> the validation rule will be negated.</param>
        public LuhnValidator(
            string messageTemplate = null,
            string tag = null,
            bool negated = false)
            : base(messageTemplate, tag, negated)
        {
        }

        /// <summary>
        /// Does the actual validation.
        /// </summary>
        /// <param name="objectToValidate">The object to be validated.</param>
        /// <param name="currentTarget">The object to which the element is related to.</param>
        /// <param name="key">Specifies how the result relates to the target.</param>
        /// <param name="validationResults">The list of validation results to which the current result should be added.</param>
        protected override void DoValidate(
            string objectToValidate,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            var isValid = !string.IsNullOrWhiteSpace(objectToValidate)  &&
                          _numberExpression.IsMatch(objectToValidate, 0);

            if (isValid)
            {
                var checkSum = 0;
                var times2 = false;

                for (var i = objectToValidate.Length-1; i >= 0; i--)
                {
                    var digitNumber = objectToValidate[i] - '0';

                    if (times2)
                    {
                        digitNumber *= 2;

                        if (digitNumber <= 8)
                            checkSum += digitNumber;
                        else
                            checkSum += 1 + digitNumber % 10;
                    }
                    else
                        checkSum += digitNumber;

                    times2 = !times2;
                }

                isValid = checkSum % 10 == 0;
            }

            if (Negated)
                isValid = !isValid;

            if (!isValid)
                LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
        }

        /// <summary>
        /// Gets the default negated message template.
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.ExValidLuhnNumber;

        /// <summary>
        /// Gets the default non negated message template.
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.ExNotValidLuhnNumber;
    }
}
