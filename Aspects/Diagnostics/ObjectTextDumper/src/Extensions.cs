using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;

using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.Implementation;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects
{
    /// <summary>
    /// Class Extensions. Adds extension methods for easy dumping of objects, as well as a few useful reflection methods not available in .NET 4.0.
    /// </summary>
    public static class Extensions
    {
        const int _efHexadecimalSuffixLength = 64;
        const int _efSuffixLength = _efHexadecimalSuffixLength + 1;

        /// <summary>
        /// Matches a type name with hexadecimal suffix.
        /// </summary>
        static readonly Regex _hexadecimalSuffix = new Regex(@$"_[0-9A-F]{{{_efHexadecimalSuffixLength}}}", RegexOptions.Compiled);

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
        [SecuritySafeCritical]
        public static object? DumpText(
            this object? value,
            TextWriter writer,
            int indentLevel = 0,
            Type? dumpMetadata = null,
            DumpAttribute? dumpAttribute = null)
        {
            using var dumper = new ObjectTextDumper(writer);
            try
            {
                dumper.Dump(value, dumpMetadata, dumpAttribute, indentLevel);
            }
            catch (Exception x)
            {
                writer.WriteLine($@"

ATTENTION:
The TextDumper threw an exception:
{x}");
            }

            return value;
        }

        /// <summary>
        /// Dumps a LINQ expression as a C# text.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The C# text.</returns>
        public static string DumpCSharpText(
            this Expression expression)
        {
            using var visitor = new CSharpDumpExpression();

            visitor.Visit(expression);
            return visitor.DumpText;
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
        public static string DumpString(
            this object? value,
            int indentLevel = 0,
            Type? dumpMetadata = null,
            DumpAttribute? dumpAttribute = null)
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);

            value.DumpText(writer, indentLevel, dumpMetadata, dumpAttribute);
            return writer.GetStringBuilder().ToString();
        }

        /// <summary>
        /// Determines whether the specified type is basic: primitive, enum, decimal, string, Guid, Uri, DateTime, TimeSpan, DateTimeOffset, IntPtr,
        /// UIntPtr.
        /// </summary>
        /// <param name="type">The type to be tested.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is one of the basic types; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsBasicType(this Type type) =>
            WriterExtensions.DumpBasicValues.ContainsKey(type) || type.IsEnum;

        /// <summary>
        /// Determines whether the specified object is dynamic.
        /// </summary>
        /// <param name="obj">The object to be tested.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is dynamic; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsDynamicObject(this object obj) =>
            obj is IDynamicMetaObjectProvider;
        // TODO: we need a better test here (http://stackoverflow.com/questions/43769230/how-to-find-out-if-an-object-is-a-dynamic-object)

        /// <summary>
        /// Retrieves a custom attribute of a specified type that is applied to a specified member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="attributeProvider">The member to inspect.</param>
        /// <param name="inherit"><see langword="true" /> to inspect the ancestors of element; otherwise, <see langword="false" /> (the default).</param>
        /// <returns>A custom attribute that matches <typeparamref name="T"/>, or <see langword="null" /> if no such attribute is found.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="attributeProvider"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if more than one attribute of the specified type <typeparamref name="T"/> is found.</exception>
        internal static T? GetCustomAttribute<T>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit = false) where T : class =>
            attributeProvider.IsDefined(typeof(T), inherit)
                ? attributeProvider.GetCustomAttributes(typeof(T), inherit).Single() as T
                : null;

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
            bool inherit = false) where T : class =>
            attributeProvider is null
                ? throw new ArgumentNullException(nameof(attributeProvider))
                : attributeProvider.IsDefined(typeof(T), inherit)
                    ? attributeProvider.GetCustomAttributes(typeof(T), inherit).OfType<T>()
                    : Array.Empty<T>();

        /// <summary>
        /// Determines whether the specified <see cref="PropertyInfo"/> object represents a virtual property.
        /// </summary>
        /// <param name="pi">The <see cref="PropertyInfo"/> object.</param>
        /// <returns><c>true</c> if the specified <see cref="PropertyInfo"/> object represents a virtual property; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="pi"/> is <see langword="null"/>.</exception>
        internal static bool IsVirtual(this PropertyInfo pi)
        {
            var mi = pi.GetMethod ?? pi.SetMethod;
            return mi?.IsVirtual is true  && mi?.IsFinal is false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="PropertyInfo"/> object represents a property that can be read.
        /// </summary>
        /// <param name="mi">The <see cref="PropertyInfo"/> object.</param>
        /// <returns><c>true</c> if the specified <see cref="PropertyInfo"/> object represents a virtual property that can be read; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="mi"/> is <see langword="null"/>.</exception>
        internal static bool CanRead(this MemberInfo mi) =>
            mi switch
            {
                FieldInfo => true,
                PropertyInfo pi => pi.CanRead,
                _ => false,
            };

        /// <summary>
        /// Determines whether the specified string is null, or empty or consist of whitespace characters only.
        /// Equivalent to <c>!string.IsNullOrWhiteSpace(s)</c>.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true" /> if the specified string is not blank; otherwise, <see langword="false" />.</returns>
        public static bool IsWhiteSpace(this string value) =>
            value.All(c => char.IsWhiteSpace(c));

        /// <summary>
        /// Determines whether the specified string is null, or empty.
        /// Equivalent to <c>string.IsNullOrEmpty(s)</c>.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true" /> if the specified string is not <see langword="null"/> or empty; otherwise, <see langword="false" />.</returns>
        public static bool IsEmpty(this string value) =>
            value.Length is 0;

        /// <summary>
        /// Determines whether the specified string is null, or empty or consist of whitespace characters only.
        /// Equivalent to <c>!string.IsNullOrWhiteSpace(s)</c>.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true" /> if the specified string is not blank; otherwise, <see langword="false" />.</returns>
        public static bool IsNullOrWhiteSpace(this string? value) =>
            value?.All(c => char.IsWhiteSpace(c)) ?? true;

        /// <summary>
        /// Determines whether the specified string is null, or empty.
        /// Equivalent to <c>string.IsNullOrEmpty(s)</c>.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true" /> if the specified string is not <see langword="null"/> or empty; otherwise, <see langword="false" />.</returns>
        public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

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

        /// <summary>
        /// Gets the name of a type. In case the type is a EF dynamic proxy it will return only the first portion of the name, e.g.
        /// from the name "SomeTypeName_CFFF21E2EAC773F63711A0F93BE77F1CBC891DE8F0E5FFC46E7C4BB2E4BCC8D3" it will return only "SomeTypeName"
        /// </summary>
        /// <param name="type">The object which type name needs to be retrieved.</param>
        /// <param name="shortenEfTypes">if set to <c>true</c> the method will shorten EF dynamically generated types.</param>
        /// <returns>The type name.</returns>
        internal static string GetTypeName(
            this Type type,
            bool shortenEfTypes = true)
        {
            if (_cSharpTypeName.TryGetValue(type, out var typeName))
                return typeName;

            typeName = type.Name;

            var baseTypeName = type.BaseType?.Name;

            if (baseTypeName is null)
                return typeName;

            if (shortenEfTypes                    &&
                typeName.Length > _efSuffixLength &&
                _hexadecimalSuffix.IsMatch(typeName[^_efSuffixLength..]))
                typeName = baseTypeName[0..(typeName.Length-_efSuffixLength)];

            if (type.IsGenericType)
            {
                var tickIndex = typeName.IndexOf('`');

                if (tickIndex > -1)
                    typeName = typeName.Substring(0, tickIndex);

                typeName = $"{typeName}<{string.Join(Resources.GenericParamSeparator, type.GetGenericArguments().Select(t => GetTypeName(t, shortenEfTypes)))}>";
            }

            return type.IsArray
                ? GetTypeName(type.GetElementType()!, shortenEfTypes)
                : typeName;
        }

        /// <summary>
        /// Gets the types of the key and the value of a generic dictionary or if <paramref name="sequenceType"/> is not generic dictionary returns <see langword="null"/>.
        /// </summary>
        /// <param name="sequenceType">Type of the sequence.</param>
        /// <returns>Type[] - the types of the key (index 0) and the value (index 1) or <see langword="null"/> if <paramref name="sequenceType"/> is not generic dictionary.</returns>
        public static (Type keyType, Type valueType) DictionaryTypeArguments(this Type sequenceType)
        {
            var dictionaryType = Array.Find(sequenceType.GetInterfaces(),
                                            t => t.IsGenericType  &&
                                                 t.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (dictionaryType == null)
                return (typeof(void), typeof(void));

            var typeArguments = dictionaryType.GetGenericArguments();

            Debug.Assert(typeArguments.Length == 2);

            return typeArguments[0].IsBasicType()
                    ? (typeArguments[0], typeArguments[1])
                    : (typeof(void), typeof(void));
        }

        internal static int GetMaxToDump(
            this DumpAttribute dumpAttribute,
            int length = int.MaxValue) =>
            dumpAttribute.MaxLength < 0
                ? length
                : dumpAttribute.MaxLength == 0
                    ? Math.Min(DumpAttribute.DefaultMaxElements, length)    // force limit on sequences of primitive types (can be very big)
                    : Math.Min(dumpAttribute.MaxLength, length);
    }
}
