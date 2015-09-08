using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Xml.Linq;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Properties;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Creates a <see cref="XmlStringValidatorAttribute"/> to validate that the target element is either <see langword="null"/>, 
    /// empty or consists of only whitespace characters string or represents a valid XML document.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class XmlStringValidatorAttribute : ValueValidatorAttribute
    {
        readonly string _rootElementLocalName;
        readonly string _rootElementNamespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStringValidatorAttribute"/> class that checks if the document is well-formed and valid.
        /// </summary>
        public XmlStringValidatorAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStringValidatorAttribute" /> class that checks if the document is well-formed, valid and
        /// the root element name is <paramref name="expandedName"/> (if specified).
        /// </summary>
        /// <param name="expandedName">
        /// The XML expanded name of the document root element in the form {namespace}localName.
        /// Can be <see langword="null"/>, empty or all whitespace characters.
        /// </param>
        public XmlStringValidatorAttribute(
            string expandedName)
        {
            if (!string.IsNullOrWhiteSpace(expandedName))
            {
                var name = XName.Get(expandedName);

                _rootElementLocalName = name.LocalName;
                _rootElementNamespace = name.NamespaceName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStringValidatorAttribute" /> class that checks if the document is well-formed, valid and
        /// the root element name is {<paramref name="rootElementNamespace"/>}<paramref name="rootElementLocalName"/>
        /// (if <paramref name="rootElementLocalName"/> is specified).
        /// </summary>
        /// <param name="rootElementLocalName">
        /// The local name of the root element.
        /// Can be <see langword="null"/>, empty or consist of whitespace characters only in which case the root element name is not checked.
        /// </param>
        /// <param name="rootElementNamespace">
        /// The namespace of the root element. Can be <see langword="null"/>, empty or consist of whitespace characters only. It will be ignored if
        /// the local name of the root element is <see langword="null"/>, empty or consist of whitespace characters only.
        /// </param>
        public XmlStringValidatorAttribute(
            string rootElementLocalName,
            string rootElementNamespace)
        {
            if (!string.IsNullOrWhiteSpace(rootElementLocalName))
            {
                _rootElementLocalName = rootElementLocalName;
                _rootElementNamespace = rootElementNamespace;
            }
        }

        /// <summary>
        /// Gets the expanded name of the document in the form <c>{namespace}element</c>.
        /// </summary>
        public string ExpandedName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                return string.IsNullOrWhiteSpace(_rootElementLocalName)
                            ? null
                            : ((string.IsNullOrWhiteSpace(_rootElementNamespace)
                                    ? XNamespace.None
                                    : XNamespace.Get(_rootElementNamespace)) + _rootElementLocalName).ToString();
            }
        }

        /// <summary>
        /// Gets the local name of the root element.
        /// </summary>
        public string RootElementLocalName => _rootElementLocalName;

        /// <summary>
        /// Gets the root element namespace.
        /// </summary>
        public string RootElementNamespace => _rootElementNamespace;

        /// <summary>
        /// Creates the validator.
        /// </summary>
        /// <param name="targetType">The type of the target to be validated. Must be <see cref="string"/>.</param>
        /// <returns>The created validator.</returns>
        /// <exception cref="ArgumentException">Thrown if the type of the target is not <see cref="string"/>.</exception>
        protected override Validator DoCreateValidator(Type targetType)
        {
            Contract.Ensures(Contract.Result<Validator>() != null);

            if (targetType != typeof(string))
                throw new ArgumentException(Resources.ExNotStringType, "targetType");

            XName rootElementName = null;

            if (!string.IsNullOrWhiteSpace(_rootElementLocalName))
            {
                var ns = !string.IsNullOrWhiteSpace(_rootElementNamespace)
                                ? XNamespace.Get(_rootElementNamespace)
                                : XNamespace.None;

                rootElementName = ns + _rootElementLocalName;
            }

            return new XmlStringValidator(
                            rootElementName,
                            GetMessageTemplate(),
                            Tag,
                            Negated);
        }
    }
}
