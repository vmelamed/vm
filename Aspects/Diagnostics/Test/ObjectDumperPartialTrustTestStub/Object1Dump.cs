using System;

namespace vm.Aspects.Diagnostics.ObjectDumper.Tests.PartialTrust
{
    public class Object1
    {
#pragma warning disable 1720
        public object ObjectProperty { get; set; }
        public int? NullIntProperty { get; set; }
        public long? NullLongProperty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool")]
        public bool BoolProperty { get; set; }
        public char CharProperty { get; set; }
        public byte ByteProperty { get; set; }
        public sbyte SByteProperty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "short")]
        public short ShortProperty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
        public int IntProperty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long LongProperty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ushort")]
        public ushort UShortProperty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "uint")]
        public uint UIntProperty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ulong")]
        public ulong ULongProperty { get; set; }
        public double DoubleProperty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "float")]
        public float FloatProperty { get; set; }
        public decimal DecimalProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public Uri UriProperty { get; set; }
        public DateTime DateTimeProperty { get; set; }
        public TimeSpan TimeSpanProperty { get; set; }
        public DateTimeOffset DateTimeOffsetProperty { get; set; }
#pragma warning restore 1720
    }

    public class Object1Dump : MarshalByRefObject
    {
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
                                                };                                  

        public virtual string DumpObject1() => GetObject1().DumpString();
    }
}
