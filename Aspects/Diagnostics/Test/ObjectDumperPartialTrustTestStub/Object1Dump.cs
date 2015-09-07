using System;

namespace vm.Aspects.Diagnostics.ObjectDumper.Test.PartialTrust
{
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
    }

    public class Object1Dump : MarshalByRefObject
    {
        internal static Object1 GetObject1()
        {
            return new Object1
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
        }

        public string DumpObject1()
        {
            return GetObject1().DumpString();
        }
    }
}
