using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class XmlStringValidator. Validates strings representing XML documents for well-formedness and possibly validity if schema is provided.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    public class XmlStringValidator : ValueValidator<string>
    {
        /// <summary>
        /// A collection of XML schemas to check validity against.
        /// </summary>
        static XmlSchemaSet _schemas = new XmlSchemaSet();

        /// <summary>
        /// Adds the specified schema to the schema collection available for validation.
        /// </summary>
        /// <param name="schemaUrn">The schema urn.</param>
        /// <param name="filePath">The file path to the schema file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="schemaUrn"/> or <paramref name="filePath"/> are null, empty or whitespace only.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="0#")]
        public static void AddSchema(
            string schemaUrn,
            string filePath)
        {
            Contract.Requires<ArgumentNullException>(schemaUrn != null, nameof(schemaUrn));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(schemaUrn), "The argument \"schemaUrn\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(filePath != null, nameof(filePath));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(filePath), "The argument \"filePath\" cannot be null, empty or consist of whitespace characters only.");

            using (var schemaStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                _schemas.Add(
                    schemaUrn,
                    XmlReader.Create(schemaStream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }));
        }

        readonly XName _rootElementName;

        /// <summary>
        /// Initializes a new validator with default message template and a <see langword="null"/> tag.
        /// </summary>
        public XmlStringValidator()
            : base(null, null, false)
        {
            _rootElementName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStringValidator"/> class.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="negated">if set to <see langword="true" /> [negated].</param>
        public XmlStringValidator(
            string messageTemplate,
            string tag,
            bool negated)
            : base(messageTemplate, tag, negated)
        {
            _rootElementName = null;
        }

        /// <summary>
        /// Initializes a new validator with default message template and a <see langword="null"/> tag.
        /// </summary>
        public XmlStringValidator(
            XName rootElement)
            : base(null, null, false)
        {
            _rootElementName = rootElement;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStringValidator"/> class.
        /// </summary>
        /// <param name="rootElement">The root element.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="negated">if set to <see langword="true" /> [negated].</param>
        public XmlStringValidator(
            XName rootElement,
            string messageTemplate,
            string tag,
            bool negated)
            : base(messageTemplate, tag, negated)
        {
            _rootElementName = rootElement;
        }

        /// <summary>
        /// Validates the XML.
        /// </summary>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <param name="currentTarget">The current target.</param>
        /// <param name="key">The key.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns>
        /// <see langword="true" /> if the XML document in <paramref name="objectToValidate"/> is well-formed and valid,
        /// <see langword="false" /> otherwise.
        /// </returns>
        bool ValidateXml(
            string objectToValidate,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            if (string.IsNullOrWhiteSpace(objectToValidate))
            {
                if (!Negated)
                    LogValidationResult(
                        validationResults,
                        Resources.ExNullXmlDocument,
                        currentTarget,
                        key);
                return false;
            }

            XDocument xmlDocument;

            try
            {
                xmlDocument = XDocument.Parse(objectToValidate);
            }
            catch (XmlException x)
            {
                if (!Negated)
                    LogValidationResult(
                        validationResults,
                        x.Message,
                        currentTarget,
                        key);
                return false;
            }

            bool isValidXml = true;

            xmlDocument.Validate(
                    _schemas,
                    (o, x) =>
                    {
                        isValidXml = false;
                        if (!Negated)
                            LogValidationResult(
                                validationResults,
                                x.Message,
                                currentTarget,
                                key);
                    });

            if (_rootElementName != null &&
                xmlDocument.Root.Name != _rootElementName)
            {
                isValidXml = false;
                if (!Negated)
                    LogValidationResult(
                        validationResults,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.ExRootElementNotMatching,
                            xmlDocument.Root.Name.ToString(),
                            _rootElementName.ToString()),
                        currentTarget,
                        key);
            }

            return isValidXml;
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
            if (validationResults == null)
                throw new ArgumentNullException("validationResults");

            var isValidXml = ValidateXml(objectToValidate, currentTarget, key, validationResults);

            if (Negated)
                isValidXml = !isValidXml;

            if (!isValidXml  ||  !validationResults.IsValid)
                LogValidationResult(validationResults, GetMessage(objectToValidate, key), currentTarget, key);
        }

        /// <summary>
        /// Gets the default negated message template.
        /// </summary>
        protected override string DefaultNegatedMessageTemplate => Resources.ExValidXmlString;

        /// <summary>
        /// Gets the default non negated message template.
        /// </summary>
        protected override string DefaultNonNegatedMessageTemplate => Resources.ExNotValidXmlString;
    }
}
