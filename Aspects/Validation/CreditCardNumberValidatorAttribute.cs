using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Creates a <see cref="CreditCardNumberValidator"/> to validate the target element if it is a valid credit card number.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.ReturnValue |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class CreditCardNumberValidatorAttribute : ValueValidatorAttribute
    {
        readonly string _rexCreditCard;
        readonly Regex _regex;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardNumberValidatorAttribute"/> class.
        /// </summary>
        /// <param name="rexCreditCard">The credit card regular expression.</param>
        public CreditCardNumberValidatorAttribute(
            string rexCreditCard = RegularExpression.RexAmexMCVisa)
        {
            Contract.Requires<ArgumentNullException>(rexCreditCard!=null, nameof(rexCreditCard));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(rexCreditCard), "The argument "+nameof(rexCreditCard)+" cannot be empty or consist of whitespace characters only.");

            _rexCreditCard = rexCreditCard;
            _regex = new Regex(_rexCreditCard, RegexOptions.Compiled);
        }

        /// <summary>
        /// Gets the credit card regular expression.
        /// </summary>
        public string RexCreditCard
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                return _rexCreditCard;
            }
        }

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

            return new CreditCardNumberValidator(_regex, GetMessageTemplate(), Tag, Negated);
        }
    }
}
