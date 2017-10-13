using System.Linq;

namespace vm.Aspects
{
    /// <summary>
    /// Class Extensions. Adds extension methods for easy dumping of objects, as well as a few useful reflection methods not available in .NET 4.0.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Determines whether the specified string is null, or empty or consist of whitespace characters only.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true" /> if the specified string is not blank; otherwise, <see langword="false" />.</returns>
        public static bool IsNullOrWhiteSpace(this string value) => value?.All(c => char.IsWhiteSpace(c)) ?? true;

        /// <summary>
        /// Determines whether the specified string is null, or empty.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true" /> if the specified string is not blank; otherwise, <see langword="false" />.</returns>
        public static bool IsNullOrEmpty(this string value) => value?.Any() ?? true;
    }
}
