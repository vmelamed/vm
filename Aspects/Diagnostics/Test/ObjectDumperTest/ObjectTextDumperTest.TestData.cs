using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace vm.Aspects.Diagnostics.ObjectDumper.Tests
{
    public partial class ObjectTextDumperTest
    {
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

        public class Object1
        {
            public object ObjectProperty { get; set; } = null;
            public int? NullIntProperty { get; set; } = null;
            public long? NullLongProperty { get; set; } = 1L;
            public bool BoolProperty { get; set; } = true;
            public char CharProperty { get; set; } = 'A';
            public byte ByteProperty { get; set; } = (byte)1;
            public sbyte SByteProperty { get; set; } = (sbyte)1;
            public short ShortProperty { get; set; } = (short)1;
            public int IntProperty { get; set; } = (int)1;
            public long LongProperty { get; set; } = (long)1;
            public ushort UShortProperty { get; set; } = (ushort)1;
            public uint UIntProperty { get; set; } = (uint)1;
            public ulong ULongProperty { get; set; } = (ulong)1;
            public double DoubleProperty { get; set; } = 1.0;
            public float FloatProperty { get; set; } = (float)1.0;
            public decimal DecimalProperty { get; set; } = 1M;
            public Guid GuidProperty { get; set; } = Guid.Empty;
            public Uri UriProperty { get; set; } = new Uri("http://localhost");
            public DateTime DateTimeProperty { get; set; } = new DateTime(2013, 1, 13);
            public TimeSpan TimeSpanProperty { get; set; } = new TimeSpan(123L);
            public DateTimeOffset DateTimeOffsetProperty { get; set; } = new DateTimeOffset(new DateTime(2013, 1, 13));


            public object ObjectField                 = null;
            public int? NullIntField                  = null;
            public long? NullLongField                = 1L;
            public bool BoolField                     = true;
            public char CharField                     = 'A';
            public byte ByteField                     = (byte)1;
            public sbyte SByteField                   = (sbyte)1;
            public short ShortField                   = (short)1;
            public int IntField                       = (int)1;
            public long LongField                     = (long)1;
            public ushort UShortField                 = (ushort)1;
            public uint UIntField                     = (uint)1;
            public ulong ULongField                   = (ulong)1;
            public double DoubleField                 = 1.0;
            public float FloatField                   = (float)1.0;
            public decimal DecimalField               = 1M;
            public Guid GuidField                     = Guid.Empty;
            public Uri UriField                       = new Uri("http://localhost");
            public DateTime DateTimeField             = new DateTime(2013, 1, 13);
            public TimeSpan TimeSpanField             = new TimeSpan(123L);
            public DateTimeOffset DateTimeOffsetField = new DateTimeOffset(new DateTime(2013, 1, 13));
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
            public object ObjectProperty { get; set; } = null;
            [Dump(1)]
            public int? NullIntProperty { get; set; } = null;
            [Dump(2)]
            public long? NullLongProperty { get; set; } = 1L;
            [Dump(3)]
            public bool BoolProperty { get; set; } = true;
            [Dump(4)]
            public char CharProperty { get; set; } = 'A';
            [Dump(5)]
            public byte ByteProperty { get; set; } = (byte)1;
            [Dump(6)]
            public sbyte SByteProperty { get; set; } = (sbyte)1;
            [Dump(7)]
            public short ShortProperty { get; set; } = (short)1;
            [Dump(8)]
            public int IntProperty { get; set; } = (int)1;
            [Dump(9)]
            public long LongProperty { get; set; } = (long)1;
            [Dump(false)]
            public ushort UShortProperty { get; set; } = (ushort)1;
            [Dump(-1)]
            public uint UIntProperty { get; set; } = (uint)1;
            [Dump(-2)]
            public ulong ULongProperty { get; set; } = (ulong)1;
            [Dump(-3, ValueFormat = "{0:F01}")]
            public double DoubleProperty { get; set; } = 1.0;
            [Dump(-4, ValueFormat = "{0:F01}")]
            public float FloatProperty { get; set; } = (float)1.0;
            [Dump(-5)]
            public decimal DecimalProperty { get; set; } = 1M;
            [Dump(-6)]
            public Guid GuidProperty { get; set; } = Guid.Empty;
            [Dump(-7)]
            public Uri UriProperty { get; set; } = new Uri("http://localhost");
            [Dump(-8)]
            public DateTime DateTimeProperty { get; set; } = new DateTime(2013, 1, 13);
            [Dump(-9, ValueFormat = "ToString()")]
            public DateTime DateTimeProperty1 { get; set; } = new DateTime(2013, 1, 25, 11, 23, 45);
            [Dump(-10)]
            public TimeSpan TimeSpanProperty { get; set; } = new TimeSpan(123L);
            [Dump(false)]
            public DateTimeOffset DateTimeOffsetProperty { get; set; } = new DateTimeOffset(new DateTime(2013, 1, 13));
        }

        [MetadataType(typeof(Object3Metadata))]
        public class Object3
        {
            public object ObjectProperty { get; set; } = null;
            public int? NullIntProperty { get; set; } = null;
            public long? NullLongProperty { get; set; } = 1L;
            public bool BoolProperty { get; set; } = true;
            public char CharProperty { get; set; } = 'A';
            public byte ByteProperty { get; set; } = (byte)1;
            public sbyte SByteProperty { get; set; } = (sbyte)1;
            public short ShortProperty { get; set; } = (short)1;
            public int IntProperty { get; set; } = (int)1;
            public long LongProperty { get; set; } = (long)1;
            public ushort UShortProperty { get; set; } = (ushort)1;
            public uint UIntProperty { get; set; } = (uint)1;
            public ulong ULongProperty { get; set; } = (ulong)1;
            public double DoubleProperty { get; set; } = 1.0;
            public float FloatProperty { get; set; } = (float)1.0;
            public decimal DecimalProperty { get; set; } = 1M;
            public Guid GuidProperty { get; set; } = Guid.Empty;
            public Uri UriProperty { get; set; } = new Uri("http://localhost");
            public DateTime DateTimeProperty { get; set; } = new DateTime(2013, 1, 13);
            public TimeSpan TimeSpanProperty { get; set; } = new TimeSpan(123L);
            public DateTimeOffset DateTimeOffsetProperty { get; set; } = new DateTimeOffset(new DateTime(2013, 1, 13));
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

        abstract class MetaObject7_1
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            [Dump(MaxLength = -1)]
            public List<int> List { get; set; }
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

#pragma warning disable 67, 618
            public event EventHandler Event;
            public int member;
#pragma warning restore 67, 618
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

        public class DavidATest
        {
            public int A { get; } = 10;
            public int[] Array { get; } = new[] { 1, 2, 3 };
            public int B { get; } = 6;
        }

        public class Object14_1
        {
            public int Property1 { get; set; }
            public string Property2 { get; set; }
        }

        public class Object14
        {
            public int Property11 { get; set; }
            public string Property12 { get; set; }
            public ICollection<Object14_1> Collection { get; set; }
        }

        public class BaseClass
        {
            public BaseClass()
            {
                Property = 0;
                VirtualProperty1 = 1;
                VirtualProperty2 = 2;
            }
            public int Property { get; set; }
            public virtual int VirtualProperty1 { get; set; }
            public virtual int VirtualProperty2 { get; set; }
        }

        public class Descendant1 : BaseClass
        {
        }

        public class Descendant2 : Descendant1
        {
            public Descendant2()
            {
                VirtualProperty1 = 21;
            }
            public override int VirtualProperty1 { get; set; }
        }

        public class Descendant3 : Descendant2
        {
            public Descendant3()
            {
                VirtualProperty2 = 32;
            }

            public override int VirtualProperty2 { get; set; }
        }
    }
}
