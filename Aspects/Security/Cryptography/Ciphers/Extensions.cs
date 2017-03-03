using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects
{
    /// <summary>
    /// Class Extensions. Adds extension methods for easy dumping of objects, as well as a few useful reflection methods not available in .NET 4.0.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines whether the specified string is null, or empty or consist of whitespace characters only.
        /// Equivalent to <code>!string.IsNullOrWhiteSpace(s)</code> but has the attribute <see cref="PureAttribute"/>
        /// which makes it suitable to participate in Code Contracts.
        /// </summary>
        /// <param name="value">The s.</param>
        /// <returns><see langword="true" /> if the specified string is not blank; otherwise, <see langword="false" />.</returns>
        [Pure]
        public static bool IsNullOrWhiteSpace(this string value) => value?.All(c => char.IsWhiteSpace(c)) ?? true;
    }
}
