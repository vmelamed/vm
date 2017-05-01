using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    static class WriterExtensions
    {
        static readonly IReadOnlyDictionary<Type,Func<object,ulong>> _castsToUlong = new ReadOnlyDictionary<Type,Func<object,ulong>>(
            new Dictionary<Type,Func<object,ulong>>()
            {
                [typeof(byte)]           = v => (ulong)(byte)v,
                [typeof(sbyte)]          = v => (ulong)(sbyte)v & 0xFF,
                [typeof(char)]           = v => (ulong)(char)v,
                [typeof(short)]          = v => (ulong)(short)v & 0xFFFF,
                [typeof(ushort)]         = v => (ulong)(ushort)v,
                [typeof(int)]            = v => (ulong)(int)v & 0xFFFFFFFF,
                [typeof(uint)]           = v => (ulong)(uint)v,
                [typeof(long)]           = v => (ulong)(long)v,
                [typeof(ulong)]          = v => (ulong)v,
            });

        static readonly IReadOnlyDictionary<Type, Action<TextWriter, object, int>> _dumpBasicValues = new ReadOnlyDictionary<Type, Action<TextWriter, object, int>>(
            new Dictionary<Type, Action<TextWriter, object, int>>
            {
                [typeof(DBNull)]         = (w, v, max) => w.Write("DBNull"),
                [typeof(bool)]           = (w, v, max) => w.Write((bool)v),
                [typeof(byte)]           = (w, v, max) => w.Write((byte)v),
                [typeof(sbyte)]          = (w, v, max) => w.Write((sbyte)v),
                [typeof(char)]           = (w, v, max) => w.Write((char)v),
                [typeof(short)]          = (w, v, max) => w.Write((short)v),
                [typeof(int)]            = (w, v, max) => w.Write((int)v),
                [typeof(long)]           = (w, v, max) => w.Write((long)v),
                [typeof(ushort)]         = (w, v, max) => w.Write((ushort)v),
                [typeof(uint)]           = (w, v, max) => w.Write((uint)v),
                [typeof(ulong)]          = (w, v, max) => w.Write((ulong)v),
                [typeof(float)]          = (w, v, max) => w.Write((float)v),
                [typeof(double)]         = (w, v, max) => w.Write((double)v),
                [typeof(decimal)]        = (w, v, max) => w.Write((decimal)v),
                [typeof(DateTime)]       = (w, v, max) => w.Write($"{v:o}"),
                [typeof(DateTimeOffset)] = (w, v, max) => w.Write($"{v:o}"),
                [typeof(TimeSpan)]       = (w, v, max) => w.Write(v.ToString()),
                [typeof(Uri)]            = (w, v, max) => w.Write(v.ToString()),
                [typeof(Guid)]           = (w, v, max) => w.Write(v.ToString()),
                [typeof(IntPtr)]         = (w, v, max) => w.Write($"0x{v:x16}"),
                [typeof(UIntPtr)]        = (w, v, max) => w.Write($"0x{v:x16}"),
                [typeof(string)]         = (w, v, max) => w.Dumped((string)v, max),
            });

        /// <summary>
        /// Matches the name space of the types within System
        /// </summary>
        static readonly Regex _systemNameSpace = new Regex(Resources.RegexSystemNamespace, RegexOptions.Compiled);

        public static bool IsFromSystem(
            this Type type) => _systemNameSpace.IsMatch(type.Namespace);

        public static IReadOnlyDictionary<Type, Action<TextWriter, object, int>> DumpBasicValues => _dumpBasicValues;

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        static bool Dumped(
            this TextWriter writer,
            string @string,
            int max)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (@string == null)
                return false;

            writer.Write("{0}", max >= 0  &&  max < @string.Length
                                        ? @string.Substring(0, max) + "..."
                                        : @string);
            return true;
        }

        static void DumpEnumValue(
            this TextWriter writer,
            object v,
            int max)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(v != null, nameof(v));

            var type = v.GetType();

            if (type.GetCustomAttribute<FlagsAttribute>() == null)
            {
                writer.Write(
                    DumpFormat.Enum,
                    type.Name,
                    type.Namespace,
                    type.AssemblyQualifiedName,
                    v.ToString());
                return;
            }

            var names       = type.GetEnumNames();
            var values      = type.GetEnumValues();
            var castToUlong = _castsToUlong[type.GetEnumUnderlyingType()];
            var ulongValue  = castToUlong(v);
            var nameLookup  = new Dictionary<ulong, string>(values.Length);

            for (var i = 0; i<values.Length; i++)
                nameLookup[castToUlong(values.GetValue(i))] = names[i];

            writer.Write(
                DumpFormat.EnumFlagPrefix,
                type.Name,
                type.Namespace,
                type.AssemblyQualifiedName);

            var first = true;

            for (ulong flag = 1; flag <= castToUlong(values.GetValue(values.Length-1)); flag <<= 1)
                if ((ulongValue & flag) != 0)
                {
                    if (first)
                        first = false;
                    else
                        writer.Write(DumpFormat.EnumFlagSeparator);
                    writer.Write(
                        DumpFormat.EnumFlag,
                        type.Name,
                        type.Namespace,
                        type.AssemblyQualifiedName,
                        nameLookup[flag]);
                }

            writer.Write(
                DumpFormat.EnumFlagSuffix,
                type.Name,
                type.Namespace,
                type.AssemblyQualifiedName);
        }

        /// <summary>
        /// Dumps the value of basic types (all primitive types plus <see cref="String" />, <see cref="DateTime" />, <see cref="DateTimeOffset" />,
        /// <see cref="TimeSpan" />, <see cref="Decimal" /> enum-s, <see cref="Guid" />, Uri).
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="dumpAttribute">The dump attribute associated with the value (e.g. property attribute).</param>
        /// <returns><c>true</c> if the value was dumped; otherwise <c>false</c> (e.g. the value is struct)</returns>
        public static bool DumpedBasicValue(
            this TextWriter writer,
            object value,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (value == null)
            {
                writer.Write(DumpUtilities.Null);
                return true;
            }

            if (!value.GetType().IsBasicType())
                return false;

            // store the max dump length here
            var dumpMaxLength = int.MaxValue;

            if (dumpAttribute != null)
            {
                // should we mask it - dump the mask and return
                if (dumpAttribute.Mask)
                {
                    writer.Write(dumpAttribute.MaskValue);
                    return true;
                }

                // special formatting - dump blindly and return
                if (dumpAttribute.ValueFormat != DumpFormat.Value)
                {
                    if (dumpAttribute.ValueFormat == Resources.ValueFormatToString)
                        writer.Write(value.ToString());
                    else
                        writer.Write(dumpAttribute.ValueFormat, value);
                    return true;
                }

                // get the max dump length
                if (dumpAttribute.MaxLength > 0)
                    dumpMaxLength = dumpAttribute.MaxLength;
            }

            Action<TextWriter, object, int> dump;
            var type = value.GetType();

            if (!_dumpBasicValues.TryGetValue(type, out dump))
                if (type.IsEnum)
                    dump = DumpEnumValue;
                else
                    dump = (w, v, max) => w.Write($"[{type.Name} value]");

            dump(writer, value, dumpMaxLength);
            return true;
        }

        public static bool Dumped(
            this TextWriter writer,
            Delegate @delegate)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (@delegate == null)
                return false;

            writer.Write(
                DumpFormat.Delegate,
                @delegate.Method.DeclaringType!=null ? @delegate.Method.DeclaringType.Name : string.Empty,
                @delegate.Method.DeclaringType!=null ? @delegate.Method.DeclaringType.Namespace : string.Empty,
                @delegate.Method.DeclaringType!=null ? @delegate.Method.DeclaringType.AssemblyQualifiedName : string.Empty,
                @delegate.Method.Name,
                @delegate.Target==null
                    ? Resources.ClassMethodDesignator
                    : Resources.InstanceMethodDesignator);

            return true;
        }

        public static bool Dumped(
            this TextWriter writer,
            MemberInfo memberInfo)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (memberInfo == null)
                return false;

            writer.Write(
                DumpFormat.MemberInfoMemberType,
                memberInfo.MemberType.ToString());

            Func<TextWriter, MemberInfo, bool> dump = null;
            Type miType = memberInfo.GetType();

            do
            {
                if (_memberInfoDumpers.TryGetValue(miType, out dump))
                    return dump(writer, memberInfo);

                miType = miType.BaseType;
            }
            while (miType != typeof(MemberInfo));

            // last resort:
            writer.Write(memberInfo.Name);
            return true;
        }

        public static IDictionary<Type, Func<TextWriter, MemberInfo, bool>> _memberInfoDumpers = new Dictionary<Type, Func<TextWriter, MemberInfo, bool>>
        {
            [typeof(Type)]              = (w,mi) => w.Dumped((Type)mi),
            [typeof(TypeInfo)]          = (w,mi) => w.Dumped((Type)mi),

            [typeof(EventInfo)]         = (w,mi) => w.Dumped((EventInfo)mi),
            [typeof(EventInfo)]         = (w,mi) => w.Dumped((EventInfo)mi),
            [typeof(ComAwareEventInfo)] = (w,mi) => w.Dumped((EventInfo)mi),

            [typeof(FieldInfo)]         = (w,mi) => w.Dumped((FieldInfo)mi),
            [typeof(FieldBuilder)]      = (w,mi) => w.Dumped((FieldInfo)mi),

            [typeof(PropertyInfo)]      = (w,mi) => w.Dumped((PropertyInfo)mi),
            [typeof(PropertyBuilder)]   = (w,mi) => w.Dumped((PropertyInfo)mi),

            [typeof(MethodInfo)]        = (w,mi) => w.Dumped((MethodInfo)mi),
        };

        static bool Dumped(
            this TextWriter writer,
            Type type)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (type == null)
                return false;

            writer.Write(
                DumpFormat.TypeInfo,
                type.Name,
                type.Namespace,
                type.AssemblyQualifiedName);

            return true;
        }

        static bool Dumped(
            this TextWriter writer,
            EventInfo eventInfo)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (eventInfo == null)
                return false;

            writer.Write(
                DumpFormat.MethodInfo,
                eventInfo.EventHandlerType.Name,
                eventInfo.EventHandlerType.Namespace,
                eventInfo.EventHandlerType.AssemblyQualifiedName,
                eventInfo.DeclaringType.Name,
                eventInfo.DeclaringType.Namespace,
                eventInfo.DeclaringType.AssemblyQualifiedName,
                eventInfo.Name);

            return true;
        }

        static bool Dumped(
            this TextWriter writer,
            FieldInfo fieldInfo)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (fieldInfo == null)
                return false;

            writer.Write(
                DumpFormat.MethodInfo,
                fieldInfo.FieldType.Name,
                fieldInfo.FieldType.Namespace,
                fieldInfo.FieldType.AssemblyQualifiedName,
                fieldInfo.DeclaringType.Name,
                fieldInfo.DeclaringType.Namespace,
                fieldInfo.DeclaringType.AssemblyQualifiedName,
                fieldInfo.Name);

            return true;
        }

        static bool Dumped(
            this TextWriter writer,
            PropertyInfo propertyInfo)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (propertyInfo == null)
                return false;

            var indexes = propertyInfo.GetIndexParameters();

            writer.Write(
                DumpFormat.MethodInfo,
                propertyInfo.PropertyType.Name,
                propertyInfo.PropertyType.Namespace,
                propertyInfo.PropertyType.AssemblyQualifiedName,
                propertyInfo.DeclaringType.Name,
                propertyInfo.DeclaringType.Namespace,
                propertyInfo.DeclaringType.AssemblyQualifiedName,
                indexes.Length == 0
                    ? propertyInfo.Name
                    : Resources.IndexerStart);

            if (indexes.Length > 0)
            {
                for (var i = 0; i<indexes.Length; i++)
                {
                    if (i > 0)
                        writer.Write(Resources.ParametersSeparator);
                    writer.Write(
                        DumpFormat.IndexerIndexType,
                        indexes[i].ParameterType.Name,
                        indexes[i].ParameterType.Namespace,
                        indexes[i].ParameterType.AssemblyQualifiedName);
                }
                writer.Write(Resources.IndexerEnd);
            }

            writer.Write(Resources.PropertyBegin);
            if (propertyInfo.CanRead)
                writer.Write(Resources.PropertyGetter);
            if (propertyInfo.CanWrite)
                writer.Write(Resources.PropertySetter);
            writer.Write(Resources.PropertyEnd);

            return true;
        }

        static bool Dumped(
            this TextWriter writer,
            MethodInfo methodInfo)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (methodInfo == null)
                return false;

            writer.Write(
                DumpFormat.MethodInfo,
                methodInfo.ReturnType.Name,
                methodInfo.ReturnType.Namespace,
                methodInfo.ReturnType.AssemblyQualifiedName,
                methodInfo.DeclaringType.Name,
                methodInfo.DeclaringType.Namespace,
                methodInfo.DeclaringType.AssemblyQualifiedName,
                methodInfo.Name);

            if (methodInfo.ContainsGenericParameters)
            {
                var genericParameters = methodInfo.GetGenericArguments();

                writer.Write(Resources.GenericParamListBegin);

                for (var i = 0; i<genericParameters.Length; i++)
                {
                    if (i > 0)
                        writer.Write(Resources.ParametersSeparator);
                    writer.Write(genericParameters[i].Name);
                }

                writer.Write(Resources.GenericParamListEnd);
            }

            var parameters = methodInfo.GetParameters();

            writer.Write(Resources.MethodParameterListBegin);

            for (var i = 0; i<parameters.Length; i++)
            {
                if (i > 0)
                    writer.Write(Resources.ParametersSeparator);
                writer.Write(
                    DumpFormat.MethodParameter,
                    parameters[i].ParameterType.Name,
                    parameters[i].ParameterType.Namespace,
                    parameters[i].ParameterType.AssemblyQualifiedName,
                    parameters[i].Name);
            }

            writer.Write(Resources.MethodParameterListEnd);

            return true;
        }

        public static bool DumpedDictionary(
            this TextWriter writer,
            ICollection sequence,
            DumpAttribute dumpAttribute,
            Action<object> dumpObject,
            Action indent,
            Action unindent)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            var sequenceType = sequence.GetType();

            if (!sequenceType.IsGenericType)
                return false;

            var dictionaryType = sequenceType
                                    .GetInterfaces()
                                    .FirstOrDefault(t => t.IsGenericType  &&
                                                         t.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (dictionaryType == null)
                return false;

            var typeArguments = dictionaryType.GetGenericArguments();

            Contract.Assume(typeArguments.Length == 2);

            var keyType   = typeArguments[0];
            var valueType = typeArguments[1];

            if (!keyType.IsBasicType())
                return false;

            var keyValueType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);

            writer.Write(
                DumpFormat.SequenceTypeName,
                sequenceType.GetTypeName(),
                sequence.Count.ToString(CultureInfo.InvariantCulture));

            writer.Write(
                    DumpFormat.SequenceType,
                    sequenceType.GetTypeName(),
                    sequenceType.Namespace,
                    sequenceType.AssemblyQualifiedName);

            // stop the recursion if dump.Recurse is false
            if (dumpAttribute.RecurseDump==ShouldDump.Skip)
                return true;

            // how many items to dump max?
            var max = dumpAttribute.GetMaxToDump(sequence.Count);
            var n = 0;

            writer.WriteLine();
            writer.Write("{");
            indent();

            foreach (var kv in sequence)
            {
                Contract.Assume(kv.GetType() == keyValueType);

                writer.WriteLine();
                if (n++ >= max)
                {
                    writer.Write(DumpFormat.SequenceDumpTruncated, max);
                    break;
                }

                var key   = keyValueType.GetProperty("Key").GetValue(kv, null);
                var value = keyValueType.GetProperty("Value").GetValue(kv, null);

                writer.Write("[");
                dumpObject(key);
                writer.Write("] = ");

                indent();
                dumpObject(value);
                writer.Write(";");
                unindent();
            }

            unindent();
            writer.WriteLine();
            writer.Write("}");

            return true;
        }

        public static bool DumpedCollection(
            this TextWriter writer,
            IEnumerable sequence,
            DumpAttribute dumpAttribute,
            bool enumerateCustom,
            Action<object> dumpObject,
            Action indent,
            Action unindent)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));
            Contract.Requires(sequence != null);

            var sequenceType = sequence.GetType();

            if (!sequenceType.IsArray && !sequenceType.IsFromSystem())
            {
                if (!enumerateCustom  ||  dumpAttribute.Enumerate != ShouldDump.Dump)
                    return false;

                writer.WriteLine();
            }

            var collection = sequence as ICollection;
            var elementsType = sequenceType.IsArray
                                    ? new Type[] { sequenceType.GetElementType() }
                                    : sequenceType.IsGenericType
                                        ? sequenceType.GetGenericArguments()
                                        : new Type[] { typeof(object) };

            writer.Write(
                DumpFormat.SequenceTypeName,
                sequenceType.IsArray
                        ? elementsType[0].GetTypeName()
                        : sequenceType.GetTypeName(),
                collection != null
                        ? collection.Count.ToString(CultureInfo.InvariantCulture)
                        : string.Empty);

            // how many items to dump max?
            var max = dumpAttribute.GetMaxToDump();
            var bytes = sequence as byte[];

            if (bytes != null)
            {
                // dump no more than max elements from the sequence:
                writer.Write(BitConverter.ToString(bytes, 0, max));
                if (max < bytes.Length)
                    writer.Write(DumpFormat.SequenceDumpTruncated, max);

                return true;
            }

            writer.Write(
                DumpFormat.SequenceType,
                sequenceType.GetTypeName(),
                sequenceType.Namespace,
                sequenceType.AssemblyQualifiedName);

            // stop the recursion if dump.Recurse is false
            if (dumpAttribute.RecurseDump!=ShouldDump.Skip)
            {
                var n = 0;

                indent();

                foreach (var item in sequence)
                {
                    writer.WriteLine();
                    if (n++ >= max)
                    {
                        writer.Write(DumpFormat.SequenceDumpTruncated, max);
                        break;
                    }
                    dumpObject(item);
                }

                unindent();
            }

            return true;
        }
    }
}
