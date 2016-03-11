using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Validates credit card account numbers
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    public class CreditCardNumberValidator : ValueValidator<string>
    {
        readonly Regex _creditCardRegularExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardNumberValidator" /> class.
        /// </summary>
        /// <param name="creditCardRegularExpression">The credit card number regular expression (e.g. pick one from the RegularExpression class).</param>
        public CreditCardNumberValidator(
            Regex creditCardRegularExpression)
            : this(creditCardRegularExpression, null, null, false)
        {
            Contract.Requires<ArgumentNullException>(creditCardRegularExpression != null, "creditCardRegularExpression");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardNumberValidator" /> class.
        /// </summary>
        /// <param name="creditCardRegularExpression">The credit card number regular expression (e.g. pick one from the RegularExpression class).</param>
        /// <param name="messageTemplate">The validation message template.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="negated">if set to <see langword="true" /> the validation rule will be negated.</param>
        public CreditCardNumberValidator(
            Regex creditCardRegularExpression,
            string messageTemplate,
            string tag,
            bool negated)
            : base(messageTemplate, tag, negated)
        {
            Contract.Requires<ArgumentNullException>(creditCardRegularExpression != null, "creditCardRegularExpression");

            _creditCardRegularExpression = creditCardRegularExpression;
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
                          _creditCardRegularExpression.IsMatch(objectToValidate, 0);

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
        protected override string DefaultNegatedMessageTemplate => Resources.ExValidCcNumber;

        /// <summary>
        /// Gets the default non negated message template.
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.ExNotValidCcNumber;
    }
}
