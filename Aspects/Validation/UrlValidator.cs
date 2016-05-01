using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class UrlValidator. Validates if the string value to which it is applied is a valid URL string.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    public class UrlValidator : NonemptyStringValidator
    {
        readonly int _maxUrlLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlValidator"/> class.
        /// </summary>
        public UrlValidator()
        {
            _maxUrlLength = 2048;
        }

        /// <summary>
        /// Initializes a new validator.
        /// </summary>
        /// <param name="messageTemplate">The validation message template.</param>
        /// <param name="tag">The tag of the item the validator is applied to.</param>
        /// <param name="negated">if set to <see langword="true"/> the result of the validation is negated.</param>
        /// <param name="maxUrlLength">The maximum length of the URL.</param>
        public UrlValidator(
            string messageTemplate,
            string tag,
            bool negated,
            int maxUrlLength)
            : base(messageTemplate, tag, negated)
        {
            _maxUrlLength = maxUrlLength;
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
            var isValid =  !string.IsNullOrWhiteSpace(objectToValidate)  &&
                           objectToValidate.Length <= _maxUrlLength      &&
                           (RegularExpression.Url.IsMatch(objectToValidate)        ||
                            RegularExpression.UrlFull.IsMatch(objectToValidate)    ||
                            RegularExpression.WcfUrl.IsMatch(objectToValidate)     ||
                            RegularExpression.WcfUrlFull.IsMatch(objectToValidate) ||
                            RegularExpression.WcfMsmqService.IsMatch(objectToValidate));

            if (Negated)
                isValid = !isValid;

            if (!isValid)
                LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
        }

        /// <summary>
        /// Gets the default negated message template.
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.ExValidUrl;

        /// <summary>
        /// Gets the default non negated message template.
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.ExNotValidUrl;
    }
}
