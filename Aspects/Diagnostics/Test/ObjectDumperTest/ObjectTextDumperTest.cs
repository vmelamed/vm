using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using vm.Aspects.Diagnostics.DumpImplementation;
using vm.Aspects.Diagnostics.ObjectDumper.Tests.PartialTrust;

#pragma warning disable 67, 649

namespace vm.Aspects.Diagnostics.ObjectDumper.Tests
{
    [TestClass]
    public class ObjectTextDumperTest
    {
        public TestContext TestContext { get; set; }

        PrivateObject GetDumperInstanceAccessor(int indentLevel = 0, int indentLength = 2) => new PrivateObject(
                                                                                                    typeof(ObjectTextDumper),
                                                                                                    new StringWriter(CultureInfo.InvariantCulture),
                                                                                                    indentLevel,
                                                                                                    indentLength,
                                                                                                    DumpTextWriter.DefaultMaxLength,
                                                                                                    BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly, BindingFlags.Default);

        PrivateObject GetDumperInstanceAccessor(StringWriter w, int indentLevel = 0, int indentLength = 2) => new PrivateObject(
                                                                                                                        typeof(ObjectTextDumper),
                                                                                                                        w,
                                                                                                                        indentLevel,
                                                                                                                        indentLength,
                                                                                                                        DumpTextWriter.DefaultMaxLength,
                                                                                                                        BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly, BindingFlags.Default);

        PrivateType GetDumperClassAccessor() => new PrivateType(typeof(ObjectTextDumper));

        enum TestEnum
        {
            None,
            One,
            Two,
            All,
        }

        [Flags]
        enum TestFlags
        {
            One = 1 << 0,
            Two = 1 << 1,
            Four = 1 << 2,
            Eight = 1 << 3,
        }

        #region basic values and corresponding strings
        object[] basicValues =
        {
            null,
            new int?(),
            true,
            'A',
            (byte)1,
            (sbyte)1,
            (short)1,
            (int)1,
            (long)1,
            (ushort)1,
            (uint)1,
            (ulong)1,
            1.0,
            (float)1.0,
            1M,
            "1M",
            Guid.Empty,
            new Uri("http://localhost"),
            new DateTime(2013, 1, 13),
            new TimeSpan(123L),
            new DateTimeOffset(new DateTime(2013, 1, 13)),
        };

        string[] basicValuesStrings =
        {
            "<null>",
            "<null>",
            "True",
            "A",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1M",
            "00000000-0000-0000-0000-000000000000",
            "http://localhost/",
            "2013-01-13T00:00:00.0000000",
            "00:00:00.0000123",
            "2013-01-13T00:00:00.0000000-05:00",
        };
        #endregion

        [TestMethod]
        public void TestIsBasicType()
        {
            var target = GetDumperClassAccessor();

            for (var i = 2; i<basicValues.Length; i++)
                Assert.IsTrue(basicValues[i].GetType().IsBasicType());
        }

        [TestMethod]
        public void TestDumpedBasicValue()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                Assert.IsFalse(w.DumpedBasicValue(this, null));
                foreach (var v in basicValues)
                    Assert.IsTrue(w.DumpedBasicValue(v, null));
            }
        }

        void TestDumpedBasicValueText(
            string expected,
            object value,
            DumpAttribute dumpAttribute = null,
            int indentValue = 0)
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = GetDumperInstanceAccessor(w, indentValue);

                Assert.IsTrue(w.DumpedBasicValue(value, dumpAttribute));

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        void TestDumpObjectBasicValueText(
            string expected,
            object value,
            Type metadata = null,
            DumpAttribute dumpAttribute = null,
            int indentValue = 0)
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = GetDumperInstanceAccessor(w, indentValue);

                Assert.AreEqual(target.Target, target.Invoke("Dump", value, metadata, dumpAttribute));

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpedBasicValueText()
        {
            for (var i = 0; i<basicValues.Length; i++)
                TestDumpedBasicValueText(basicValuesStrings[i], basicValues[i]);
        }

        [TestMethod]
        public void TestDumpedBasicValueTextIndent()
        {
            for (var i = 0; i<basicValues.Length; i++)
                TestDumpedBasicValueText(basicValuesStrings[i], basicValues[i], null, 2);
        }

        [TestMethod]
        public void TestDumpMaskedBasicValueText()
        {
            for (var i = 0; i<basicValues.Length; i++)
                TestDumpedBasicValueText(basicValues[i]==null ? "<null>" : "------", basicValues[i], new DumpAttribute { Mask = true, MaskValue = "------" });
        }

        [TestMethod]
        public void TestDumpMaskedBasicValueText1()
        {
            for (var i = 0; i<basicValues.Length; i++)
                TestDumpedBasicValueText(basicValues[i]==null ? "<null>" : "******", basicValues[i], new DumpAttribute { Mask = true });
        }

        [TestMethod]
        public void TestDumpedBasicValueFormat()
        {
            TestDumpedBasicValueText("{00000000-0000-0000-0000-000000000000}", Guid.Empty, new DumpAttribute { ValueFormat = "{0:B}" });
            TestDumpedBasicValueText("¤1.00", 1.0, new DumpAttribute { ValueFormat = "{0:C}" });
            TestDumpedBasicValueText("¤1.00", 1M, new DumpAttribute { ValueFormat = "{0:C}" });
        }

        [TestMethod]
        public void TestStringValueLength()
        {
            TestDumpedBasicValueText("012345678901...", "01234567890123456789", new DumpAttribute { MaxLength = 12 });
        }

        [TestMethod]
        public void TestDumpObjectBasicValueText()
        {
            for (var i = 0; i<basicValues.Length; i++)
                TestDumpObjectBasicValueText(basicValuesStrings[i], basicValues[i]);
        }

        [TestMethod]
        public void TestDumpObjectBasicValueTextIndent()
        {
            for (var i = 0; i<basicValues.Length; i++)
                TestDumpObjectBasicValueText(basicValuesStrings[i], basicValues[i], null, null, 2);
        }

        [TestMethod]
        public void TestDumpObjectMaskedBasicValueText()
        {
            for (var i = 0; i<basicValues.Length; i++)
                TestDumpObjectBasicValueText(basicValues[i]==null ? "<null>" : "------", basicValues[i], null, new DumpAttribute { Mask = true, MaskValue = "------" });
        }

        [TestMethod]
        public void TestDumpObjectMaskedBasicValueText1()
        {
            for (var i = 0; i<basicValues.Length; i++)
                TestDumpObjectBasicValueText(basicValues[i]==null ? "<null>" : "******", basicValues[i], null, new DumpAttribute { Mask = true });
        }

        [TestMethod]
        public void TestDumpObjectBasicValueFormat()
        {
            TestDumpObjectBasicValueText("{00000000-0000-0000-0000-000000000000}", Guid.Empty, null, new DumpAttribute { ValueFormat = "{0:B}" });
            TestDumpObjectBasicValueText("¤1.00", 1.0, null, new DumpAttribute { ValueFormat = "{0:C}" });
            TestDumpObjectBasicValueText("¤1.00", 1M, null, new DumpAttribute { ValueFormat = "{0:C}" });
        }

        [TestMethod]
        public void TestObjectStringValueLength()
        {
            TestDumpObjectBasicValueText("012345678901...", "01234567890123456789", null, new DumpAttribute { MaxLength = 12 });
        }

        public class Object1
        {
            public object ObjectProperty { get; set; }
            public int? NullIntProperty { get; set; }
            public long? NullLongProperty { get; set; }
            public bool BoolProperty { get; set; }
            public char CharProperty { get; set; }
            public byte ByteProperty { get; set; }
            public sbyte SByteProperty { get; set; }
            public short ShortProperty { get; set; }
            public int IntProperty { get; set; }
            public long LongProperty { get; set; }
            public ushort UShortProperty { get; set; }
            public uint UIntProperty { get; set; }
            public ulong ULongProperty { get; set; }
            public double DoubleProperty { get; set; }
            public float FloatProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public Uri UriProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
            public TimeSpan TimeSpanProperty { get; set; }
            public DateTimeOffset DateTimeOffsetProperty { get; set; }


            public object ObjectField;
            public int? NullIntField;
            public long? NullLongField;
            public bool BoolField;
            public char CharField;
            public byte ByteField;
            public sbyte SByteField;
            public short ShortField;
            public int IntField;
            public long LongField;
            public ushort UShortField;
            public uint UIntField;
            public ulong ULongField;
            public double DoubleField;
            public float FloatField;
            public decimal DecimalField;
            public Guid GuidField;
            public Uri UriField;
            public DateTime DateTimeField;
            public TimeSpan TimeSpanField;
            public DateTimeOffset DateTimeOffsetField;
        }

        class Object1Metadata
        {
            private Object1Metadata() { }

            [Dump(0, LabelFormat = "{0,-24} : ")]
            public object ObjectProperty { get; set; }
            [Dump(1)]
            public object NullIntProperty { get; set; }
            [Dump(2)]
            public object NullLongProperty { get; set; }
            [Dump(3)]
            public object BoolProperty { get; set; }
            [Dump(4)]
            public object CharProperty { get; set; }
            [Dump(5)]
            public object ByteProperty { get; set; }
            [Dump(6)]
            public object SByteProperty { get; set; }
            [Dump(7)]
            public object ShortProperty { get; set; }
            [Dump(8)]
            public object IntProperty { get; set; }
            [Dump(9)]
            public object LongProperty { get; set; }
            [Dump(false)]
            public object UShortProperty { get; set; }
            [Dump(-1)]
            public object UIntProperty { get; set; }
            [Dump(-2)]
            public object ULongProperty { get; set; }
            [Dump(-3, ValueFormat = "{0:F01}")]
            public object DoubleProperty { get; set; }
            [Dump(-4, ValueFormat = "{0:F01}")]
            public object FloatProperty { get; set; }
            [Dump(-5)]
            public object DecimalProperty { get; set; }
            [Dump(-6)]
            public object GuidProperty { get; set; }
            [Dump(-7)]
            public object UriProperty { get; set; }
            [Dump(-8)]
            public object DateTimeProperty { get; set; }
            [Dump(-9)]
            public object TimeSpanProperty { get; set; }
            [Dump(false)]
            public object DateTimeOffsetProperty { get; set; }
        }

        abstract class Object1FieldsMetadata
        {
            [Dump(0, LabelFormat = "{0,-24} : ")]
            public object ObjectProperty;
            [Dump(1)]
            public object NullIntProperty;
            [Dump(2)]
            public object NullLongProperty;
            [Dump(3)]
            public object BoolProperty;
            [Dump(4)]
            public object CharProperty;
            [Dump(5)]
            public object ByteProperty;
            [Dump(6)]
            public object SByteProperty;
            [Dump(7)]
            public object ShortProperty;
            [Dump(8)]
            public object IntProperty;
            [Dump(9)]
            public object LongProperty;
            [Dump(false)]
            public object UShortProperty;
            [Dump(-1)]
            public object UIntProperty;
            [Dump(-2)]
            public object ULongProperty;
            [Dump(-3, ValueFormat = "{0:F01}")]
            public object DoubleProperty;
            [Dump(-4, ValueFormat = "{0:F01}")]
            public object FloatProperty;
            [Dump(-5)]
            public object DecimalProperty;
            [Dump(-6)]
            public object GuidProperty;
            [Dump(-7)]
            public object UriProperty;
            [Dump(-8)]
            public object DateTimeProperty;
            [Dump(-9)]
            public object TimeSpanProperty;
            [Dump(false)]
            public object DateTimeOffsetProperty;


            [Dump(10, LabelFormat = "{0,-24} : ")]
            public object ObjectField;
            [Dump(11)]
            public object NullIntField;
            [Dump(12)]
            public object NullLongField;
            [Dump(13)]
            public object BoolField;
            [Dump(14)]
            public object CharField;
            [Dump(15)]
            public object ByteField;
            [Dump(16)]
            public object SByteField;
            [Dump(17)]
            public object ShortField;
            [Dump(18)]
            public object IntField;
            [Dump(19)]
            public object LongField;
            [Dump(false)]
            public object UShortField;
            [Dump(-11)]
            public object UIntField;
            [Dump(-12)]
            public object ULongField;
            [Dump(-13, ValueFormat = "{0:F01}")]
            public object DoubleField;
            [Dump(-14, ValueFormat = "{0:F01}")]
            public object FloatField;
            [Dump(-15)]
            public object DecimalField;
            [Dump(-16)]
            public object GuidField;
            [Dump(-17)]
            public object UriField;
            [Dump(-18)]
            public object DateTimeField;
            [Dump(-19)]
            public object TimeSpanField;
            [Dump(false)]
            public object DateTimeOffsetField;
        }

        [Dump(DumpNullValues = ShouldDump.Skip)]
        public class Object2
        {
            [Dump(0, LabelFormat = "{0,-24} : ")]
            public object ObjectProperty { get; set; }
            [Dump(1)]
            public int? NullIntProperty { get; set; }
            [Dump(2)]
            public long? NullLongProperty { get; set; }
            [Dump(3)]
            public bool BoolProperty { get; set; }
            [Dump(4)]
            public char CharProperty { get; set; }
            [Dump(5)]
            public byte ByteProperty { get; set; }
            [Dump(6)]
            public sbyte SByteProperty { get; set; }
            [Dump(7)]
            public short ShortProperty { get; set; }
            [Dump(8)]
            public int IntProperty { get; set; }
            [Dump(9)]
            public long LongProperty { get; set; }
            [Dump(false)]
            public ushort UShortProperty { get; set; }
            [Dump(-1)]
            public uint UIntProperty { get; set; }
            [Dump(-2)]
            public ulong ULongProperty { get; set; }
            [Dump(-3, ValueFormat = "{0:F01}")]
            public double DoubleProperty { get; set; }
            [Dump(-4, ValueFormat = "{0:F01}")]
            public float FloatProperty { get; set; }
            [Dump(-5)]
            public decimal DecimalProperty { get; set; }
            [Dump(-6)]
            public Guid GuidProperty { get; set; }
            [Dump(-7)]
            public Uri UriProperty { get; set; }
            [Dump(-8)]
            public DateTime DateTimeProperty { get; set; }
            [Dump(-9, ValueFormat = "ToString()")]
            public DateTime DateTimeProperty1 { get; set; }
            [Dump(-10)]
            public TimeSpan TimeSpanProperty { get; set; }
            [Dump(false)]
            public DateTimeOffset DateTimeOffsetProperty { get; set; }
        }

        [MetadataType(typeof(Object3Metadata))]
        public class Object3
        {
            public object ObjectProperty { get; set; }
            public int? NullIntProperty { get; set; }
            public long? NullLongProperty { get; set; }
            public bool BoolProperty { get; set; }
            public char CharProperty { get; set; }
            public byte ByteProperty { get; set; }
            public sbyte SByteProperty { get; set; }
            public short ShortProperty { get; set; }
            public int IntProperty { get; set; }
            public long LongProperty { get; set; }
            public ushort UShortProperty { get; set; }
            public uint UIntProperty { get; set; }
            public ulong ULongProperty { get; set; }
            public double DoubleProperty { get; set; }
            public float FloatProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public Uri UriProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
            public TimeSpan TimeSpanProperty { get; set; }
            public DateTimeOffset DateTimeOffsetProperty { get; set; }
        }

        [Dump(DumpNullValues = ShouldDump.Skip)]
        class Object3Metadata
        {
            private Object3Metadata() { }

            [Dump(0, LabelFormat = "{0,-24} : ")]
            public object ObjectProperty { get; set; }
            [Dump(1)]
            public object NullIntProperty { get; set; }
            [Dump(2)]
            public object NullLongProperty { get; set; }
            [Dump(3)]
            public object BoolProperty { get; set; }
            [Dump(4)]
            public object CharProperty { get; set; }
            [Dump(5)]
            public object ByteProperty { get; set; }
            [Dump(6)]
            public object SByteProperty { get; set; }
            [Dump(7)]
            public object ShortProperty { get; set; }
            [Dump(8)]
            public object IntProperty { get; set; }
            [Dump(9)]
            public object LongProperty { get; set; }
            [Dump(false)]
            public object UShortProperty { get; set; }
            [Dump(-1)]
            public object UIntProperty { get; set; }
            [Dump(-2)]
            public object ULongProperty { get; set; }
            [Dump(-3, ValueFormat = "{0:F01}")]
            public object DoubleProperty { get; set; }
            [Dump(-4, ValueFormat = "{0:F01}")]
            public object FloatProperty { get; set; }
            [Dump(-5)]
            public object DecimalProperty { get; set; }
            [Dump(-6)]
            public object GuidProperty { get; set; }
            [Dump(-7)]
            public object UriProperty { get; set; }
            [Dump(-8)]
            public object DateTimeProperty { get; set; }
            [Dump(-9)]
            public object TimeSpanProperty { get; set; }
            [Dump(false)]
            public object DateTimeOffsetProperty { get; set; }
        }

        internal static Object1 GetObject1() => new Object1
        {
            ObjectProperty         = null,
            NullIntProperty        = null,
            NullLongProperty       = 1L,
            BoolProperty           = true,
            CharProperty           = 'A',
            ByteProperty           = (byte)1,
            SByteProperty          = (sbyte)1,
            ShortProperty          = (short)1,
            IntProperty            = (int)1,
            LongProperty           = (long)1,
            UShortProperty         = (ushort)1,
            UIntProperty           = (uint)1,
            ULongProperty          = (ulong)1,
            DoubleProperty         = 1.0,
            FloatProperty          = (float)1.0,
            DecimalProperty        = 1M,
            GuidProperty           = Guid.Empty,
            UriProperty            = new Uri("http://localhost"),
            DateTimeProperty       = new DateTime(2013, 1, 13),
            TimeSpanProperty       = new TimeSpan(123L),
            DateTimeOffsetProperty = new DateTimeOffset(new DateTime(2013, 1, 13)),

            ObjectField            = null,
            NullIntField           = null,
            NullLongField          = 1L,
            BoolField              = true,
            CharField              = 'A',
            ByteField              = (byte)1,
            SByteField             = (sbyte)1,
            ShortField             = (short)1,
            IntField               = (int)1,
            LongField              = (long)1,
            UShortField            = (ushort)1,
            UIntField              = (uint)1,
            ULongField             = (ulong)1,
            DoubleField            = 1.0,
            FloatField             = (float)1.0,
            DecimalField           = 1M,
            GuidField              = Guid.Empty,
            UriField               = new Uri("http://localhost"),
            DateTimeField          = new DateTime(2013, 1, 13),
            TimeSpanField          = new TimeSpan(123L),
            DateTimeOffsetField    = new DateTimeOffset(new DateTime(2013, 1, 13)),
        };

        internal static Object2 GetObject2() => new Object2
        {
            ObjectProperty         = null,
            NullIntProperty        = null,
            NullLongProperty       = 1L,
            BoolProperty           = true,
            CharProperty           = 'A',
            ByteProperty           = (byte)1,
            SByteProperty          = (sbyte)1,
            ShortProperty          = (short)1,
            IntProperty            = (int)1,
            LongProperty           = (long)1,
            UShortProperty         = (ushort)1,
            UIntProperty           = (uint)1,
            ULongProperty          = (ulong)1,
            DoubleProperty         = 1.0,
            FloatProperty          = (float)1.0,
            DecimalProperty        = 1M,
            GuidProperty           = Guid.Empty,
            UriProperty            = new Uri("http://localhost"),
            DateTimeProperty       = new DateTime(2013, 1, 13),
            DateTimeProperty1      = new DateTime(2013, 1, 25, 11, 23, 45),
            TimeSpanProperty       = new TimeSpan(123L),
            DateTimeOffsetProperty = new DateTimeOffset(new DateTime(2013, 1, 13)),
        };

        internal static Object3 GetObject3() => new Object3
        {
            ObjectProperty         = null,
            NullIntProperty        = null,
            NullLongProperty       = 1L,
            BoolProperty           = true,
            CharProperty           = 'A',
            ByteProperty           = (byte)1,
            SByteProperty          = (sbyte)1,
            ShortProperty          = (short)1,
            IntProperty            = (int)1,
            LongProperty           = (long)1,
            UShortProperty         = (ushort)1,
            UIntProperty           = (uint)1,
            ULongProperty          = (ulong)1,
            DoubleProperty         = 1.0,
            FloatProperty          = (float)1.0,
            DecimalProperty        = 1M,
            GuidProperty           = Guid.Empty,
            UriProperty            = new Uri("http://localhost"),
            DateTimeProperty       = new DateTime(2013, 1, 13),
            TimeSpanProperty       = new TimeSpan(123L),
            DateTimeOffsetProperty = new DateTimeOffset(new DateTime(2013, 1, 13)),
        };

        [TestMethod]
        public void TestDumpObject1_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullIntField             = <null>
  NullLongField            = 1
  ObjectField              = <null>
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  BoolProperty             = True
  ByteProperty             = 1
  CharProperty             = A
  DateTimeOffsetProperty   = 2013-01-13T00:00:00.0000000-05:00
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  DecimalProperty          = 1
  DoubleProperty           = 1
  FloatProperty            = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 1
  LongProperty             = 1
  NullIntProperty          = <null>
  NullLongProperty         = 1
  ObjectProperty           = <null>
  SByteProperty            = 1
  ShortProperty            = 1
  TimeSpanProperty         = 00:00:00.0000123
  UIntProperty             = 1
  ULongProperty            = 1
  UShortProperty           = 1
  UriProperty              = http://localhost/";

                target.Dump(GetObject1());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject1WithFields_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(
                                    w, 0, 2, DumpTextWriter.DefaultMaxLength,
                                    BindingFlags.DeclaredOnly|
                                        BindingFlags.Instance|
                                        BindingFlags.NonPublic|
                                        BindingFlags.Public,
                                    BindingFlags.DeclaredOnly|
                                        BindingFlags.Instance|
                                        BindingFlags.NonPublic|
                                        BindingFlags.Public);
                var expected = @"
Object1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullIntField             = <null>
  NullLongField            = 1
  ObjectField              = <null>
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  BoolProperty             = True
  ByteProperty             = 1
  CharProperty             = A
  DateTimeOffsetProperty   = 2013-01-13T00:00:00.0000000-05:00
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  DecimalProperty          = 1
  DoubleProperty           = 1
  FloatProperty            = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 1
  LongProperty             = 1
  NullIntProperty          = <null>
  NullLongProperty         = 1
  ObjectProperty           = <null>
  SByteProperty            = 1
  ShortProperty            = 1
  TimeSpanProperty         = 00:00:00.0000123
  UIntProperty             = 1
  ULongProperty            = 1
  UShortProperty           = 1
  UriProperty              = http://localhost/";

                target.Dump(GetObject1());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject1_1_Limited()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w, 0, 2, 500);
                var expected = @"
Object1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  Gui...
The dump exceeded the maximum length of 500 characters. Either increase the value of the argument maxDumpLength of the constructor of the ObjectTextDumper class, or suppress the dump of some types and properties using DumpAttribute-s and metadata.";

                target.Dump(GetObject1());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject1_2()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  ObjectProperty           : <null>
  NullIntProperty          = <null>
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullIntField             = <null>
  NullLongField            = 1
  ObjectField              = <null>
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  TimeSpanProperty         = 00:00:00.0000123";

                target.Dump(GetObject1(), typeof(Object1Metadata), null);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject1WithFieldsMetadata_2()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  ObjectProperty           : <null>
  NullIntProperty          = <null>
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullIntField             = <null>
  NullLongField            = 1
  ObjectField              = <null>
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  TimeSpanProperty         = 00:00:00.0000123";

                target.Dump(GetObject1(), typeof(Object1FieldsMetadata), null);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject1_3()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullLongField            = 1
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  TimeSpanProperty         = 00:00:00.0000123";

                target.Dump(GetObject1(), typeof(Object1Metadata), new DumpAttribute { DumpNullValues = ShouldDump.Skip });

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject2_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object2 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object2, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  DateTimeProperty1        = 1/25/2013 11:23:45 AM
  TimeSpanProperty         = 00:00:00.0000123";

                target.Dump(GetObject2());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject3_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object3 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object3, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  TimeSpanProperty         = 00:00:00.0000123";

                target.Dump(GetObject3());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class Object4_1
        {
            public Object4_1()
            {
                Property1 = "Property1";
                Property2 = "Property2";
            }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }

        class Object5_1
        {
            public Object5_1()
            {
                PropertyA = "PropertyA";
                PropertyB = "PropertyB";
                Associate = new Object4_1();
                Associate2 = new Object4_1();
            }

            [Dump(0)]
            public string PropertyA { get; set; }

            [Dump(1)]
            public string PropertyB { get; set; }

            [Dump(RecurseDump = ShouldDump.Skip, DefaultProperty = "Property1")]
            public Object4_1 Associate { get; set; }

            [Dump(RecurseDump = ShouldDump.Skip)]
            public Object4_1 Associate2 { get; set; }
        }

        [TestMethod]
        public void TestDumpObject5_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object5_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object5_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  PropertyA                = PropertyA
  PropertyB                = PropertyB
  Associate                = Object4_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
    Property1                = Property1
  Associate2               = Object4_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): ";

                target.Dump(new Object5_1());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        static bool TestMethod(int a) => a < 0;

        [TestMethod]
        public void TestDumpDBNull()
        {
            var dbnull = DBNull.Value;

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"DBNull";

                target.Dump(dbnull);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpExpression()
        {
            Expression<Func<int, int>> expression = a => 5;

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Expression<Func<int, int>> (System.Linq.Expressions.Expression`1[[System.Func`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089): 
  NodeType                 = ExpressionType.Lambda
  Name                     = <null>
  ReturnType               = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
  Parameters               = TrueReadOnlyCollection<ParameterExpression>[1]: (System.Runtime.CompilerServices.TrueReadOnlyCollection`1[[System.Linq.Expressions.ParameterExpression, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    PrimitiveParameterExpression<int> (System.Linq.Expressions.PrimitiveParameterExpression`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089): 
      NodeType                 = ExpressionType.Parameter
      Name                     = a
      IsByRef                  = False
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      CanReduce                = False
  Body                     = ConstantExpression (System.Linq.Expressions.ConstantExpression, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089): 
    NodeType                 = ExpressionType.Constant
    Value                    = 5
    Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
    CanReduce                = False
  Type                     = (TypeInfo): System.Func`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
  TailCall                 = False
  CanReduce                = False";

                target.Dump(expression);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class Object6
        {
            public Object6()
            {
                Property1 = "Property1";
                Property2 = "Property2";
                d = TestMethod;
                ex = p => p > 0;
            }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public Func<int, bool> d { get; set; }

            [Dump(ValueFormat = "ToString()")]
            public Expression<Func<int, bool>> ex { get; set; }
        }

        [TestMethod]
        public void TestDumpObject6_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object6 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object6, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Property1                = Property1
  Property2                = Property2
  d                        = static ObjectTextDumperTest.TestMethod
  ex                       = p => (p > 0)";

                target.Dump(new Object6());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class Object7
        {
            public Object7()
            {
                Property1 = "Property1";
                Property2 = "Property2";
                Array = new[] { 0, 1, 2, 3, 4, 5, };
            }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public int[] Array { get; set; }
        }

        [TestMethod]
        public void TestDumpObject7_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object7 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object7, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Array                    = int[6]: (System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    0
    1
    2
    3
    4
    5
  Property1                = Property1
  Property2                = Property2";

                target.Dump(new Object7());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject7_2()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var test = new Object7
                {
                    Array = new[] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, },
                };
                var expected = @"
Object7 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object7, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Array                    = int[18]: (System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    0
    1
    2
    3
    4
    5
    0
    1
    2
    3
    ... dumped the first 10 elements.
  Property1                = Property1
  Property2                = Property2";

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class Object8
        {
            public Object8()
            {
                Property1 = "Property1";
                Property2 = "Property2";
                Array = new[] { 0, 1, 2, 3, 4, 5, };
                Array2 = new[] { 0, 1, 2, 3, 4, 5, };
                Array3 = new byte[] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, };
                Array4 = new byte[] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, };
                Array5 = new byte[] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, };
                Array6 = new ArrayList { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, };
            }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
            [Dump(MaxLength = 3)]
            public int[] Array { get; set; }
            [Dump(RecurseDump = ShouldDump.Skip)]
            public int[] Array2 { get; set; }
            [Dump(MaxLength = -1)]
            public byte[] Array3 { get; set; }
            [Dump()]
            public byte[] Array4 { get; set; }
            [Dump(MaxLength = 3)]
            public byte[] Array5 { get; set; }
            [Dump(MaxLength = 3)]
            public ArrayList Array6 { get; set; }
        }

        [TestMethod]
        public void TestDumpObject8_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object8 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object8, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Array                    = int[6]: (System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    0
    1
    2
    ... dumped the first 3 elements.
  Array2                   = int[6]: (System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
  Array3                   = byte[18]: 00-01-02-03-04-05-00-01-02-03-04-05-00-01-02-03-04-05
  Array4                   = byte[18]: 00-01-02-03-04-05-00-01-02-03... dumped the first 10 elements.
  Array5                   = byte[18]: 00-01-02... dumped the first 3 elements.
  Array6                   = ArrayList[18]: (System.Collections.ArrayList, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    0
    1
    2
    ... dumped the first 3 elements.
  Property1                = Property1
  Property2                = Property2";

                target.Dump(new Object8());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class Object7_1
        {
            public Object7_1()
            {
                Property1 = "Property1";
                Property2 = "Property2";
                List = new List<int> { 0, 1, 2, 3, 4, 5, };
            }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public List<int> List { get; set; }
        }

        [TestMethod]
        public void TestDumpObject7_1_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object7_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object7_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  List                     = List<int>[6]: (System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    0
    1
    2
    3
    4
    5
  Property1                = Property1
  Property2                = Property2";

                target.Dump(new Object7_1());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpObject7_1_2()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var test = new Object7_1
                {
                    List = new List<int> { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, },
                };
                var expected = @"
Object7_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object7_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  List                     = List<int>[18]: (System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    0
    1
    2
    3
    4
    5
    0
    1
    2
    3
    ... dumped the first 10 elements.
  Property1                = Property1
  Property2                = Property2";

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class Object8_1
        {
            public Object8_1()
            {
                Property1 = "Property1";
                Property2 = "Property2";
                List = new[] { 0, 1, 2, 3, 4, 5, };
                List2 = new[] { 0, 1, 2, 3, 4, 5, };
            }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
            [Dump(MaxLength = 3)]
            public int[] List { get; set; }
            [Dump(RecurseDump = ShouldDump.Skip)]
            public int[] List2 { get; set; }
        }

        [TestMethod]
        public void TestDumpObject8_1_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object8_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object8_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  List                     = int[6]: (System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    0
    1
    2
    ... dumped the first 3 elements.
  List2                    = int[6]: (System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
  Property1                = Property1
  Property2                = Property2";

                target.Dump(new Object8_1());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class Object90
        {
            public TestEnum Prop { get; set; }
            public TestFlags Flags { get; set; }
        }

        class Object9
        {
            public Object9()
            {
                Object90 = new Object90 { Prop=TestEnum.One, Flags = TestFlags.Two | TestFlags.Four };
                Prop91 = 0;
                Prop92 = 1;
                Prop93 = 4;
                Prop94 = 5;
            }

            [Dump(0)]
            public Object90 Object90 { get; set; }
            [Dump(1)]
            public int Prop91 { get; set; }
            [Dump(2)]
            public int Prop92 { get; set; }
            [Dump(-1)]
            public int Prop93 { get; set; }
            [Dump(-2)]
            public int Prop94 { get; set; }
            public string this[int index]
            {
                get { return index.ToString(); }
                set { }
            }
        }

        class Object91 : Object9
        {
            public Object91()
            {
                Prop911 = 2;
                Prop912 = 3;
                Prop913 = 6;
                Prop914 = 7;
            }

            [Dump(0)]
            public int Prop911 { get; set; }
            [Dump(1)]
            public int Prop912 { get; set; }
            [Dump(-1)]
            public int Prop913 { get; set; }
            [Dump(-2)]
            public int Prop914 { get; set; }
            [Dump(-3)]
            public Object90 InheritedObject90 => Object90;
        }

        [TestMethod]
        public void TestDumpObject9_1()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
Object91 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object91, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Object90                 = Object90 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object90, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
    Flags                    = TestFlags (Two | Four)
    Prop                     = TestEnum.One
  Prop91                   = 0
  Prop92                   = 1
  Prop911                  = 2
  Prop912                  = 3
  Prop913                  = 6
  Prop914                  = 7
  InheritedObject90        = Object90 (see above)
  Prop93                   = 4
  Prop94                   = 5";

                target.Dump(new Object91());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        public class Object10
        {
            public int Offset { get; set; }
            public static string Static(int param) => param.ToString();
            public string Instance(int param) => (Offset+param).ToString();
        }

        public class ObjectWithDelegates
        {
            public ObjectWithDelegates()
            {
                Object10Prop = new Object10 { Offset = 23 };
                DelegateProp1 = Object10.Static;
                DelegateProp2 = Object10Prop.Instance;
                DelegateProp3 = TestMethod;
            }

            public Object10 Object10Prop { get; set; }
            public Func<int, string> DelegateProp0 { get; set; }
            public Func<int, string> DelegateProp1 { get; set; }
            public Func<int, string> DelegateProp2 { get; set; }
            public Func<int, bool> DelegateProp3 { get; set; }
        }

        [TestMethod]
        public void TestDumpObjectWithDelegates()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
ObjectWithDelegates (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+ObjectWithDelegates, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  DelegateProp0            = <null>
  DelegateProp1            = static Object10.Static
  DelegateProp2            = Object10.Instance
  DelegateProp3            = static ObjectTextDumperTest.TestMethod
  Object10Prop             = Object10 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object10, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
    Offset                   = 23";

                target.Dump(new ObjectWithDelegates());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class MyEnumerable : IEnumerable
        {
            [Dump(true)]
            List<int> List { get; }

            public MyEnumerable()
            {
                List = new List<int> { 0, 1, 3, };
            }

            public string Property { get; set; }

            #region IEnumerable Members
            public IEnumerator GetEnumerator() => List.GetEnumerator();
            #endregion
        }

        class ObjectWithMyEnumerable
        {
            public ObjectWithMyEnumerable()
            {
                Stuff = "stuff";
                MyEnumerable = new MyEnumerable();
            }

            public string Stuff { get; set; }
            public MyEnumerable MyEnumerable { get; set; }
        }

        [TestMethod]
        public void TestDumpObjectWithMyEnumerable()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
ObjectWithMyEnumerable (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+ObjectWithMyEnumerable, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  MyEnumerable             = MyEnumerable (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+MyEnumerable, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
    List                     = List<int>[3]: (System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
      0
      1
      3
    Property                 = <null>
  Stuff                    = stuff";

                target.Dump(new ObjectWithMyEnumerable());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class ObjectWithMembers
        {
            public int Property { get; set; }
            public void Method1()
            {
            }
            public int Method2(int a) => a;
            public string Method3(int a, int b) => (a+b).ToString();
            public T Method4<T>(int a, int b) where T : new() => new T();
            public T Method5<T, U>(int a, int b) where T : new() => new T();
            public string this[int index] => index.ToString();
            public string this[string index, int index1]
            {
                get { return index+index1; }
                set { }
            }

            public event EventHandler Event;
            public int member;
        }

        class ObjectWithMemberInfos
        {
            public Type Type { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
            public PropertyInfo IndexerIntInfo { get; set; }
            public PropertyInfo IndexerStringInfo { get; set; }
            public MethodInfo Method1Info { get; set; }
            public MethodInfo Method2Info { get; set; }
            public MethodInfo Method3Info { get; set; }
            public MethodInfo Method4Info { get; set; }
            public MethodInfo Method5Info { get; set; }
            public EventInfo EventInfo { get; set; }
            public MemberInfo MemberInfo { get; set; }
            public MemberInfo[] MemberInfos { get; set; }

            public ObjectWithMemberInfos()
            {
                Type = typeof(ObjectWithMembers);
                foreach (var pi in Type.GetProperties())
                {
                    if (pi.Name == "Property")
                        PropertyInfo = pi;
                    else
                        if (pi.GetIndexParameters()[0].ParameterType == typeof(int))
                        IndexerIntInfo = pi;
                    else
                            if (pi.GetIndexParameters()[0].ParameterType == typeof(string))
                        IndexerStringInfo = pi;
                }
                Method1Info = Type.GetMethod("Method1");
                Method2Info = Type.GetMethod("Method2");
                Method3Info = Type.GetMethod("Method3");
                Method4Info = Type.GetMethod("Method4");
                Method5Info = Type.GetMethod("Method5");
                EventInfo   = Type.GetEvent("Event");
                MemberInfo  = Type.GetMember("member")[0];
                MemberInfos = Type.GetMembers();
            }
        }

        [TestMethod]
        public void TestDumpObjectWithMemberInfos()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);
                var expected = @"
ObjectWithMemberInfos (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+ObjectWithMemberInfos, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  EventInfo                = (Event): EventHandler ObjectWithMembers.Event
  IndexerIntInfo           = (Property): String ObjectWithMembers.this[Int32] { get; }
  IndexerStringInfo        = (Property): String ObjectWithMembers.this[String, Int32] { get;set; }
  MemberInfo               = (Field): Int32 ObjectWithMembers.member
  MemberInfos              = MemberInfo[22]: (System.Reflection.MemberInfo[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    (Method): Int32 ObjectWithMembers.get_Property()
    (Method): Void ObjectWithMembers.set_Property(Int32 value)
    (Method): Void ObjectWithMembers.Method1()
    (Method): Int32 ObjectWithMembers.Method2(Int32 a)
    (Method): String ObjectWithMembers.Method3(Int32 a, Int32 b)
    (Method): T ObjectWithMembers.Method4<T>(Int32 a, Int32 b)
    (Method): T ObjectWithMembers.Method5<T, U>(Int32 a, Int32 b)
    (Method): String ObjectWithMembers.get_Item(Int32 index)
    (Method): String ObjectWithMembers.get_Item(String index, Int32 index1)
    (Method): Void ObjectWithMembers.set_Item(String index, Int32 index1, String value)
    ... dumped the first 10 elements.
  Method1Info              = (Method): Void ObjectWithMembers.Method1()
  Method2Info              = (Method): Int32 ObjectWithMembers.Method2(Int32 a)
  Method3Info              = (Method): String ObjectWithMembers.Method3(Int32 a, Int32 b)
  Method4Info              = (Method): T ObjectWithMembers.Method4<T>(Int32 a, Int32 b)
  Method5Info              = (Method): T ObjectWithMembers.Method5<T, U>(Int32 a, Int32 b)
  PropertyInfo             = (Property): Int32 ObjectWithMembers.Property { get;set; }
  Type                     = (NestedType): vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+ObjectWithMembers, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393";

                target.Dump(new ObjectWithMemberInfos());

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [Dump(MaxDepth = 3)]
        class NestedItem
        {
            public int Property { get; set; }
            public NestedItem Next { get; set; }
        }

        [TestMethod]
        public void TestDumpNestedObject()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var test = new NestedItem
                {
                    Property = 0,
                    Next = new NestedItem
                    {
                        Property = 1,
                        Next = new NestedItem
                        {
                            Property = 2,
                            Next = new NestedItem
                            {
                                Property = 3,
                                Next = new NestedItem
                                {
                                    Property = 4,
                                    Next = null,
                                }
                            }
                        },
                    },
                };
                var target = new ObjectTextDumper(w);
                var expected = @"
NestedItem (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+NestedItem, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Next                     = NestedItem (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+NestedItem, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
    Next                     = NestedItem (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+NestedItem, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
      Next                     = NestedItem (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+NestedItem, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
        Next                     = ...object dump reached the maximum depth level. Use the DumpAttribute.MaxDepth to increase the depth level if needed.
        Property                 = 3
      Property                 = 2
    Property                 = 1
  Property                 = 0";

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestCollectionObject()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var test = new List<string> { "one", "two", "three" };
                var target = new ObjectTextDumper(w);
                var expected = @"
List<string>[3]: (System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
  one
  two
  three";

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDictionaryBaseTypes()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var test = new Dictionary<string,int>
                {
                    ["one"] = 1,
                    ["two"] = 2,
                    ["three"] = 3,
                };
                var target = new ObjectTextDumper(w);
                var expected = @"
Dictionary<string, int>[3]: (System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
{
  [one] = 1;
  [two] = 2;
  [three] = 3;
}";

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDictionaryBaseTypeAndObject()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var test = new Dictionary<int,Object4_1>
                {
                    [1] = new Object4_1 { Property1 = "one" },
                    [2] = new Object4_1 { Property1 = "two" },
                    [3] = new Object4_1 { Property1 = "three" },
                };
                var target = new ObjectTextDumper(w);
                var expected = @"
Dictionary<int, Object4_1>[3]: (System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
{
  [1] = Object4_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
      Property1                = one
      Property2                = Property2;
  [2] = Object4_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
      Property1                = two
      Property2                = Property2;
  [3] = Object4_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
      Property1                = three
      Property2                = Property2;
}";

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        class Base
        {
            public virtual string StringProperty { get; set; }
            public int IntProperty { get; set; }
        }

        class Derived : Base
        {
            public override string StringProperty { get; set; }
            public new int IntProperty { get; set; }
        }

        [TestMethod]
        public void TestVirtualProperties()
        {
            var test = new Derived
            {
                StringProperty = "StringProperty",
                IntProperty    = 5,
            };
            var expected = @"
Derived (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Derived, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  IntProperty              = 0
  StringProperty           = StringProperty
  IntProperty              = 5";

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpOfDynamic()
        {
            dynamic test = new { IntProperty = 10, StringProperty = "hello", DoubleProperty = Math.PI, };
            var expected = @"
<>f__AnonymousType0<int, string, double> (<>f__AnonymousType0`3[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  DoubleProperty           = 3.14159265358979
  IntProperty              = 10
  StringProperty           = hello";

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpOfExpando()
        {
            dynamic test = new ExpandoObject();

            test.IntProperty = 10;
            test.StringProperty = "hello";
            test.DoubleProperty = Math.PI;

            var expected = @"
ExpandoObject[]: (System.Dynamic.ExpandoObject, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
  KeyValuePair<string, object> (System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089): 
    Key                      = IntProperty
    Value                    = 10
  KeyValuePair<string, object> (System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089): 
    Key                      = StringProperty
    Value                    = hello
  KeyValuePair<string, object> (System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089): 
    Key                      = DoubleProperty
    Value                    = 3.14159265358979";

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        T GetSandboxedObject<T>(SecurityZone zone)
        {
            // get the permission set:
            Evidence evidence = new Evidence();

            evidence.AddHostEvidence(new Zone(zone));

            var permissionSet = SecurityManager.GetStandardSandbox(evidence);

            // create the app domain:
            AppDomainSetup setup = new AppDomainSetup();

            setup.ApplicationBase = Path.GetDirectoryName(typeof(T).Assembly.Location);

            AppDomain domain = AppDomain.CreateDomain(
                                    "TestSandbox",
                                    null,
                                    setup,
                                    permissionSet);

            //create a remote instance of the T class
            return (T)Activator.CreateInstanceFrom(
                                    domain,
                                    typeof(T).Assembly.ManifestModule.FullyQualifiedName,
                                    typeof(T).FullName)
                               .Unwrap();
        }

        [TestMethod]
        public void TestCallFromSandbox()
        {
            Object1Dump od = GetSandboxedObject<Object1Dump>(SecurityZone.Internet);

            var actual = od.DumpObject1();
            var expected = @"
vm.Aspects.Diagnostics.ObjectDumper.Tests.PartialTrust.Object1 (Note: The caller does not have the permission to use reflection. Therefore System.Object.ToString() on the object has been dumped instead.)";

            TestContext.WriteLine("{0}", actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCallFromSandbox1()
        {
            var actual = GetSandboxedObject<Object1Dump>(SecurityZone.MyComputer).DumpObject1();
            var expected = @"
Object1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.PartialTrust.Object1, vm.Aspects.Diagnostics.ObjectDumperPartialTrustTestStub, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a64e726b1908ae7b): 
  BoolProperty             = True
  ByteProperty             = 1
  CharProperty             = A
  DateTimeOffsetProperty   = 2013-01-13T00:00:00.0000000-05:00
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  DecimalProperty          = 1
  DoubleProperty           = 1
  FloatProperty            = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 1
  LongProperty             = 1
  NullIntProperty          = <null>
  NullLongProperty         = 1
  ObjectProperty           = <null>
  SByteProperty            = 1
  ShortProperty            = 1
  TimeSpanProperty         = 00:00:00.0000123
  UIntProperty             = 1
  ULongProperty            = 1
  UShortProperty           = 1
  UriProperty              = http://localhost/";

            TestContext.WriteLine("{0}", actual);
            Assert.AreEqual(expected, actual);
        }

        static class Object11Dumper
        {
            public static string DumpObject11(Object11 value) => $"Dumped by Objec11Dumper.Dump1: {value.StringProperty}";
            public static string Dump(Object11 value) => $"Dumped by Objec11Dumper.Dump: {value.StringProperty}";
        }

        class Object11
        {
            public Object11()
            {
                StringProperty = "string value";
            }

            public string StringProperty { get; set; }

            public string DumpMe() => $"Dumped by Objec11.DumpMe: {StringProperty}";

            public static string DumpMeStatic(Object11 value) => $"Dumped by Objec11.DumpMeStatic: {value.StringProperty}";
        }

        class Object11_1 : Object11
        {
        }

        class Object12
        {

            public Object12()
            {
                Object11Property_1 = new Object11();
                Object11Property_2 = new Object11();
                Object11Property_3 = new Object11();
                Object11Property_4 = new Object11();
                Object11Property_11 = new Object11_1();
                Object11Property_21 = new Object11_1();
                Object11Property_31 = new Object11_1();
                Object11Property_41 = new Object11_1();
                Object11Property_51 = new Object11_1();
            }

            [Dump(DumpClass = typeof(Object11Dumper), DumpMethod = "DumpObject11")]
            public Object11 Object11Property_1 { get; set; }

            [Dump(DumpClass = typeof(Object11Dumper))]
            public Object11 Object11Property_2 { get; set; }

            [Dump(DumpMethod = "DumpMe")]
            public Object11 Object11Property_3 { get; set; }

            [Dump(DumpMethod = "DumpMeStatic")]
            public Object11 Object11Property_4 { get; set; }



            [Dump(DumpClass = typeof(Object11Dumper), DumpMethod = "DumpObject11")]
            public Object11_1 Object11Property_11 { get; set; }

            [Dump(DumpClass = typeof(Object11Dumper))]
            public Object11_1 Object11Property_21 { get; set; }

            [Dump(DumpMethod = "DumpMe")]
            public Object11_1 Object11Property_31 { get; set; }

            [Dump(DumpMethod = "DumpMeStatic")]
            public Object11_1 Object11Property_41 { get; set; }


            [Dump(DumpMethod = "DumpMeNoneSuch")]
            public Object11_1 Object11Property_51 { get; set; }

        }

        [TestMethod]
        public void TestDumpClassDumpMethod()
        {
            var expected = @"
Object12 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object12, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Object11Property_1       = Dumped by Objec11Dumper.Dump1: string value
  Object11Property_11      = Dumped by Objec11Dumper.Dump1: string value
  Object11Property_2       = Dumped by Objec11Dumper.Dump: string value
  Object11Property_21      = Dumped by Objec11Dumper.Dump: string value
  Object11Property_3       = Dumped by Objec11.DumpMe: string value
  Object11Property_31      = Dumped by Objec11.DumpMe: string value
  Object11Property_4       = Dumped by Objec11.DumpMeStatic: string value
  Object11Property_41      = Dumped by Objec11.DumpMeStatic: string value
  Object11Property_51      = *** Could not find a public instance method with name DumpMeNoneSuch and no parameters or static method with a single parameter of type vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object11_1, with return type of System.String in the class vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object11_1.";
            var test = new Object12();

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Debug.WriteLine("{0}", actual, null);
                Assert.AreEqual(expected, actual);
            }
        }

        class Object13
        {
            public string Prop1 { get; set; }

            [Dump(true)]
            public string Prop2 { get; set; }

            [Dump(Skip = ShouldDump.Dump)]
            public string Prop3 { get; set; }

            [Dump(Skip = ShouldDump.Skip)]
            public string Prop4 { get; set; }
        }

        [TestMethod]
        public void TestOptInDump()
        {
            var expected = @"
Object13 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object13, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Prop2                    = <null>
  Prop3                    = <null>";
            var test = new Object13();

            DumpAttribute.Default.Skip = ShouldDump.Skip;

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = new ObjectTextDumper(w);

                target.Dump(test);

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Debug.WriteLine("{0}", actual, null);
                Assert.AreEqual(expected, actual);
            }

            DumpAttribute.Default.Skip = ShouldDump.Dump;
        }
    }
}