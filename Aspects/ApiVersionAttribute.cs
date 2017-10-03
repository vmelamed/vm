using System;

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
            if (version.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(version));

            if (!SemanticVersion.TryParse(version, out _version))
                throw new ArgumentException("Invalid semantic version specified. See http://semver.org.", nameof(version));
        }

        /// <summary>
        /// Gets the semantic version as a <see cref="SemanticVersion"/> object.
        /// </summary>
        public SemanticVersion SemanticVersion => _version;

        /// <summary>
        /// Gets the version as a string.
        /// </summary>
        public string Version => _version.ToString();
    }
}
