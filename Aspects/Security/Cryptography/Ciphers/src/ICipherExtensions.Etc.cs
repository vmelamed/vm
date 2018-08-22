using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class ICipherExtensions contains extension methods for <see cref="ICipherTasks"/> objects.
    /// </summary>
    public static partial class ICipherExtensions
    {
        #region En/Decrypt DateTime-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            DateTime data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static DateTime DecryptDateTime(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 8)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToDateTime(decrypted);
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            DateTime[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="DateTime"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static DateTime[] DecryptDateTimeArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToDateTimeArray(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt decimal-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            decimal data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static decimal DecryptDecimal(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 16)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToDecimal(decrypted);
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            decimal[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="decimal"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static decimal[] DecryptDecimalArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToDecimalArray(cipher.Decrypt(encrypted));
        }
        #endregion

        #region En/Decrypt string-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            string data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encryptedText"/> to a string, provided the original string was encoded with <see cref="T:System.Text.Encoding.UTF8"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedText">The crypto text.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static string DecryptString(
            this ICipher cipher,
            byte[] encryptedText)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encryptedText == null)
                return null;

            return FromByteArray.ToString(cipher.Decrypt(encryptedText));
        }

        /// <summary>
        /// Encrypts the <paramref name="text"/> and encodes the result with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="text">The text.</param>
        /// <returns>The encrypted text encoded Base64.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static string EncryptText64(
            this ICipher cipher,
            string text)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var encryptedText = Convert.ToBase64String(cipher.Encrypt(text));

            cipher.Base64Encoded = base64;

            return encryptedText;
        }

        /// <summary>
        /// Decrypts the <paramref name="encryptedText64"/> to a string, provided the crypto text was encoded with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedText64">The crypto text encoded Base64.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static string DecryptText64(
            this ICipher cipher,
            string encryptedText64)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encryptedText64 == null)
                return null;

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var decryptedData = cipher.DecryptString(Convert.FromBase64String(encryptedText64));

            cipher.Base64Encoded = base64;

            return decryptedData;
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The text.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [Obsolete("Use the overloaded cipher.Encrypt(string data) instead.")]
        public static byte[] EncryptText(
            this ICipher cipher,
            string data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return cipher.Encrypt(data);
        }

        /// <summary>
        /// Decrypts the <paramref name="encryptedText"/> to a string, provided the original string was encoded with <see cref="T:System.Text.Encoding.UTF8"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedText">The crypto text.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [Obsolete("Use DecryptString instead.")]
        public static string DecryptText(
            this ICipher cipher,
            byte[] encryptedText)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encryptedText == null)
                return null;

            return cipher.DecryptString(encryptedText);
        }
        #endregion

        #region En/Decrypt Guid-s
        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            Guid data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> text to a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted text.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cipher"/> or <paramref name="encrypted"/> are <see langword="null"/>.</exception>
        public static Guid DecryptGuid(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length != 16)
                throw new ArgumentException(Resources.InvalidEncryptedValue);

            return FromByteArray.ToGuid(decrypted);
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static byte[] Encrypt(
            this ICipher cipher,
            Guid[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (data == null)
                return null;

            return cipher.Encrypt(ToByteArray.Convert(data));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> to an array of <see cref="Guid"/> values.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The data to be decrypted.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        public static Guid[] DecryptGuidArray(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encrypted == null)
                return null;

            return FromByteArray.ToGuidArray(cipher.Decrypt(encrypted));
        }
        #endregion

        /// <summary>
        /// Encrypts the <paramref name="data"/> with the specified <paramref name="cipher"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data to be encrypted.</typeparam>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted data.</returns>
        public static byte[] EncryptNullable<T>(
            this ICipher cipher,
            T? data) where T : struct
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (!EncryptTypedData.ContainsKey(typeof(T)))
                throw new ArgumentException("The specified data type cannot be converted.", nameof(data));

            return cipher.Encrypt(ToByteArray.ConvertNullable(data));
        }

        /// <summary>
        /// Dictionary of types and the corresponding methods that can decrypt those types.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public readonly static IReadOnlyDictionary<Type, Func<ICipher, object, byte[]>> EncryptTypedData =
            new ReadOnlyDictionary<Type, Func<ICipher, object, byte[]>>( new Dictionary<Type, Func<ICipher, object, byte[]>>
            {
                #region EncryptTypedData
                [typeof(bool)]       = (c,d) => c.Encrypt((bool)      d),
                [typeof(bool[])]     = (c,d) => c.Encrypt((bool[])    d),
                [typeof(char)]       = (c,d) => c.Encrypt((char)      d),
                [typeof(char[])]     = (c,d) => c.Encrypt((char[])    d),
                [typeof(sbyte)]      = (c,d) => c.Encrypt((sbyte)     d),
                [typeof(sbyte[])]    = (c,d) => c.Encrypt((sbyte[])   d),
                [typeof(byte)]       = (c,d) => c.Encrypt((byte)      d),
                [typeof(byte[])]     = (c,d) => c.Encrypt((byte[])    d),
                [typeof(short)]      = (c,d) => c.Encrypt((short)     d),
                [typeof(short[])]    = (c,d) => c.Encrypt((short[])   d),
                [typeof(ushort)]     = (c,d) => c.Encrypt((ushort)    d),
                [typeof(ushort[])]   = (c,d) => c.Encrypt((ushort[])  d),
                [typeof(int)]        = (c,d) => c.Encrypt((int)       d),
                [typeof(int[])]      = (c,d) => c.Encrypt((int[])     d),
                [typeof(uint)]       = (c,d) => c.Encrypt((uint)      d),
                [typeof(uint[])]     = (c,d) => c.Encrypt((uint[])    d),
                [typeof(long)]       = (c,d) => c.Encrypt((long)      d),
                [typeof(long[])]     = (c,d) => c.Encrypt((long[])    d),
                [typeof(ulong)]      = (c,d) => c.Encrypt((ulong)     d),
                [typeof(ulong[])]    = (c,d) => c.Encrypt((ulong[])   d),
                [typeof(float)]      = (c,d) => c.Encrypt((float)     d),
                [typeof(float[])]    = (c,d) => c.Encrypt((float[])   d),
                [typeof(double)]     = (c,d) => c.Encrypt((double)    d),
                [typeof(double[])]   = (c,d) => c.Encrypt((double[])  d),
                [typeof(decimal)]    = (c,d) => c.Encrypt((decimal)   d),
                [typeof(decimal[])]  = (c,d) => c.Encrypt((decimal[]) d),
                [typeof(DateTime)]   = (c,d) => c.Encrypt((DateTime)  d),
                [typeof(DateTime[])] = (c,d) => c.Encrypt((DateTime[])d),
                [typeof(Guid)]       = (c,d) => c.Encrypt((Guid)      d),
                [typeof(Guid[])]     = (c,d) => c.Encrypt((Guid[])    d),
                [typeof(string)]     = (c,d) => c.Encrypt((string)    d),

                [typeof(bool?)]      = (c,d) => c.EncryptNullable((bool?)d),
                [typeof(char?)]      = (c,d) => c.EncryptNullable((char?)d),
                [typeof(sbyte?)]     = (c,d) => c.EncryptNullable((sbyte?)d),
                [typeof(byte?)]      = (c,d) => c.EncryptNullable((byte?)d),
                [typeof(short?)]     = (c,d) => c.EncryptNullable((short?)d),
                [typeof(ushort?)]    = (c,d) => c.EncryptNullable((ushort?)d),
                [typeof(int?)]       = (c,d) => c.EncryptNullable((int?)d),
                [typeof(uint?)]      = (c,d) => c.EncryptNullable((uint?)d),
                [typeof(long?)]      = (c,d) => c.EncryptNullable((long?)d),
                [typeof(ulong?)]     = (c,d) => c.EncryptNullable((ulong?)d),
                [typeof(float?)]     = (c,d) => c.EncryptNullable((float?)d),
                [typeof(double?)]    = (c,d) => c.EncryptNullable((double?)d),
                [typeof(decimal?)]   = (c,d) => c.EncryptNullable((decimal?)d),
                [typeof(DateTime?)]  = (c,d) => c.EncryptNullable((DateTime?)d),
                [typeof(Guid?)]      = (c,d) => c.EncryptNullable((Guid?)d),
                #endregion
            });

        /// <summary>
        /// Encrypts the <paramref name="data"/> with the <paramref name="cipher"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <returns>The encrypted data.</returns>
        public static byte[] Encrypt(
            this ICipher cipher,
            object data,
            Type dataType)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));

            if (!EncryptTypedData.ContainsKey(dataType))
                throw new ArgumentException("The specified data type cannot be encrypted.", nameof(dataType));

            return EncryptTypedData[dataType](cipher, data);
        }

        /// <summary>
        /// Encrypts the <paramref name="data"/> with the <paramref name="cipher"/>.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data.</param>
        /// <returns>The encrypted data.</returns>
        public static byte[] Encrypt<T>(
            this ICipher cipher,
            T data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (!EncryptTypedData.ContainsKey(typeof(T)))
                throw new ArgumentException("The specified data type cannot be encrypted.");

            return Encrypt(cipher, data, typeof(T));
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> data with the <paramref name="cipher"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted.</param>
        /// <returns>T.</returns>
        public static Nullable<T> DecryptNullable<T>(
            this ICipher cipher,
            byte[] encrypted) where T : struct
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (encrypted == null)
                throw new ArgumentNullException(nameof(encrypted));
            if (encrypted.Length < 2)
                throw new ArgumentException(Resources.InvalidArgumentLength, nameof(encrypted));
            if (!EncryptTypedData.ContainsKey(typeof(T)))
                throw new ArgumentException("The specified data type cannot be decrypted.");

            var decrypted = cipher.Decrypt(encrypted);

            if (decrypted.Length < 2)
                throw new ArgumentException("The argument is not a valid encrypted Nullable<T> value.");

            return FromByteArray.ToNullable<T>(decrypted);
        }

        /// <summary>
        /// Dictionary of types and the corresponding methods that can decrypt those types.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public readonly static IReadOnlyDictionary<Type, Func<ICipher, byte[], object>> DecryptTypedData = new ReadOnlyDictionary<Type, Func<ICipher, byte[], object>>( new Dictionary<Type, Func<ICipher, byte[], object>>
        {
            #region DecryptTypedData
            [typeof(bool)]       = (c,d) => c.DecryptBoolean(d),
            [typeof(bool[])]     = (c,d) => c.DecryptBooleanArray(d),
            [typeof(char)]       = (c,d) => c.DecryptChar(d),
            [typeof(char[])]     = (c,d) => c.DecryptCharArray(d),
            [typeof(sbyte)]      = (c,d) => c.DecryptSByte(d),
            [typeof(sbyte[])]    = (c,d) => c.DecryptSByteArray(d),
            [typeof(byte)]       = (c,d) => c.DecryptByte(d),
            [typeof(byte[])]     = (c,d) => c.Decrypt(d),
            [typeof(short)]      = (c,d) => c.DecryptInt16(d),
            [typeof(short[])]    = (c,d) => c.DecryptInt16Array(d),
            [typeof(ushort)]     = (c,d) => c.DecryptUInt16(d),
            [typeof(ushort[])]   = (c,d) => c.DecryptUInt16Array(d),
            [typeof(int)]        = (c,d) => c.DecryptInt32(d),
            [typeof(int[])]      = (c,d) => c.DecryptInt32Array(d),
            [typeof(uint)]       = (c,d) => c.DecryptUInt32(d),
            [typeof(uint[])]     = (c,d) => c.DecryptUInt32Array(d),
            [typeof(long)]       = (c,d) => c.DecryptInt64(d),
            [typeof(long[])]     = (c,d) => c.DecryptInt64Array(d),
            [typeof(ulong)]      = (c,d) => c.DecryptUInt64(d),
            [typeof(ulong[])]    = (c,d) => c.DecryptUInt64Array(d),
            [typeof(float)]      = (c,d) => c.DecryptSingle(d),
            [typeof(float[])]    = (c,d) => c.DecryptSingleArray(d),
            [typeof(double)]     = (c,d) => c.DecryptDouble(d),
            [typeof(double[])]   = (c,d) => c.DecryptDoubleArray(d),
            [typeof(decimal)]    = (c,d) => c.DecryptDecimal(d),
            [typeof(decimal[])]  = (c,d) => c.DecryptDecimalArray(d),
            [typeof(DateTime)]   = (c,d) => c.DecryptDateTime(d),
            [typeof(DateTime[])] = (c,d) => c.DecryptDateTimeArray(d),
            [typeof(Guid)]       = (c,d) => c.DecryptGuid(d),
            [typeof(Guid[])]     = (c,d) => c.DecryptGuidArray(d),
            [typeof(string)]     = (c,d) => c.DecryptString(d),

            [typeof(bool?)]      = (c,d) => c.DecryptNullable<bool>(d),
            [typeof(char?)]      = (c,d) => c.DecryptNullable<char>(d),
            [typeof(sbyte?)]     = (c,d) => c.DecryptNullable<sbyte>(d),
            [typeof(byte?)]      = (c,d) => c.DecryptNullable<byte>(d),
            [typeof(short?)]     = (c,d) => c.DecryptNullable<short>(d),
            [typeof(ushort?)]    = (c,d) => c.DecryptNullable<ushort>(d),
            [typeof(int?)]       = (c,d) => c.DecryptNullable<int>(d),
            [typeof(uint?)]      = (c,d) => c.DecryptNullable<uint>(d),
            [typeof(long?)]      = (c,d) => c.DecryptNullable<long>(d),
            [typeof(ulong?)]     = (c,d) => c.DecryptNullable<ulong>(d),
            [typeof(float?)]     = (c,d) => c.DecryptNullable<float>(d),
            [typeof(double?)]    = (c,d) => c.DecryptNullable<double>(d),
            [typeof(decimal?)]   = (c,d) => c.DecryptNullable<decimal>(d),
            [typeof(DateTime?)]  = (c,d) => c.DecryptNullable<DateTime>(d),
            [typeof(Guid?)]      = (c,d) => c.DecryptNullable<Guid>(d), 
#endregion
        });

        /// <summary>
        /// Decrypts the <paramref name="encrypted" /> data with the <paramref name="cipher" />.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted data.</param>
        /// <param name="dataType">Type of the encrypted data.</param>
        /// <returns>T.</returns>
        public static object Decrypt(
            this ICipher cipher,
            byte[] encrypted,
            Type dataType)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));

            if (!DecryptTypedData.ContainsKey(dataType))
                throw new ArgumentException("The specified data type cannot be decrypted.");

            return DecryptTypedData[dataType](cipher, encrypted);
        }

        /// <summary>
        /// Decrypts the <paramref name="encrypted"/> data with the <paramref name="cipher"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encrypted">The encrypted.</param>
        /// <returns>T.</returns>
        public static T Decrypt<T>(
            this ICipher cipher,
            byte[] encrypted)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (!DecryptTypedData.ContainsKey(typeof(T)))
                throw new ArgumentException("The specified data type cannot be decrypted.");

            return (T)Decrypt(cipher, encrypted, typeof(T));
        }

        #region Obsolete Base64 methods.
        /// <summary>
        /// Encrypts the array of bytes <paramref name="data"/> and encodes the result with Base64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <returns>The encrypted text encoded Base64.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [Obsolete("Chain cipher.Encrypt(data).ToBase64String() instead.")]
        public static string EncryptData64(
            this ICipher cipher,
            byte[] data)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var encryptedText = Convert.ToBase64String(cipher.Encrypt(data));

            cipher.Base64Encoded = base64;

            return encryptedText;
        }

        /// <summary>
        /// Decrypts the string <paramref name="encryptedData64"/> to an array of bytes, provided the crypto text was encoded with Byte64.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="encryptedData64">The crypto text encoded Base64.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="cipher"/> is <see langword="null"/>.</exception>
        [Obsolete("Use cipher.Encrypt(encryptedData64.FromBase64String()) instead.")]
        public static byte[] DecryptData64(
            this ICipher cipher,
            string encryptedData64)
        {
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            if (encryptedData64 == null)
                return null;

            var base64 = cipher.Base64Encoded;

            cipher.Base64Encoded = false;

            var decryptedData = cipher.Decrypt(Convert.FromBase64String(encryptedData64));

            cipher.Base64Encoded = base64;

            return decryptedData;
        }
        #endregion
    }
}