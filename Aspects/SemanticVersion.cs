using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace vm.Aspects
{
    /// <summary>
    /// Class SemanticVersion as defined at http://semver.org/.
    /// </summary>
    public sealed class SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticVersion"/> class.
        /// </summary>
        public SemanticVersion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticVersion"/> class.
        /// </summary>
        /// <param name="major">The major version. Cannot be negative.</param>
        /// <param name="minor">The minor version. Cannot be negative.</param>
        /// <param name="patch">The patch version. Cannot be negative.</param>
        /// <param name="prerelease">The prerelease version.</param>
        /// <param name="build">The build version.</param>
        /// <exception cref="System.ArgumentException">
        /// The prerelease version numeric identifiers must not include leading zeroes.
        /// or
        /// The prerelease version numeric identifiers must not include leading zeroes.
        /// </exception>
        public SemanticVersion(
            int major,
            int minor = 0,
            int patch = 0,
            string prerelease = null,
            string build = null)
        {
            Contract.Requires<ArgumentException>(major >= 0, "The major version cannot be negative.");
            Contract.Requires<ArgumentException>(minor >= 0, "The minor version cannot be negative.");
            Contract.Requires<ArgumentException>(patch >= 0, "The patch version cannot be negative.");
            Contract.Requires<ArgumentException>(prerelease == null || RegularExpression.SemanticVersionPrerelease.IsMatch(prerelease) && ValidateParts(prerelease), "The prerelease version must comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-]. Numeric identifiers must not include leading zeroes.");
            Contract.Requires<ArgumentException>(build == null || RegularExpression.SemanticVersionPrerelease.IsMatch(build) && ValidateParts(build), "The build version must comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-]. Numeric identifiers must not include leading zeroes.");

            Major      = major;
            Minor      = minor;
            Patch      = patch;
            Prerelease = prerelease;
            Build      = build;
        }

        /// <summary>
        /// Validates the parts.
        /// </summary>
        /// <param name="prereleaseOrBuildVersionParts">The prerelease or build version parts.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [Pure]
        public static bool ValidateParts(string prereleaseOrBuildVersionParts)
        {
            if (prereleaseOrBuildVersionParts == null)
                return true;

            var parts = prereleaseOrBuildVersionParts.Split('.');

            foreach (var part in parts)
                if (part.Length > 1  &&  part.All(c => char.IsDigit(c))  &&  part[0] == '0')
                    return false;

            return true;
        }

        #region Properties
        /// <summary>
        /// Gets or sets the major component of the version.
        /// For complete description of the versioning <see cref="RegularExpression.SemanticVersion"/>.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Gets or sets the minor component of the version.
        /// For complete description of the versioning <see cref="RegularExpression.SemanticVersion"/>.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// Gets or sets the patch component of the version.
        /// For complete description of the versioning <see cref="RegularExpression.SemanticVersion"/>.
        /// </summary>
        public int Patch { get; set; }

        /// <summary>
        /// Gets or sets the prerelease component of the version.
        /// For complete description of the versioning <see cref="RegularExpression.SemanticVersion"/>.
        /// </summary>
        public string Prerelease { get; set; }

        /// <summary>
        /// Gets or sets the build component of the version.
        /// For complete description of the versioning <see cref="RegularExpression.SemanticVersion"/>.
        /// </summary>
        public string Build { get; set; }
        #endregion

        #region Identity rules implementation.
        #region IEquatable<SemanticVersion> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="SemanticVersion"/> to compare with the current object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="other"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true" /> if <paramref name="other"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="other"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true" /> if the current object and the <paramref name="other"/> are considered to be equal by value; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(SemanticVersion)"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for objects' deep equality, 
        /// i.e. they test for equality of the objects' properties and fields.
        /// </remarks>
        public bool Equals(SemanticVersion other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (GetType() != other.GetType())
                return false;

            return Major      == other.Major  &&
                   Minor      == other.Minor  &&
                   Patch      == other.Patch  &&
                   Prerelease == other.Prerelease;
        }
        #endregion

        /// <summary>
        /// Determines whether this <see cref="SemanticVersion"/> instance is equal to the specified <see cref="object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> reference to compare with this <see cref="SemanticVersion"/> object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="obj"/> cannot be cast to <see cref="SemanticVersion"/>, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true" /> if <paramref name="obj"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true" /> if the current object and the <paramref name="obj"/> are considered to be equal by value; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(SemanticVersion)"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for objects' deep equality, 
        /// i.e. they test for equality of the objects' properties and fields.
        /// </remarks>
        public override bool Equals(object obj) => Equals(obj as SemanticVersion);

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="SemanticVersion"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="SemanticVersion"/> instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            hashCode = Constants.HashMultiplier * hashCode + Major.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + Minor.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + Patch.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + (Prerelease?.GetHashCode() ?? 0);
            hashCode = Constants.HashMultiplier * hashCode + (Build?.GetHashCode() ?? 0);

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="SemanticVersion"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are considered to be equal (<see cref="Equals(SemanticVersion)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(SemanticVersion left, SemanticVersion right) => ReferenceEquals(left, null)
                                                                                ? ReferenceEquals(right, null)
                                                                                : left.Equals(right);

        /// <summary>
        /// Compares two <see cref="SemanticVersion"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(SemanticVersion)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(SemanticVersion left, SemanticVersion right) => !(left==right);
        #endregion

        #region IComparable<SemanticVersion>
        /// <summary>
        /// Compares this instance to <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>System.Int32.</returns>
        public int CompareTo(SemanticVersion other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var result = 0;

            result = Major - other.Major;

            if (result != 0)
                return result;

            result = Minor - other.Minor;

            if (result != 0)
                return result;

            result = Patch - other.Patch;

            if (result != 0)
                return result;

            if (string.IsNullOrWhiteSpace(Prerelease)  &&  string.IsNullOrWhiteSpace(other.Prerelease))
                return 0;
            if (string.IsNullOrWhiteSpace(Prerelease)  &&  !string.IsNullOrWhiteSpace(other.Prerelease))
                return 1;
            if (!string.IsNullOrWhiteSpace(Prerelease)  &&  string.IsNullOrWhiteSpace(other.Prerelease))
                return -1;

            var thisPrerelease = Prerelease.Split('.');
            var otherPrerelease = other.Prerelease.Split('.');
            var i = 0;

            do
            {
                if (thisPrerelease[i].All(c => char.IsDigit(c))  &&
                    otherPrerelease[i].All(c => char.IsDigit(c)))
                {
                    result = int.Parse(thisPrerelease[i], CultureInfo.InvariantCulture) - int.Parse(otherPrerelease[i], CultureInfo.InvariantCulture);

                    if (result != 0)
                        return result;
                }
                else
                {
                    result = string.Compare(thisPrerelease[i], otherPrerelease[i], StringComparison.Ordinal);

                    if (result != 0)
                        return result;
                }

                i++;

                if (i == thisPrerelease.Length  &&  i == otherPrerelease.Length)
                    return 0;
                if (i < thisPrerelease.Length  &&  i >= otherPrerelease.Length)
                    return 1;
                if (i >= thisPrerelease.Length  &&  i < otherPrerelease.Length)
                    return -1;
            }
            while (true);
        }

        /// <summary>
        /// Implements the operator greater than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));

            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the operator less than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator <(SemanticVersion left, SemanticVersion right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));

            return left.CompareTo(right) < 0;
        }
        #endregion

        /// <summary>
        /// Represents the instance as a string.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string ToString()
        {
            var preIsBlank = string.IsNullOrWhiteSpace(Prerelease);
            var buildIsBlank = string.IsNullOrWhiteSpace(Build);

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}.{2}{3}{4}{5}{6}",
                Major,
                Minor,
                Patch,
                !preIsBlank ? "-" : "",
                !preIsBlank ? Prerelease : "",
                !buildIsBlank ? "+" : "",
                !buildIsBlank ? Build : "");
        }

        /// <summary>
        /// Tries to parse a string into a <see cref="SemanticVersion"/> object.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="semanticVersion">The created semantic version object.</param>
        /// <returns><see langword="true"/> if the parsing was successful; otherwise <see langword="false"/>.</returns>
        public static bool TryParse(
            string version,
            out SemanticVersion semanticVersion)
        {
            Contract.Requires<ArgumentNullException>(version!=null, nameof(version));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(version), "The argument "+nameof(version)+" cannot be empty or consist of whitespace characters only.");

            semanticVersion = null;

            var match = RegularExpression.SemanticVersion.Match(version);

            if (!match.Success)
                return false;

            var ver = new SemanticVersion(
                                int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture),
                                int.Parse(match.Groups["minor"].Value, CultureInfo.InvariantCulture),
                                int.Parse(match.Groups["patch"].Value, CultureInfo.InvariantCulture));

            var prerelease = match.Groups["prerelease"];

            if (prerelease.Success)
            {
                if (!ValidateParts(prerelease.Value))
                    return false;

                ver.Prerelease = prerelease.Value;
            }

            var build = match.Groups["build"];

            if (build.Success)
            {
                if (!ValidateParts(build.Value))
                    return false;

                ver.Build = build.Value;
            }

            semanticVersion = ver;
            return true;
        }


        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(Major >= 0, "The major version cannot be negative.");
            Contract.Invariant(Minor >= 0, "The minor version cannot be negative.");
            Contract.Invariant(Patch >= 0, "The patch version cannot be negative.");
            Contract.Invariant(Prerelease == null || RegularExpression.SemanticVersionPrerelease.IsMatch(Prerelease) && ValidateParts(Prerelease), "The prerelease version must comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-]. Numeric identifiers must not include leading zeroes. ");
            Contract.Invariant(Build == null || RegularExpression.SemanticVersionPrerelease.IsMatch(Build) && ValidateParts(Build), "The build version must comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-]. Numeric identifiers must not include leading zeroes. ");
        }

    }
}
