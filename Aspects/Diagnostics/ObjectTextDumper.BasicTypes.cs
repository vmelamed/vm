using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    sealed partial class ObjectTextDumper
    {
        #region Basic types handling
        #region Default basic types' dump methods
        static internal readonly IDictionary<Type, Action<object, TextWriter, int>> DumpBasicValues = new Dictionary<Type, Action<object, TextWriter, int>>
        {
            { typeof(DBNull),            (v, w, max) => w.Write("DBNull") },
            { typeof(Boolean),           (v, w, max) => w.Write((bool)v) },
            { typeof(Byte),              (v, w, max) => w.Write((byte)v) },
            { typeof(SByte),             (v, w, max) => w.Write((sbyte)v) },
            { typeof(Char),              (v, w, max) => w.Write((char)v) },
            { typeof(Int16),             (v, w, max) => w.Write((Int16)v) },
            { typeof(Int32),             (v, w, max) => w.Write((Int32)v) },
            { typeof(Int64),             (v, w, max) => w.Write((Int64)v) },
            { typeof(UInt16),            (v, w, max) => w.Write((UInt16)v) },
            { typeof(UInt32),            (v, w, max) => w.Write((UInt32)v) },
            { typeof(UInt64),            (v, w, max) => w.Write((UInt64)v) },
            { typeof(Single),            (v, w, max) => w.Write((float)v) },
            { typeof(Double),            (v, w, max) => w.Write((double)v) },
            { typeof(Decimal),           (v, w, max) => w.Write((decimal)v) },
            { typeof(DateTime),          (v, w, max) => w.Write($"{v:o}") },
            { typeof(DateTimeOffset),    (v, w, max) => w.Write($"{v:o}") },
            { typeof(TimeSpan),          (v, w, max) => w.Write(v.ToString()) },
            { typeof(Uri),               (v, w, max) => w.Write(v.ToString()) },
            { typeof(Guid),              (v, w, max) => w.Write(v.ToString()) },
            { typeof(IntPtr),            (v, w, max) => w.Write($"0x{v:x16}") },
            { typeof(UIntPtr),           (v, w, max) => w.Write($"0x{v:x16}") },
            { typeof(string),            DumpStringValue },
        };
        #endregion

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        static void DumpStringValue(
            object v,
            TextWriter w,
            int max)
        {
            Contract.Requires(v != null);
            Contract.Requires(v is string);
            Contract.Requires(max >= 0);

            var stringValue = (string)v;

            if (max < stringValue.Length)
                stringValue = stringValue.Substring(0, max) + "...";

            w.Write(stringValue);
        }

        static IDictionary<Type,Func<object,ulong>> _castsToUlong = new Dictionary<Type,Func<object,ulong>>()
        {
            [typeof(byte)]    = v => (byte)v,
            [typeof(sbyte)]   = v => (uint)(sbyte)v & 0xFF,
            [typeof(char)]    = v => (char)v,
            [typeof(short)]   = v => (uint)(short)v & 0xFFFF,
            [typeof(ushort)]  = v => (ushort)v,
            [typeof(int)]     = v => (uint)(int)v & 0xFFFFFFFF,
            [typeof(uint)]    = v => (uint)v,
            [typeof(long)]    = v => (ulong)(long)v,
            [typeof(ulong)]   = v => (ulong)v,
        };

        static void DumpEnumValue(
            object v,
            TextWriter w,
            int max)
        {
            Contract.Requires(v != null);

            var type = v.GetType();

            if (type.GetCustomAttribute<FlagsAttribute>() == null)
            {
                w.Write(
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

            w.Write(
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
                        w.Write(DumpFormat.EnumFlagSeparator);
                    w.Write(
                        DumpFormat.EnumFlag,
                        type.Name,
                        type.Namespace,
                        type.AssemblyQualifiedName,
                        nameLookup[flag]);
                }

            w.Write(
                DumpFormat.EnumFlagSuffix,
                type.Name,
                type.Namespace,
                type.AssemblyQualifiedName);
        }

        /// <summary>
        /// Dumps the value of basic types (all primitive types plus <see cref="String"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, 
        /// <see cref="TimeSpan"/>, <see cref="Decimal"/> enum-s, <see cref="Guid"/>, Uri).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyDumpAttribute">The dump attribute associated with the value (e.g. property attribute).</param>
        /// <returns>
        /// <c>true</c> if the value was dumped; otherwise <c>false</c> (e.g. the value is struct)
        /// </returns>
        bool DumpedBasicValue(
            object value,
            DumpAttribute propertyDumpAttribute)
        {
            if (value == null)
            {
                _writer.Write(DumpUtilities.Null);
                return true;
            }

            if (!value.GetType().IsBasicType())
                return false;

            // store the max dump length here
            int dumpMaxLength = int.MaxValue;

            if (propertyDumpAttribute != null)
            {
                // should we mask it - dump the mask and return
                if (propertyDumpAttribute.Mask)
                {
                    _writer.Write(propertyDumpAttribute.MaskValue);
                    return true;
                }

                // special formatting - dump blindly and return
                if (propertyDumpAttribute.ValueFormat != DumpFormat.Value)
                {
                    if (propertyDumpAttribute.ValueFormat == Resources.ValueFormatToString)
                        _writer.Write(value.ToString());
                    else
                        _writer.Write(propertyDumpAttribute.ValueFormat, value);
                    return true;
                }

                // get the max dump length
                if (propertyDumpAttribute.MaxLength > 0)
                    dumpMaxLength = propertyDumpAttribute.MaxLength;
            }

            Action<object, TextWriter, int> dump;
            var type = value.GetType();

            if (!DumpBasicValues.TryGetValue(type, out dump))
                if (type.IsEnum)
                    dump = DumpEnumValue;
                else
                    dump = (v, w, max) => _writer.Write($"[{type.Name} value]");

            dump(value, _writer, dumpMaxLength);
            return true;
        }
        #endregion
    }
}
