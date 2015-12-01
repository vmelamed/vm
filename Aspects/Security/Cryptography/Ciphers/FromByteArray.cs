using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class FromByteArray contains a number of functions which convert byte arrays to various data types.
    /// Utility class that can be used in the IHasher and ICipher extension methods.
    /// </summary>
    static class FromByteArray
    {
        public static bool ToBoolean(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));

            return BitConverter.ToBoolean(data, 0);
        }

        public static bool[] ToBooleanArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<bool[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(bool);
            var array = new bool[data.Length];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToBoolean(data, index);
                index += elementSize;
            }

            return array;
        }

        public static char ToChar(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));

            return BitConverter.ToChar(data, 0);
        }

        public static char[] ToCharArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<char[]>() == null));

            if (data == null)
                return null;

            return ToString(data).ToCharArray();
        }

        public static byte ToByte(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length > 0, "The encrypted value does not represent a valid array.");

            return (byte)data[0];
        }

        public static byte[] ToByteArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            return data;
        }

        public static sbyte ToSByte(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length > 0, "The encrypted value does not represent a valid array.");

            return (sbyte)data[0];
        }

        public static sbyte[] ToSByteArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<sbyte[]>() == null));

            if (data == null)
                return null;

            var array = new sbyte[data.Length];

            for (var i = 0; i<array.Length; i++)
                array[i] = unchecked((sbyte)data[i]);

            return array;
        }

        public static short ToInt16(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(Int16), "The encrypted value does not represent a valid array.");

            return BitConverter.ToInt16(data, 0);
        }

        public static short[] ToInt16Array(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<short[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(short);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new short[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToInt16(data, index);
                index += elementSize;
            }

            return array;
        }

        public static ushort ToUInt16(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(UInt16), "The encrypted value does not represent a valid array.");

            return BitConverter.ToUInt16(data, 0);
        }

        public static ushort[] ToUInt16Array(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<ushort[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(ushort);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new ushort[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToUInt16(data, index);
                index += elementSize;
            }

            return array;
        }

        public static int ToInt32(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(Int32), "The encrypted value does not represent a valid array.");

            return BitConverter.ToInt32(data, 0);
        }

        public static int[] ToInt32Array(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<int[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(int);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new int[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToInt32(data, index);
                index += elementSize;
            }

            return array;
        }

        public static uint ToUInt32(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(UInt32), "The encrypted value does not represent a valid array.");

            return BitConverter.ToUInt32(data, 0);
        }

        public static uint[] ToUInt32Array(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<uint[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(uint);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new uint[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToUInt32(data, index);
                index += elementSize;
            }

            return array;
        }

        public static long ToInt64(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(Int64), "The encrypted value does not represent a valid array.");

            return BitConverter.ToInt64(data, 0);
        }

        public static long[] ToInt64Array(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<long[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(long);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new long[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToInt64(data, index);
                index += elementSize;
            }

            return array;
        }

        public static ulong ToUInt64(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(UInt64), "The encrypted value does not represent a valid array.");

            return BitConverter.ToUInt64(data, 0);
        }

        public static ulong[] ToUInt64Array(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<ulong[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(ulong);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new ulong[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToUInt64(data, index);
                index += elementSize;
            }

            return array;
        }

        public static float ToSingle(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(float), "The encrypted value does not represent a valid array.");

            return BitConverter.ToSingle(data, 0);
        }

        public static float[] ToSingleArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<float[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(float);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new float[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToSingle(data, index);
                index += elementSize;
            }

            return array;
        }

        public static double ToDouble(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(double), "The encrypted value does not represent a valid array.");

            return BitConverter.ToDouble(data, 0);
        }

        public static double[] ToDoubleArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<double[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(double);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new double[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = BitConverter.ToDouble(data, index);
                index += elementSize;
            }

            return array;
        }

        public static decimal ToDecimal(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(decimal), "The encrypted value does not represent a valid array.");

            return new decimal(ToInt32Array(data));
        }

        public static decimal[] ToDecimalArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<decimal[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(int) * 4;
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var intData = ToInt32Array(data);
            var array = new decimal[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new decimal(
                                    intData[index+0],
                                    intData[index+1],
                                    intData[index+2],
                                    (intData[index+3] & 0x80000000) != 0,
                                    (byte)((intData[index+3] >> 16) & 0x7F));
                index += 4;
            }

            return array;
        }

        public static string ToString(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<string>() == null));

            if (data == null)
                return null;

            return Encoding.UTF8.GetString(data);
        }

        public static DateTime ToDateTime(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == sizeof(Int64), "The encrypted value does not represent a valid array.");

            return new DateTime(ToInt64(data));
        }

        public static DateTime[] ToDateTimeArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<DateTime[]>() == null));

            if (data == null)
                return null;

            var elementSize = sizeof(long);
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var array = new DateTime[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new DateTime(BitConverter.ToInt64(data, index));
                index += elementSize;
            }

            return array;
        }

        public static Guid ToGuid(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));
            Contract.Requires<ArgumentException>(data.Length == 16, "The encrypted value does not represent a valid array.");

            return new Guid(data);
        }

        public static Guid[] ToGuidArray(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<Guid[]>() == null));

            if (data == null)
                return null;

            var elementSize = 16;
            int reminder;
            var count = Math.DivRem(data.Length, elementSize, out reminder);

            if (reminder != 0)
                throw new ArgumentException("The encrypted value does not represent a valid array.");

            var guidData = new byte[elementSize];
            var array = new Guid[count];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                Array.Copy(data, index, guidData, 0, elementSize);
                array[i] = new Guid(guidData);
                index += elementSize;
            }

            return array;
        }
    }
}
