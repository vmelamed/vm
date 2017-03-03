using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using vm.Aspects.Diagnostics;

namespace vm.Aspects
{
    /// <summary>
    /// Class Extensions. Adds extension methods for easy dumping of objects, as well as a few useful reflection methods not available in .NET 4.0.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Dumps the specified <paramref name="value"/> to a text writer.
        /// </summary>
        /// <param name="value">The object to dump.</param>
        /// <param name="writer">The text writer to dump to.</param>
        /// <param name="indentLevel">The initial indent level.</param>
        /// <param name="dumpMetadata">
        /// Optional metadata class to use to extract the dump attributes from. If not specified, the dump metadata will be sought in
        /// a <see cref="MetadataTypeAttribute"/> attribute applied to the <paramref name="value"/>'s class. And if that is not found - from the 
        /// attributes applied within the class itself.
        /// </param>
        /// <param name="dumpAttribute">
        /// An explicit dump attribute to be applied at a class level. If not specified the <see cref="MetadataTypeAttribute"/> attribute applied to 
        /// <paramref name="value"/>'s class and if that is not specified - <see cref="DumpAttribute.Default"/> will be assumed.
        /// </param>
        /// <returns>
        /// The object.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [SuppressMessage("Microsoft.Security", "CA2136:TransparencyAnnotationsShouldNotConflictFxCopRule")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any exceptions here are most likely bugs which should not kill the process.")]
        [SecuritySafeCritical]
        public static object DumpText(
            this object value,
            TextWriter writer,
            int indentLevel = 0,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            try
            {
                using (var dumper = new ObjectTextDumper(writer, indentLevel))
                    dumper.Dump(value, dumpMetadata, dumpAttribute);
            }
            catch (Exception x)
            {
                writer.WriteLine($"\n\nATTENTION:\nThe TextDumper threw an exception:\n{x.ToString()}");
            }

            return value;
        }

        /// <summary>
        /// Dumps the <paramref name="value" /> to a string.
        /// </summary>
        /// <param name="value">The object to dump.</param>
        /// <param name="indentLevel">The initial indent level.</param>
        /// <param name="dumpMetadata">
        /// Optional metadata class to use to extract the dump attributes from. If not specified, the dump metadata will be sought in
        /// a <see cref="MetadataTypeAttribute"/> attribute applied to the <paramref name="value"/>'s class. And if that is not found - from the 
        /// attributes applied within the class itself.
        /// </param>
        /// <param name="dumpAttribute">
        /// An explicit dump attribute to be applied at a class level. If not specified the <see cref="MetadataTypeAttribute"/> attribute applied to 
        /// <paramref name="value"/>'s class and if that is not specified - <see cref="DumpAttribute.Default"/> will be assumed.
        /// </param>
        /// <returns>The text dump of the object.</returns>
        [SuppressMessage("Microsoft.Security", "CA2136:TransparencyAnnotationsShouldNotConflictFxCopRule")]
        [SecuritySafeCritical]
        public static string DumpString(
            this object value,
            int indentLevel = 0,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                value.DumpText(writer, indentLevel, dumpMetadata, dumpAttribute);
                return writer.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Determines whether the specified type is basic: primitive, enum, decimal, string, Guid, Uri, DateTime, TimeSpan, DateTimeOffset, IntPtr, 
        /// UIntPtr.
        /// </summary>
        /// <param name="type">The type to be tested.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is one of the basic types; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsBasicType(
            this Type type)
        {
            Contract.Requires(type != null, "type");

            return ObjectTextDumper.DumpBasicValues.ContainsKey(type) || type.IsEnum;
        }

        /// <summary>
        /// Retrieves a custom attribute of a specified type that is applied to a specified member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="attributeProvider">The member to inspect.</param>
        /// <param name="inherit"><see langword="true" /> to inspect the ancestors of element; otherwise, <see langword="false" /> (the default).</param>
        /// <returns>A custom attribute that matches <typeparamref name="T"/>, or <see langword="null" /> if no such attribute is found.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="attributeProvider"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if more than one attribute of the specified type <typeparamref name="T"/> is found.</exception>
        internal static T GetCustomAttribute<T>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit = false) where T : class
        {
            Contract.Requires(attributeProvider != null, "attributeProvider");

            if (!attributeProvider.IsDefined(typeof(T), inherit))
                return null;

            return (T)attributeProvider.GetCustomAttributes(typeof(T), inherit).Single();
        }

        /// <summary>
        /// Retrieves all custom attributes of a specified type that are applied to a specified member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="attributeProvider">The member to inspect.</param>
        /// <param name="inherit"><see langword="true" /> to inspect the ancestors of element; otherwise, <see langword="false" /> (the default).</param>
        /// <returns>A sequence of custom attributes that match <typeparamref name="T"/> (possibly empty).</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="attributeProvider"/> is <see langword="null"/>.</exception>
        internal static IEnumerable<T> GetCustomAttributes<T>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit = false) where T : class
        {
            Contract.Requires(attributeProvider != null, "attributeProvider");

            if (!attributeProvider.IsDefined(typeof(T), inherit))
                return new T[0];

            return attributeProvider.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="PropertyInfo"/> object represents a virtual property.
        /// </summary>
        /// <param name="mi">The <see cref="PropertyInfo"/> object.</param>
        /// <returns><c>true</c> if the specified <see cref="PropertyInfo"/> object represents a virtual property; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="mi"/> is <see langword="null"/>.</exception>
        internal static bool? IsVirtual(this MemberInfo mi)
        {
            Contract.Requires(mi != null, "pi");

            var pi = mi as PropertyInfo;

            if (pi == null)
                return false;

            bool? isVirtual = null;

            foreach (var accessor in pi.GetAccessors())
                if (isVirtual.HasValue)
                {
                    if (isVirtual.Value != accessor.IsVirtual)
                        return null;
                }
                else
                    isVirtual = accessor.IsVirtual;

            return isVirtual;
        }

        /// <summary>
        /// Determines whether the specified <see cref="PropertyInfo"/> object represents a property that can be read.
        /// </summary>
        /// <param name="mi">The <see cref="PropertyInfo"/> object.</param>
        /// <returns><c>true</c> if the specified <see cref="PropertyInfo"/> object represents a virtual property that can be read; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="mi"/> is <see langword="null"/>.</exception>
        internal static bool CanRead(this MemberInfo mi)
        {
            Contract.Requires(mi != null, "pi");

            if (mi is FieldInfo)
                return true;

            var pi = mi as PropertyInfo;

            if (pi == null)
                return false;

            return pi.CanRead;
        }

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
