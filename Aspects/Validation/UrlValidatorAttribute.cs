using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Creates a <see cref="UrlValidator"/> to validate the target element if it is a valid root URL (optional top and domain parts).
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.ReturnValue |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    public sealed class UrlValidatorAttribute : ValueValidatorAttribute
    {
        /// <summary>
        /// The default maximum URL length - 2048
        /// </summary>
        public const int DefaultMaxUrlLength = 2048;

        int _maxUrlLength = DefaultMaxUrlLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlValidatorAttribute"/> class.
        /// </summary>
        public UrlValidatorAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlValidatorAttribute"/> class.
        /// </summary>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        public UrlValidatorAttribute(int maxUrlLength)
        {
            _maxUrlLength = maxUrlLength;
        }

        /// <summary>
        /// Gets or sets the maximum length of the URL.
        /// </summary>
        public int MaxUrlLength
        {
            get { return _maxUrlLength; }
            set { _maxUrlLength = value; }
        }

        /// <summary>
        /// Creates the validator.
        /// </summary>
        /// <param name="targetType">The type of the target to be validated. Must be a <see cref="string"/>.</param>
        /// <returns>The created validator.</returns>
        /// <exception cref="ArgumentException">Thrown if the type of the target is not <see cref="string"/>.</exception>
        protected override Validator DoCreateValidator(
            Type targetType)
        {
            Contract.Ensures(Contract.Result<Validator>() != null);

            if (targetType != typeof(string))
                throw new ArgumentException(Resources.ExNotStringType, "targetType");

            return new UrlValidator(GetMessageTemplate(), Tag, Negated, _maxUrlLength);
        }
    }
}
