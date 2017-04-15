using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    static class WriterExtensions
    {
        static IDictionary<Type,Func<object,ulong>> _castsToUlong = new Dictionary<Type,Func<object,ulong>>()
        {
            [typeof(byte)]    = v => (ulong)(byte)v,
            [typeof(sbyte)]   = v => (ulong)(sbyte)v & 0xFF,
            [typeof(char)]    = v => (ulong)(char)v,
            [typeof(short)]   = v => (ulong)(short)v & 0xFFFF,
            [typeof(ushort)]  = v => (ulong)(ushort)v,
            [typeof(int)]     = v => (ulong)(int)v & 0xFFFFFFFF,
            [typeof(uint)]    = v => (ulong)(uint)v,
            [typeof(long)]    = v => (ulong)(long)v,
            [typeof(ulong)]   = v => (ulong)v,
        };

        static readonly IDictionary<Type, Action<TextWriter, object, int>> _dumpBasicValues = new Dictionary<Type, Action<TextWriter, object, int>>
        {
            { typeof(DBNull),           (w, v, max) => w.Write("DBNull") },
            { typeof(bool),             (w, v, max) => w.Write((bool)v) },
            { typeof(byte),             (w, v, max) => w.Write((byte)v) },
            { typeof(sbyte),            (w, v, max) => w.Write((sbyte)v) },
            { typeof(char),             (w, v, max) => w.Write((char)v) },
            { typeof(short),            (w, v, max) => w.Write((short)v) },
            { typeof(int),              (w, v, max) => w.Write((int)v) },
            { typeof(long),             (w, v, max) => w.Write((long)v) },
            { typeof(ushort),           (w, v, max) => w.Write((ushort)v) },
            { typeof(uint),             (w, v, max) => w.Write((uint)v) },
            { typeof(ulong),            (w, v, max) => w.Write((ulong)v) },
            { typeof(float),            (w, v, max) => w.Write((float)v) },
            { typeof(double),           (w, v, max) => w.Write((double)v) },
            { typeof(decimal),          (w, v, max) => w.Write((decimal)v) },
            { typeof(DateTime),         (w, v, max) => w.Write($"{v:o}") },
            { typeof(DateTimeOffset),   (w, v, max) => w.Write($"{v:o}") },
            { typeof(TimeSpan),         (w, v, max) => w.Write(v.ToString()) },
            { typeof(Uri),              (w, v, max) => w.Write(v.ToString()) },
            { typeof(Guid),             (w, v, max) => w.Write(v.ToString()) },
            { typeof(IntPtr),           (w, v, max) => w.Write($"0x{v:x16}") },
            { typeof(UIntPtr),          (w, v, max) => w.Write($"0x{v:x16}") },
            { typeof(string),           (w, v, max) => w.Dumped((string)v, max) },
        };

        public static IDictionary<Type, Action<TextWriter, object, int>> DumpBasicValues => _dumpBasicValues;

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static bool Dumped(
            this TextWriter writer,
            string @string,
            int max)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires(max >= 0);

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

            var names  = type.GetEnumNames();
            var values = type.GetEnumValues();
            var castToUlong = _castsToUlong[type.GetEnumUnderlyingType()];
            var ulongValue = castToUlong(v);
            var nameLookup = new Dictionary<ulong, string>(values.Length);

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

        public static bool Dumped(
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

        public static bool Dumped(
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

        public static bool Dumped(
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

        public static bool Dumped(
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

        public static bool Dumped(
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
    }
}
