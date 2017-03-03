using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects
{
    /// <summary>
    /// Specifies the current semantic version (http://semver.org) of the interface to which the attribute is applied.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [Serializable]
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class ApiVersionAttribute : Attribute
    {
        readonly SemanticVersion _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class with a semantic version string. See http://semver.org.
        /// </summary>
        /// <param name="version">The version.</param>
        public ApiVersionAttribute(
            string version)
        {
            Contract.Requires<ArgumentNullException>(version != null, nameof(version));
            Contract.Requires<ArgumentException>(version.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(version)+" cannot be null, empty string or consist of whitespace characters only.");

            if (!SemanticVersion.TryParse(version, out _version))
                throw new ArgumentException("Invalid semantic version specified. See http://semver.org.", nameof(version));
        }

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_version != null);
        }

        /// <summary>
        /// Gets the semantic version as a <see cref="SemanticVersion"/> object.
        /// </summary>
        public SemanticVersion SemanticVersion
        {
            get
            {
                Contract.Ensures(Contract.Result<SemanticVersion>() != null);

                return _version;
            }
        }

        /// <summary>
        /// Gets the version as a string.
        /// </summary>
        public string Version
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                Contract.Ensures(Contract.Result<string>().Any(c => !char.IsWhiteSpace(c)));

                return _version.ToString();
            }
        }
    }
}
