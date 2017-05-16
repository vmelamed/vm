using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.DumpImplementation;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects
{
    /// <summary>
    /// Class Extensions. Adds extension methods for easy dumping of objects, as well as a few useful reflection methods not available in .NET 4.0.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Matches a type name with hexadecimal suffix.
        /// </summary>
        static readonly Regex _hexadecimalSuffix = new Regex(@"_[0-9A-F]{64}", RegexOptions.Compiled);

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
        /// Dumps a LINQ expression as a C# text.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The C# text.</returns>
        public static string DumpCSharpText(
            this Expression expression)
        {
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(Contract.Result<string>().Any(c => !char.IsWhiteSpace(c)));

            if (expression == null)
                return DumpUtilities.Null;

            using (var visitor = new CSharpDumpExpression())
            {
                visitor.Visit(expression);
                return visitor.DumpText;
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
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));

            return WriterExtensions.DumpBasicValues.ContainsKey(type) || type.IsEnum;
        }

        /// <summary>
        /// Determines whether the specified object is dynamic.
        /// </summary>
        /// <param name="obj">The object to be tested.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is dynamic; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsDynamicObject(
            this object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            // TODO: we need a better test here (http://stackoverflow.com/questions/43769230/how-to-find-out-if-an-object-is-a-dynamic-object)
            return obj is IDynamicMetaObjectProvider;
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
            Contract.Requires<ArgumentNullException>(attributeProvider != null, nameof(attributeProvider));

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
            Contract.Requires<ArgumentNullException>(attributeProvider != null, nameof(attributeProvider));

            if (!attributeProvider.IsDefined(typeof(T), inherit))
                return new T[0];

            return attributeProvider.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="PropertyInfo"/> object represents a virtual property.
        /// </summary>
        /// <param name="pi">The <see cref="PropertyInfo"/> object.</param>
        /// <returns><c>true</c> if the specified <see cref="PropertyInfo"/> object represents a virtual property; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="pi"/> is <see langword="null"/>.</exception>
        internal static bool IsVirtual(
            this PropertyInfo pi)
        {
            Contract.Requires<ArgumentNullException>(pi != null, nameof(pi));

            return pi.GetMethod.IsVirtual  &&  !pi.GetMethod.IsFinal;
        }

        /// <summary>
        /// Determines whether the specified <see cref="PropertyInfo"/> object represents a property that can be read.
        /// </summary>
        /// <param name="mi">The <see cref="PropertyInfo"/> object.</param>
        /// <returns><c>true</c> if the specified <see cref="PropertyInfo"/> object represents a virtual property that can be read; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="mi"/> is <see langword="null"/>.</exception>
        internal static bool CanRead(
            this MemberInfo mi)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

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
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true" /> if the specified string is not blank; otherwise, <see langword="false" />.</returns>
        [Pure]
        public static bool IsNullOrWhiteSpace(this string value) => value?.All(c => char.IsWhiteSpace(c)) ?? true;

        /// <summary>
        /// Determines whether the specified string is null, or empty.
        /// Equivalent to <code>!string.IsNullOrEmpty(s)</code> but has the attribute <see cref="PureAttribute"/>
        /// which makes it suitable to participate in Code Contracts.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true" /> if the specified string is not blank; otherwise, <see langword="false" />.</returns>
        [Pure]
        public static bool IsNullOrEmpty(this string value) => value?.Any() ?? true;

#if DOTNET40
        static IDictionary<Type, string> _cSharpTypeName = new Dictionary<Type, string>
        {
            [typeof(bool)]    = "bool",
            [typeof(byte)]    = "byte",
            [typeof(sbyte)]   = "sbyte",
            [typeof(char)]    = "char",
            [typeof(short)]   = "short",
            [typeof(int)]     = "int",
            [typeof(long)]    = "long",
            [typeof(ushort)]  = "ushort",
            [typeof(uint)]    = "uint",
            [typeof(ulong)]   = "ulong",
            [typeof(float)]   = "float",
            [typeof(double)]  = "double",
            [typeof(decimal)] = "decimal",
            [typeof(string)]  = "string",
            [typeof(object)]  = "object",
        };
#else
        static readonly IReadOnlyDictionary<Type, string> _cSharpTypeName = new ReadOnlyDictionary<Type, string>(
            new Dictionary<Type, string>
            {
                [typeof(bool)]    = "bool",
                [typeof(byte)]    = "byte",
                [typeof(sbyte)]   = "sbyte",
                [typeof(char)]    = "char",
                [typeof(short)]   = "short",
                [typeof(int)]     = "int",
                [typeof(long)]    = "long",
                [typeof(ushort)]  = "ushort",
                [typeof(uint)]    = "uint",
                [typeof(ulong)]   = "ulong",
                [typeof(float)]   = "float",
                [typeof(double)]  = "double",
                [typeof(decimal)] = "decimal",
                [typeof(string)]  = "string",
                [typeof(object)]  = "object",
            });
#endif

        /// <summary>
        /// Gets the name of a type. In case the type is a EF dynamic proxy it will return only the first portion of the name, e.g.
        /// from the name "SomeTypeName_CFFF21E2EAC773F63711A0F93BE77F1CBC891DE8F0E5FFC46E7C4BB2E4BCC8D3" it will return only "SomeTypeName"
        /// </summary>
        /// <param name="type">The object which type name needs to be retrieved.</param>
        /// <param name="shortenEfTypes">if set to <c>true</c> the method will shorten EF dynamically generated types.</param>
        /// <returns>The type name.</returns>
        [Pure]
        internal static string GetTypeName(
            this Type type,
            bool shortenEfTypes = true)
        {
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(Contract.Result<string>().Length > 0);

            string typeName;

            if (_cSharpTypeName.TryGetValue(type, out typeName))
                return typeName;

            typeName = type.Name;

            if (shortenEfTypes       &&
                typeName.Length > 65 &&
                _hexadecimalSuffix.IsMatch(typeName.Substring(typeName.Length - 65-1)))
                typeName = type.BaseType.Name.Substring(0, typeName.Length-65);

            if (type.IsGenericType)
            {
                var tickIndex = typeName.IndexOf('`');

                if (tickIndex > -1)
                    typeName = typeName.Substring(0, tickIndex);

                typeName = $"{typeName}<{string.Join(Resources.GenericParamSeparator, type.GetGenericArguments().Select(t => GetTypeName(t, shortenEfTypes)))}>";
            }

            if (type.IsArray)
                return GetTypeName(type.GetElementType(), shortenEfTypes);

            return typeName;
        }

        internal static int GetMaxToDump(
            this DumpAttribute dumpAttribute,
            int length = int.MaxValue)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));
            Contract.Ensures(Contract.Result<int>() >= 0);

            var max = dumpAttribute.MaxLength;

            if (max < 0)
                return length;

            if (max == 0)        // limit sequences of primitive types (can be very big)
                return DumpAttribute.DefaultMaxElements;

            return Math.Min(max, length);
        }
    }
}
