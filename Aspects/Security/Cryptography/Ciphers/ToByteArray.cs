using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ToByteArray contains a number of functions which convert various data types to byte array.
    /// Utility class that can be used in the IHasher and ICipher extension methods.
    /// </summary>
    static class ToByteArray
    {
        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(bool data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(bool[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(bool);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(char data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(char[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            return Convert(new string(data));
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(byte data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return new byte[] { data };
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// This method is here only for completeness.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(byte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            return data;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(sbyte data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(sbyte[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(sbyte);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(short data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(short[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(short);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(ushort data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(ushort[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(ushort);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(int data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(int[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(int);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(uint data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(uint[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(uint);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(long data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(long[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(long);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(ulong data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(ulong[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(ulong);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(float data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(float[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(float);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(double data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(double[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(double);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    BitConverter.GetBytes(data[i]), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(decimal data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            var bits = decimal.GetBits(data);

            Contract.Assume(bits != null);

            return Convert(bits);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(decimal[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(int)*4;
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                var bits = decimal.GetBits(data[i]);

                Contract.Assume(bits != null);

                Array.Copy(
                    Convert(bits), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(string data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            return Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(DateTime data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return Convert(data.ToBinary());
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(DateTime[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = sizeof(long);
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    Convert(data[i].ToBinary()), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(Guid data)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return data.ToByteArray();
        }

        /// <summary>
        /// Converts the specified data to byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] Convert(Guid[] data)
        {
            Contract.Ensures(!(data == null ^ Contract.Result<byte[]>() == null));

            if (data == null)
                return null;

            int elementSize = 16;
            byte[] bytes = new byte[data.Length * elementSize];
            int index = 0;

            for (var i = 0; i < data.Length; i++)
            {
                Array.Copy(
                    data[i].ToByteArray(), 0,
                    bytes, index,
                    elementSize);

                index += elementSize;
            }

            return bytes;
        }

        /// <summary>
        /// Dictionary of types and the corresponding methods that can decrypt those types.
        /// </summary>
        public readonly static IReadOnlyDictionary<Type, Func<object, byte[]>> ConvertTypedData = new ReadOnlyDictionary<Type, Func<object, byte[]>>( new Dictionary<Type, Func<object, byte[]>>
        {
            #region ConvertTypedData
            [typeof(bool)]       = d => Convert((bool)      d),
            [typeof(bool[])]     = d => Convert((bool[])    d),
            [typeof(char)]       = d => Convert((char)      d),
            [typeof(char[])]     = d => Convert((char[])    d),
            [typeof(sbyte)]      = d => Convert((sbyte)     d),
            [typeof(sbyte[])]    = d => Convert((sbyte[])   d),
            [typeof(byte)]       = d => Convert((byte)      d),
            [typeof(byte[])]     = d => Convert((byte[])    d),
            [typeof(short)]      = d => Convert((short)     d),
            [typeof(short[])]    = d => Convert((short[])   d),
            [typeof(ushort)]     = d => Convert((ushort)    d),
            [typeof(ushort[])]   = d => Convert((ushort[])  d),
            [typeof(int)]        = d => Convert((int)       d),
            [typeof(int[])]      = d => Convert((int[])     d),
            [typeof(uint)]       = d => Convert((uint)      d),
            [typeof(uint[])]     = d => Convert((uint[])    d),
            [typeof(long)]       = d => Convert((long)      d),
            [typeof(long[])]     = d => Convert((long[])    d),
            [typeof(ulong)]      = d => Convert((ulong)     d),
            [typeof(ulong[])]    = d => Convert((ulong[])   d),
            [typeof(float)]      = d => Convert((float)     d),
            [typeof(float[])]    = d => Convert((float[])   d),
            [typeof(double)]     = d => Convert((double)    d),
            [typeof(double[])]   = d => Convert((double[])  d),
            [typeof(decimal)]    = d => Convert((decimal)   d),
            [typeof(decimal[])]  = d => Convert((decimal[]) d),
            [typeof(DateTime)]   = d => Convert((DateTime)  d),
            [typeof(DateTime[])] = d => Convert((DateTime[])d),
            [typeof(Guid)]       = d => Convert((Guid)      d),
            [typeof(Guid[])]     = d => Convert((Guid[])    d),
            [typeof(string)]     = d => Convert((string)    d),
            #endregion
        });

        public static byte[] Convert(
            object data,
            Type dataType)
        {
            Contract.Requires<ArgumentNullException>(dataType != null, nameof(dataType));

            if (!ConvertTypedData.ContainsKey(dataType))
                throw new ArgumentException("The specified data type cannot be converted.");

            return ConvertTypedData[dataType](data);
        }

        public static byte[] Convert<T>(
            T data)
        {
            if (!ConvertTypedData.ContainsKey(typeof(T)))
                throw new ArgumentException("The specified data type cannot be converted.");

            return Convert(data, typeof(T));
        }

        public static byte[] ConvertNullable<T>(
            Nullable<T> data) where T : struct
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (!ConvertTypedData.ContainsKey(typeof(T)))
                throw new ArgumentException("The specified data type cannot be converted.");

            var hasValueArray = Convert(data.HasValue);
            var valueArray    = Convert(data.GetValueOrDefault());
            var array         = new byte[hasValueArray.Length + valueArray.Length];

            Array.Copy(hasValueArray, array, hasValueArray.Length);
            Array.Copy(valueArray, 0, array, hasValueArray.Length, valueArray.Length);

            return array;
        }
    }
}
