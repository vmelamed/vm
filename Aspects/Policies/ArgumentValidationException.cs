using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Properties;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// The exception that is thrown by the <see cref="ValidationCallHandler"/> if validation fails.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class ArgumentValidationException : ArgumentException, ISerializable
    {
        [NonSerialized]
        private ValidationResults _validationResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentValidationException"/> class, storing the validation
        /// results and the name of the parameter that failed.
        /// </summary>
        /// <param name="validationResults">The <see cref="ValidationResults"/> returned from the Validation Application Block.</param>
        /// <param name="parameterName">The parameter that failed validation.</param>
        public ArgumentValidationException(
            ValidationResults validationResults,
            string parameterName)
            : base(Resources.ValidationFailedMessage, parameterName)
        {
            _validationResults = validationResults;

            SerializeObjectState += (s, e) => e.AddSerializedState(new ValidationResultsSerializationData(validationResults));
        }

        /// <summary>
        /// Gets the validation results for the failure.
        /// </summary>
        /// <value>The validation results for the failure.</value>
        public ValidationResults ValidationResults => _validationResults;

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            StringBuilder resultBuilder = new StringBuilder(base.ToString());

            if (this._validationResults.Count > 0)
            {
                resultBuilder.AppendLine();
                resultBuilder.AppendLine();
                resultBuilder.AppendLine(Resources.ValidationResultsHeader);

                var i = 0;

                foreach (ValidationResult validationResult in _validationResults)
                {
                    if (validationResult.Key != null)
                    {
                        resultBuilder.AppendFormat(
                            CultureInfo.CurrentCulture,
                            Resources.ValidationResultWithKeyTemplate,
                            i,
                            validationResult.Message,
                            validationResult.Key);
                    }
                    else
                    {
                        resultBuilder.AppendFormat(
                            CultureInfo.CurrentCulture,
                            Resources.ValidationResultTemplate,
                            i,
                            validationResult.Message);
                    }
                    resultBuilder.AppendLine();

                    i++;
                }
            }

            return resultBuilder.ToString();
        }

        [Serializable]
        private class ValidationResultsSerializationData : ISafeSerializationData
        {
            readonly ValidationResults _validationResults;

            public ValidationResultsSerializationData(ValidationResults validationResults)
            {
                _validationResults = validationResults;
            }

            public void CompleteDeserialization(object deserialized)
            {
                var exception = (ArgumentValidationException)deserialized;
                exception._validationResults = _validationResults;
            }
        }
    }
}