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
            { typeof(DateTime),          (v, w, max) => w.Write("{0:o}", v) },
            { typeof(DateTimeOffset),    (v, w, max) => w.Write("{0:o}", v) },
            { typeof(TimeSpan),          (v, w, max) => w.Write(v.ToString()) },
            { typeof(Uri),               (v, w, max) => w.Write(v.ToString()) },
            { typeof(Guid),              (v, w, max) => w.Write(v.ToString()) },
            { typeof(IntPtr),            (v, w, max) => w.Write("0x{0:x16}", v) },
            { typeof(UIntPtr),           (v, w, max) => w.Write("0x{0:x16}", v) },
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
            Contract.Requires(w != null);
            Contract.Requires(max >= 0);

            var stringValue = (string)v;

            if (max < stringValue.Length)
                stringValue = stringValue.Substring(0, max) + "...";

            w.Write(stringValue);
        }

        static void DumpEnumValue(
            object v,
            TextWriter w,
            int max)
        {
            Contract.Requires(v != null);
            Contract.Requires(w != null);

            var type = v.GetType();

            w.Write(
                DumpFormat.Enum,
                type.Name,
                type.Namespace,
                type.AssemblyQualifiedName,
                v.ToString());
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
                    dump = (v, w, max) => _writer.Write("[{0} value]", type.Name);

            dump(value, _writer, dumpMaxLength);
            return true;
        }
        #endregion
    }
}
