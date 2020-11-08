using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    static class WriterExtensions
    {
        static IReadOnlyDictionary<Type, Func<object, ulong>> CastsToUlong { get; } = new ReadOnlyDictionary<Type, Func<object, ulong>>(
            new Dictionary<Type, Func<object, ulong>>()
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

        public static IReadOnlyDictionary<Type, Action<TextWriter, object, int>> DumpBasicValues { get; } = new ReadOnlyDictionary<Type, Action<TextWriter, object, int>>(
            new Dictionary<Type, Action<TextWriter, object, int>>
            {
                [typeof(DBNull)]         = (w, _, _) => w.Write("DBNull"),
                [typeof(bool)]           = (w, v, _) => w.Write((bool)v),
                [typeof(byte)]           = (w, v, _) => w.Write((byte)v),
                [typeof(sbyte)]          = (w, v, _) => w.Write((sbyte)v),
                [typeof(char)]           = (w, v, _) => w.Write((char)v),
                [typeof(short)]          = (w, v, _) => w.Write((short)v),
                [typeof(int)]            = (w, v, _) => w.Write((int)v),
                [typeof(long)]           = (w, v, _) => w.Write((long)v),
                [typeof(ushort)]         = (w, v, _) => w.Write((ushort)v),
                [typeof(uint)]           = (w, v, _) => w.Write((uint)v),
                [typeof(ulong)]          = (w, v, _) => w.Write((ulong)v),
                [typeof(float)]          = (w, v, _) => w.Write((float)v),
                [typeof(double)]         = (w, v, _) => w.Write((double)v),
                [typeof(decimal)]        = (w, v, _) => w.Write((decimal)v),
                [typeof(DateTime)]       = (w, v, _) => w.Write($"{v:o}"),
                [typeof(DateTimeOffset)] = (w, v, _) => w.Write($"{v:o}"),
                [typeof(TimeSpan)]       = (w, v, _) => w.Write(v.ToString()),
                [typeof(Uri)]            = (w, v, _) => w.Write(v.ToString()),
                [typeof(Guid)]           = (w, v, _) => w.Write(v.ToString()),
                [typeof(IntPtr)]         = (w, v, _) => w.Write($"0x{v:x16}"),
                [typeof(UIntPtr)]        = (w, v, _) => w.Write($"0x{v:x16}"),
                [typeof(string)]         = (w, v, max) => w.Dumped((string)v, max),
            });

        static IReadOnlyDictionary<Type, Func<TextWriter, MemberInfo, bool>> MemberInfoDumpers { get; } = new ReadOnlyDictionary<Type, Func<TextWriter, MemberInfo, bool>>(
            new Dictionary<Type, Func<TextWriter, MemberInfo, bool>>
            {
                [typeof(Type)]              = (w, mi) => w.Dumped((Type)mi),
                [typeof(TypeInfo)]          = (w, mi) => w.Dumped((Type)mi),

                [typeof(EventInfo)]         = (w, mi) => w.Dumped((EventInfo)mi),
                [typeof(FieldInfo)]         = (w, mi) => w.Dumped((FieldInfo)mi),
                [typeof(PropertyInfo)]      = (w, mi) => w.Dumped((PropertyInfo)mi),
                [typeof(MethodInfo)]        = (w, mi) => w.Dumped((MethodInfo)mi),
            });

        /// <summary>
        /// Matches the name space of the types within System
        /// </summary>
        static Regex SystemNameSpace { get; } = new Regex(Resources.RegexSystemNamespace, RegexOptions.Compiled);

        public static bool IsFromSystem(this Type type) =>
            SystemNameSpace.IsMatch(type.Namespace ?? "");

        static bool Dumped(
            this TextWriter writer,
            string @string,
            int max)
        {
            writer.Write("{0}", max >= 0  &&  max < @string.Length
                                        ? @string.Substring(0, max) + "..."
                                        : @string);
            return true;
        }

        static void DumpEnumValue(
            this TextWriter writer,
            object v,
            int _)
        {
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

            var names = type.GetEnumNames();
            var values = type.GetEnumValues();
            var castToUlong = CastsToUlong[type.GetEnumUnderlyingType()];
            var ulongValue = castToUlong(v);
            var nameLookup = new Dictionary<ulong, string>(values.Length);

            for (var i = 0; i<values.Length; i++)
                nameLookup[castToUlong(values.GetValue(i) ?? DumpUtilities.Unknown)] = names[i];

            writer.Write(
                DumpFormat.EnumFlagPrefix,
                type.Name,
                type.Namespace,
                type.AssemblyQualifiedName);

            var first = true;

            for (ulong flag = 1; flag <= castToUlong(values.GetValue(values.Length-1) ?? DumpUtilities.Unknown); flag <<= 1)
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
        /// Dumps the value of basic types (all primitive types plus <see cref="string" />, <see cref="DateTime" />, <see cref="DateTimeOffset" />,
        /// <see cref="TimeSpan" />, <see cref="decimal" /> enum-s, <see cref="Guid" />, Uri).
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="dumpAttribute">The dump attribute associated with the value (e.g. property attribute).</param>
        /// <returns><c>true</c> if the value was dumped; otherwise <c>false</c> (e.g. the value is struct)</returns>
        public static bool DumpedBasicValue(
            this TextWriter writer,
            object? value,
            DumpAttribute? dumpAttribute)
        {
            if (value == null)
            {
                writer.Write(DumpUtilities.Null);
                return true;
            }

            if (!value.GetType().IsBasicType())
                return false;

            // initialize the max dump length here
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

                // override the max dump length
                if (dumpAttribute.MaxLength > 0)
                    dumpMaxLength = dumpAttribute.MaxLength;
            }

            var type = value.GetType();

            if (!DumpBasicValues.TryGetValue(type, out var dump))
                dump = type.IsEnum ? DumpEnumValue : (w, _, _) => w.Write($"[{type.Name} value]");

            dump(writer, value, dumpMaxLength);
            return true;
        }

        public static bool DumpedBasicNullable(
            this TextWriter writer,
            object? value,
            DumpAttribute? dumpAttribute)
        {
            if (value == null)
            {
                writer.Write(DumpUtilities.Null);
                return true;
            }

            var type = value.GetType();

            if (type.IsBasicType())
                return writer.DumpedBasicValue(value, dumpAttribute);

            // make sure it is Nullable<T>
            if (!type.IsGenericType  ||
                type.GetGenericTypeDefinition() != typeof(Nullable<>)  ||
                !type.GetGenericArguments()[0].IsBasicType())
                return false;

            // must have property HasValue!
            var hasValue = (bool)type.InvokeMember(nameof(Nullable<int>.HasValue), BindingFlags.Instance|BindingFlags.Public|BindingFlags.GetProperty, null, value, Array.Empty<object>())!;

            if (!hasValue)
            {
                writer.Write(DumpUtilities.Null);
                return true;
            }

            // must have property Value which is not null
            var val = type.InvokeMember(nameof(Nullable<int>.Value), BindingFlags.Instance|BindingFlags.Public|BindingFlags.GetProperty, null, value, Array.Empty<object>())!;

            return writer.DumpedBasicValue(val, dumpAttribute);
        }

        public static bool Dumped(
            this TextWriter writer,
            Delegate? @delegate)
        {
            if (@delegate == null)
                return false;

            writer.Write(
                DumpFormat.Delegate,
                @delegate.Method.DeclaringType!=null ? @delegate.Method.DeclaringType.Name : "",
                @delegate.Method.DeclaringType!=null ? @delegate.Method.DeclaringType.Namespace : "",
                @delegate.Method.DeclaringType!=null ? @delegate.Method.DeclaringType.AssemblyQualifiedName : "",
                @delegate.Method.Name,
                @delegate.Target==null
                    ? Resources.ClassMethodDesignator
                    : Resources.InstanceMethodDesignator);

            return true;
        }

        public static bool Dumped(
            this TextWriter writer,
            MemberInfo? memberInfo)
        {
            if (memberInfo == null)
                return false;

            var miType = memberInfo.GetType();

            do
            {
                if (MemberInfoDumpers.TryGetValue(miType!, out var dump))
                {
                    writer.Write(
                        DumpFormat.MemberInfoMemberType,
                        memberInfo.MemberType.ToString());
                    return dump(writer, memberInfo);
                }
                miType = miType!.BaseType;
            }
            while (miType != typeof(MemberInfo));

            // last resort:
            writer.Write(
                DumpFormat.MemberInfoMemberType,
                memberInfo.MemberType.ToString());
            writer.Write(memberInfo.Name);
            return true;
        }

        static bool Dumped(
            this TextWriter writer,
            Type type)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

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
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (eventInfo == null)
                return false;

            writer.Write(
                DumpFormat.MethodInfo,
                eventInfo.EventHandlerType?.Name ?? DumpUtilities.Unknown,
                eventInfo.EventHandlerType?.Namespace ?? DumpUtilities.Unknown,
                eventInfo.EventHandlerType?.AssemblyQualifiedName ?? DumpUtilities.Unknown,
                eventInfo.DeclaringType?.Name ?? DumpUtilities.Unknown,
                eventInfo.DeclaringType?.Namespace ?? DumpUtilities.Unknown,
                eventInfo.DeclaringType?.AssemblyQualifiedName ?? DumpUtilities.Unknown,
                eventInfo.Name);

            return true;
        }

        static bool Dumped(
            this TextWriter writer,
            FieldInfo fieldInfo)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (fieldInfo == null)
                return false;

            writer.Write(
                DumpFormat.MethodInfo,
                fieldInfo.FieldType.Name,
                fieldInfo.FieldType.Namespace,
                fieldInfo.FieldType.AssemblyQualifiedName,
                fieldInfo.DeclaringType?.Name ?? DumpUtilities.Unknown,
                fieldInfo.DeclaringType?.Namespace ?? DumpUtilities.Unknown,
                fieldInfo.DeclaringType?.AssemblyQualifiedName ?? DumpUtilities.Unknown,
                fieldInfo.Name);

            return true;
        }

        static bool Dumped(
            this TextWriter writer,
            PropertyInfo propertyInfo)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (propertyInfo == null)
                return false;

            var indexes = propertyInfo.GetIndexParameters();

            writer.Write(
                DumpFormat.MethodInfo,
                propertyInfo.PropertyType.Name,
                propertyInfo.PropertyType.Namespace,
                propertyInfo.PropertyType.AssemblyQualifiedName,
                propertyInfo.DeclaringType?.Name ?? DumpUtilities.Unknown,
                propertyInfo.DeclaringType?.Namespace ?? DumpUtilities.Unknown,
                propertyInfo.DeclaringType?.AssemblyQualifiedName ?? DumpUtilities.Unknown,
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
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (methodInfo == null)
                return false;

            writer.Write(
                DumpFormat.MethodInfo,
                methodInfo.ReturnType.Name,
                methodInfo.ReturnType.Namespace,
                methodInfo.ReturnType.AssemblyQualifiedName,
                methodInfo.DeclaringType?.Name ?? DumpUtilities.Unknown,
                methodInfo.DeclaringType?.Namespace ?? DumpUtilities.Unknown,
                methodInfo.DeclaringType?.AssemblyQualifiedName ?? DumpUtilities.Unknown,
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
            object sequence,
            DumpAttribute dumpAttribute,
            Action<object?> dumpObject,
            Action indent,
            Action unindent)
        {
            var sequenceType = sequence.GetType();
            (var keyType, var valueType) = sequenceType.DictionaryTypeArguments();

            if (keyType == typeof(void))
                return false;

            var piCount = sequenceType.GetProperty(nameof(ICollection.Count), BindingFlags.Instance|BindingFlags.Public);
            var count = (int)(piCount?.GetValue(sequence) ?? int.MaxValue);
            var keyValueType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);

            writer.Write(
                DumpFormat.SequenceTypeName,
                sequenceType.GetTypeName(),
                count.ToString(CultureInfo.InvariantCulture));

            writer.Write(
                    DumpFormat.SequenceType,
                    sequenceType.GetTypeName(),
                    sequenceType.Namespace ?? DumpUtilities.Unknown,
                    sequenceType.AssemblyQualifiedName ?? DumpUtilities.Unknown);

            // stop the recursion if dump.Recurse is false
            if (dumpAttribute.RecurseDump==ShouldDump.Skip)
                return true;

            // how many items to dump max?
            var max = dumpAttribute.GetMaxToDump(count);
            var n = 0;

            writer.WriteLine();
            writer.Write("{");
            indent();

            foreach (var kv in (IEnumerable)sequence)
            {
                Debug.Assert(kv.GetType() == keyValueType);

                writer.WriteLine();
                if (n++ >= max)
                {
                    writer.Write(DumpFormat.SequenceDumpTruncated, max, count);
                    break;
                }

                var key = keyValueType.GetProperty("Key")!.GetValue(kv, null);
                var value = keyValueType.GetProperty("Value")!.GetValue(kv, null);

                writer.Write("[");
                dumpObject(key);
                writer.Write("] = ");

                dumpObject(value);
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
            Action<object?> dumpObject,
            Action indent,
            Action unindent)
        {
            var sequenceType = sequence.GetType();
            var piCount = sequenceType.IsArray
                                ? sequenceType.GetProperty(nameof(Array.Length), BindingFlags.Instance|BindingFlags.Public)
                                : sequenceType.GetProperty(nameof(ICollection.Count), BindingFlags.Instance|BindingFlags.Public);
            var count = (int)(piCount?.GetValue(sequence) ?? int.MaxValue);
            var max = dumpAttribute.GetMaxToDump(count);    // how many items to dump max?
            var elementsType = sequenceType.IsArray
                                    ? new Type[] { sequenceType.GetElementType()! }
                                    : sequenceType.IsGenericType
                                        ? sequenceType.GetGenericArguments()
                                        : new Type[] { typeof(object) };

            writer.Write(
                DumpFormat.SequenceTypeName,
                sequenceType.IsArray
                        ? elementsType[0].GetTypeName()
                        : sequenceType.GetTypeName(),
                count is >0 and <int.MaxValue
                        ? count.ToString(CultureInfo.InvariantCulture)
                        : "");

            if (sequence is byte[] bytes)
            {
                // dump no more than max elements from the sequence:
                writer.Write(BitConverter.ToString(bytes, 0, max));
                if (max < bytes.Length)
                    writer.Write(DumpFormat.SequenceDumpTruncated, max, count);

                return true;
            }

            writer.Write(
                DumpFormat.SequenceType,
                sequenceType.GetTypeName(),
                sequenceType.Namespace ?? DumpUtilities.Unknown,
                sequenceType.AssemblyQualifiedName ?? DumpUtilities.Unknown);

            // stop the recursion if dump.Recurse is false
            if (dumpAttribute.RecurseDump is not ShouldDump.Skip)
            {
                var n = 0;

                indent();

                foreach (var item in sequence)
                {
                    writer.WriteLine();
                    if (n++ >= max)
                    {
                        writer.Write(DumpFormat.SequenceDumpTruncated, max, count);
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
