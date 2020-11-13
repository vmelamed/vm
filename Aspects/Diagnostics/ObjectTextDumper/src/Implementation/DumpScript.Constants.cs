using System;
using System.Collections;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

using ExpandoEntry = System.Collections.Generic.KeyValuePair<string, object?>;
using ExpandoCollection = System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, object?>>;
using ExpandoDictionary = System.Collections.Generic.IDictionary<string, object?>;
using ExpandoEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object?>>;
using ExpandoEnumerator = System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, object?>>;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class DumpScript
    {
        const BindingFlags publicStatic                       = BindingFlags.Public|BindingFlags.Static;
        const BindingFlags publicInstance                     = BindingFlags.Public|BindingFlags.Instance;
        const BindingFlags nonpublicInstance                  = BindingFlags.NonPublic|BindingFlags.Instance;

        static readonly Type ExpandoEntryType                 = typeof(ExpandoEntry);
        static readonly Type ExpandoDictionaryType            = typeof(ExpandoDictionary);
        static readonly Type ExpandoCollectionType            = typeof(ExpandoCollection);
        static readonly Type ExpandoEnumeratorType            = typeof(ExpandoEnumerator);
        static readonly Type ExpandoEnumerableType            = typeof(ExpandoEnumerable);

        static readonly MethodInfo _miIntToString1            = typeof(int).GetMethod(nameof(int.ToString), publicInstance, null, new Type[] { typeof(IFormatProvider) }, null)!;

        static readonly MethodInfo _miReferenceEquals         = typeof(object).GetMethod(nameof(object.ReferenceEquals), publicStatic, null, new Type[] { typeof(object), typeof(object) }, null)!;
        static readonly MethodInfo _miGetType                 = typeof(object).GetMethod(nameof(object.GetType), publicInstance, null, Array.Empty<Type>(), null)!;
        static readonly MethodInfo _miToString                = typeof(object).GetMethod(nameof(object.ToString), publicInstance, null, Array.Empty<Type>(), null)!;
        static readonly MethodInfo _miEqualsObject            = typeof(object).GetMethod(nameof(object.Equals), publicInstance, null, new Type[] { typeof(object) }, null)!;

        //static readonly MethodInfo _miDispose                 = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), publicInstance)!;

        static readonly PropertyInfo _piArrayLength           = typeof(Array).GetProperty(nameof(Array.Length), publicInstance)!;

        static readonly MethodInfo _miBitConverterToString    = typeof(BitConverter).GetMethod(nameof(BitConverter.ToString), publicStatic, null, new Type[] { typeof(byte[]), typeof(int), typeof(int) }, null)!;

        //static readonly PropertyInfo _piRecurseDump           = typeof(DumpAttribute).GetProperty(nameof(DumpAttribute.RecurseDump), publicInstance)!;

        //static readonly MethodInfo _miDumpNullValues          = typeof(ClassDumpMetadata).GetMethod(nameof(ClassDumpMetadata.DumpNullValues), publicInstance, null, new Type[] { typeof(DumpAttribute) }, null)!;

        static readonly PropertyInfo _piName                  = typeof(Type).GetProperty(nameof(Type.Name), publicInstance)!;
        static readonly PropertyInfo _piNamespace             = typeof(Type).GetProperty(nameof(Type.Namespace), publicInstance)!;
        static readonly PropertyInfo _piAssemblyQualifiedName = typeof(Type).GetProperty(nameof(Type.AssemblyQualifiedName), publicInstance)!;
        static readonly PropertyInfo _piIsArray               = typeof(Type).GetProperty(nameof(Type.IsArray), publicInstance)!;
        static readonly PropertyInfo _piIsGenericType         = typeof(Type).GetProperty(nameof(Type.IsGenericType), publicInstance)!;
        static readonly MethodInfo   _miGetElementType        = typeof(Type).GetMethod(nameof(Type.GetElementType), publicInstance, null, Array.Empty<Type>(), null)!;
        static readonly MethodInfo   _miGetGenericArguments   = typeof(Type).GetMethod(nameof(Type.GetGenericArguments), publicInstance, null, Array.Empty<Type>(), null)!;

        static readonly FieldInfo    _fiDumperIndentLevel     = typeof(ObjectTextDumper).GetField(nameof(ObjectTextDumper._indentLevel), nonpublicInstance)!;
        static readonly FieldInfo    _fiDumperIndentLength    = typeof(ObjectTextDumper).GetField(nameof(ObjectTextDumper._indentSize), nonpublicInstance)!;
        static readonly PropertyInfo _piDumperMaxDepth        = typeof(ObjectTextDumper).GetProperty(nameof(ObjectTextDumper.MaxDepth), nonpublicInstance)!;
        static readonly PropertyInfo _piDumperWriter          = typeof(ObjectTextDumper).GetProperty(nameof(ObjectTextDumper.Writer), nonpublicInstance)!;
        static readonly MethodInfo   _miIndent                = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.Indent), nonpublicInstance, null, Array.Empty<Type>(), null)!;
        static readonly MethodInfo   _miUnindent              = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.Unindent), nonpublicInstance, null, Array.Empty<Type>(), null)!;
        static readonly MethodInfo   _miDumperDumpObject      = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.DumpObject), nonpublicInstance, null, new Type[] { typeof(object), typeof(Type), typeof(DumpAttribute), typeof(DumpState) }, null)!;

        static readonly MethodInfo   _miGetTypeName           = typeof(Extensions).GetMethod(nameof(Extensions.GetTypeName), BindingFlags.NonPublic|BindingFlags.Static, null, new[] { typeof(Type), typeof(bool) }, null)!;
        static readonly MethodInfo   _miGetMaxToDump          = typeof(Extensions).GetMethod(nameof(Extensions.GetMaxToDump), BindingFlags.NonPublic|BindingFlags.Static, null, new[] { typeof(DumpAttribute), typeof(int) }, null)!;

        //static readonly MethodInfo _miIndent3               = typeof(DumpUtilities).GetMethod(nameof(DumpUtilities.Indent), publicStatic, null, new Type[] { typeof(TextWriter), typeof(int), typeof(int) }, null)!;
        static readonly MethodInfo   _miUnindent3             = typeof(DumpUtilities).GetMethod(nameof(DumpUtilities.Unindent), publicStatic, null, new Type[] { typeof(TextWriter), typeof(int), typeof(int) }, null)!;

        static readonly MethodInfo   _miIsMatch               = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.IsFromSystem), publicStatic, null, new[] { typeof(Type) }, null)!;
        static readonly MethodInfo   _miDumpedBasicValue      = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedBasicValue), publicStatic, null, new Type[] { typeof(TextWriter), typeof(object), typeof(DumpAttribute) }, null)!;
        static readonly MethodInfo   _miDumpedBasicNullable   = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedBasicNullable), publicStatic, null, new Type[] { typeof(TextWriter), typeof(object), typeof(DumpAttribute) }, null)!;
        static readonly MethodInfo   _miDumpedDelegate        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.Dumped), publicStatic, null, new Type[] { typeof(TextWriter), typeof(Delegate) }, null)!;
        static readonly MethodInfo   _miDumpedMemberInfo      = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.Dumped), publicStatic, null, new Type[] { typeof(TextWriter), typeof(MemberInfo) }, null)!;
        //static readonly MethodInfo _miDumpedDictionary        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedDictionary), publicStatic, null, new Type[] { typeof(TextWriter), typeof(IEnumerable), typeof(DumpAttribute), typeof(Action<object>), typeof(Action), typeof(Action) }, null)!;
        //static readonly MethodInfo _miDumpedSequence          = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedCollection), publicStatic, null, new Type[] { typeof(TextWriter), typeof(IEnumerable), typeof(DumpAttribute), typeof(bool), typeof(Action<object>), typeof(Action), typeof(Action) }, null)!;

        // Collection enumeration methods and properties:
        static readonly PropertyInfo _piCollectionCount       = typeof(ICollection).GetProperty(nameof(ICollection.Count), publicInstance)!;
        static readonly MethodInfo   _miGetEnumerator         = typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator), publicInstance, null, Array.Empty<Type>(), null)!;
        static readonly MethodInfo   _miEnumeratorMoveNext    = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext), publicInstance, null, Array.Empty<Type>(), null)!;
        static readonly PropertyInfo _piEnumeratorCurrent     = typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current), publicInstance)!;

        // Dictionary enumeration methods and properties:
        static readonly MethodInfo   _miGetDEnumerator        = typeof(IDictionary).GetMethod(nameof(IDictionary.GetEnumerator), publicInstance, null, Array.Empty<Type>(), null)!;
        static readonly PropertyInfo _piDictionaryEntryKey    = typeof(DictionaryEntry).GetProperty(nameof(DictionaryEntry.Key), publicInstance)!;
        static readonly PropertyInfo _piDictionaryEntryValue  = typeof(DictionaryEntry).GetProperty(nameof(DictionaryEntry.Value), publicInstance)!;

        static readonly PropertyInfo _piExpandoCount          = ExpandoCollectionType.GetProperty(nameof(ExpandoCollection.Count), publicInstance)!;
        static readonly MethodInfo   _miGetExpandoEnumerator  = ExpandoEnumerableType.GetMethod(nameof(ExpandoEnumerable.GetEnumerator), publicInstance, null, Array.Empty<Type>(), null)!;
        static readonly MethodInfo   _miExpandoMoveNext       = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext), publicInstance, null, Array.Empty<Type>(), null)!;
        static readonly PropertyInfo _piExpandoCurrent        = ExpandoEnumeratorType.GetProperty(nameof(ExpandoEnumerator.Current), publicInstance)!;
        static readonly PropertyInfo _piExpandoEntryKey       = ExpandoEntryType.GetProperty(nameof(ExpandoEntry.Key), publicInstance)!;
        static readonly PropertyInfo _piExpandoEntryValue     = ExpandoEntryType.GetProperty(nameof(ExpandoEntry.Value), publicInstance)!;

        static readonly ConstantExpression _zero              = Expression.Constant(0, typeof(int));
        static readonly ConstantExpression _intMax            = Expression.Constant(int.MaxValue, typeof(int));
        static readonly ConstantExpression _empty             = Expression.Constant(string.Empty);
        static readonly ConstantExpression _false             = Expression.Constant(false);
        static readonly ConstantExpression _stringNull        = Expression.Constant(Properties.Resources.StringNull);
        static readonly ConstantExpression _typeofExpando     = Expression.Constant(typeof(ExpandoObject), typeof(Type));
        static readonly ConstantExpression _null              = Expression.Constant(null);
        static readonly ConstantExpression _nullType          = Expression.Constant(null, typeof(Type));
        static readonly ConstantExpression _nullDumpAttribute = Expression.Constant(null, typeof(DumpAttribute));
        //static readonly ConstantExpression _true              = Expression.Constant(true);
    }
}
