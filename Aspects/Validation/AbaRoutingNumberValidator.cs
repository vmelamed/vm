using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    class AbaRoutingNumberValidator : ValueValidator<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbaRoutingNumberValidator"/> class.
        /// </summary>
        public AbaRoutingNumberValidator()
            : this(null, null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbaRoutingNumberValidator" /> class.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="negated">if set to <c>true</c> the validation condition is negated.</param>
        public AbaRoutingNumberValidator(
            string messageTemplate,
            string tag,
            bool negated)
            : base(messageTemplate, tag, negated)
        {
        }

        /// <summary>
        /// Validates by comparing <paramref name="objectToValidate"/> by matching it to the ABA routing number regular expression pattern and then calculates 
        /// the check sum of the ABA RTN and compares it to the last digit of the value.
        /// </summary>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <param name="currentTarget">The object on the behalf of which the validation is performed.</param>
        /// <param name="key">The key that identifies the source of <paramref name="objectToValidate"/>.</param>
        /// <param name="validationResults">The validation results to which the outcome of the validation should be stored.</param>
        protected override void DoValidate(
            string objectToValidate,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            var isValid = !string.IsNullOrWhiteSpace(objectToValidate)  &&
                          RegularExpression.AbaRoutingNumber.IsMatch(objectToValidate);

            if (isValid)
            {
                // compute the checksum of the first 8 digits
                int check = 10 - ((objectToValidate[0] - '0') * 3 +
                                  (objectToValidate[1] - '0') * 7 +
                                  (objectToValidate[2] - '0')     +
                                  (objectToValidate[3] - '0') * 3 +
                                  (objectToValidate[4] - '0') * 7 +
                                  (objectToValidate[5] - '0')     +
                                  (objectToValidate[6] - '0') * 3 +
                                  (objectToValidate[7] - '0') * 7) % 10;

                if (check == 10)
                    check = 0;

                // compare the checksum with the ninth digit
                isValid = check == (objectToValidate[8]-'0');
            }

            if (Negated)
                isValid = !isValid;

            if (!isValid)
                LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
        }

        /// <summary>
        /// Gets the Default Message Template when the validator is not negated.
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.ExNotValidAbaRoutingNumber;

        /// <summary>
        /// Gets the Default Message Template when the validator is negated.
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.ExValidAbaRoutingNumber;
    }
}
