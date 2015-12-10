using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json.Linq;
using vm.Aspects.Security.Cryptography.Ciphers;
using vm.Aspects.Threading;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Class EncryptedParts encrypts and decrypts the properties and fields of any visibility of an object
    /// to and from the properties and fields marked by the <see cref="DecryptedAttribute.EncryptedIn"/>.
    /// The class is useful when there are more than one property encrypted in one <see cref="DecryptedAttribute.EncryptedIn"/>.
    /// For single properties of primitive type, string, DateTime and Guid, it would be faster to use directly 
    /// the <see cref="ICipherExtensions"/> overrides of Encrypt and Decrypt.
    /// </summary>
    public class EncryptedParts
    {
        /// <summary>
        /// The binding flags used to find the encrypted parts (properties and fields).
        /// </summary>
        const BindingFlags EncryptedPartsFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// The synchronization lock of the static map of decrypted parts.
        /// </summary>
        static ReaderWriterLockSlim _lockMapDecryptedParts = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// A map of <see cref="MemberInfo"/> objects representing the mapped properties and fields, grouped by the name of the property containing the encrypted value.
        /// </summary>
        static IDictionary<Type, IEnumerable<IGrouping<string, MemberInfo>>> _mapDecryptedParts = new Dictionary<Type, IEnumerable<IGrouping<string, MemberInfo>>>(64);

        /// <summary>
        /// The object with encrypted parts that this instance operates on.
        /// </summary>
        object _instance;

        /// <summary>
        /// The type of the <see cref="_instance"/>.
        /// </summary>
        Type _type;

        /// <summary>
        /// The Cipher used to en/decrypt the encrypted parts.
        /// </summary>
        ICipher _cipher;

        /// <summary>
        /// The decrypted underlying JSON string.
        /// </summary>
        IDictionary<string, string> _decryptedJsonStrings = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedParts" /> class with an object that needs encryption/decryption of some of its properties and/or fields.
        /// </summary>
        /// <param name="instance">The object with encrypted parts.</param>
        /// <param name="cipher">The cipher used for en/decryption.</param>
        public EncryptedParts(
            object instance,
            ICipher cipher)
        {
            Contract.Requires<ArgumentNullException>(instance != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(cipher != null, nameof(cipher));

            _instance = instance;
            _type     = _instance.GetType();
            _cipher   = cipher;

            Verify();
        }

        /// <summary>
        /// Verifies that the member <see cref="_instance"/> has properties marked with the attribute <see cref="DecryptedAttribute"/>, 
        /// that these properties can be encrypted and 
        /// that the properties specified in the property <see cref="DecryptedAttribute.EncryptedIn"/> exist and their types are byte arrays.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        void Verify()
        {
            var mapDecryptedParts = ListDecryptedParts();

            // make sure that all EncryptedIn exist and are byte[]:
            if (!mapDecryptedParts.All(g => typeof(byte[]) == (_type.GetProperty(g.Key, EncryptedPartsFlags)?.PropertyType  ??
                                                               _type.GetField(g.Key, EncryptedPartsFlags)?.FieldType)))
                throw new InvalidOperationException("The property names specified in DecryptedAttribute."+
                                                    nameof(DecryptedAttribute.EncryptedIn)+
                                                    " must be names of existing properties or fields of type byte array.");

            // make sure that all properties marked with DecryptedAttribute can be encrypted
            if (!mapDecryptedParts.Select(g => (IEnumerable<MemberInfo>)g)
                                  .SelectMany(mi => mi)
                                  .All(mi =>
                                  {
                                      var pi = mi as PropertyInfo;
                                      var pType = pi != null ? pi.PropertyType : ((FieldInfo)mi).FieldType;

                                      return pType.IsSerializable  &&
                                             ICipherExtensions.DecryptTypedData.ContainsKey(pType);
                                  }))
                throw new InvalidOperationException("The properties marked with DecryptedAttribute must be encryptable: must be of any of the following types: "+
                                                    "bool, char, byte, sbyte, short, ushort, int, uint, long, ulong, decimal, float, double, DateTime, Guid, "+
                                                    "or arrays or nullable of those, or a string.");
        }

        /// <summary>
        /// Lists the decrypted parts (property info and group info) in a collection of groupings by the name of the EncryptedIn property/field.
        /// </summary>
        /// <returns>The sequence of grouped property infos and field infos.</returns>
        IEnumerable<IGrouping<string, MemberInfo>> ListDecryptedParts()
        {
            IEnumerable<IGrouping<string, MemberInfo>> mapDecryptedParts = null;

            using (_lockMapDecryptedParts.UpgradableReaderLock())
                if (!_mapDecryptedParts.TryGetValue(_type, out mapDecryptedParts))
                {
                    mapDecryptedParts = _type.GetMembers(EncryptedPartsFlags)
                                             .Where(mi => (mi.MemberType == MemberTypes.Property || mi.MemberType == MemberTypes.Field)  &&
                                                          mi.IsDefined(typeof(DecryptedAttribute), true))
                                             .GroupBy(mi => mi.GetCustomAttribute<DecryptedAttribute>(true).EncryptedIn ?? DecryptedAttribute.DefaultEncryptedProperty)
                                             .ToList()
                                             ;

                    using (_lockMapDecryptedParts.WriterLock())
                        _mapDecryptedParts[_type] = mapDecryptedParts;
                }

            return mapDecryptedParts;
        }

        /// <summary>
        /// Encrypts all properties and fields of the associated <see cref="_instance"/>.
        /// </summary>
        public void Encrypt()
        {
            var decryptedParts = ListDecryptedParts();

            // for each grouping
            foreach (var g in decryptedParts)
                Encrypt(g);
        }

        /// <summary>
        /// Encrypts the properties and fields encrypted in the property or field named <paramref name="encryptedIn"/>.
        /// </summary>
        /// <param name="encryptedIn">The property or field which needs to be populated with the encrypted value of the associated properties and fields.</param>
        public void Encrypt(
            string encryptedIn)
        {
            var decryptedParts = ListDecryptedParts();
            var g = decryptedParts.FirstOrDefault(gr => gr.Key == encryptedIn);

            if (g == null)
                throw new ArgumentException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The property {0} is not specified in a "+nameof(DecryptedAttribute)+"."+nameof(DecryptedAttribute.EncryptedIn)+" property.",
                                encryptedIn));

            Encrypt(g);
        }

        /// <summary>
        /// Encrypts the properties and fields in the specified grouping.
        /// </summary>
        /// <param name="g">The grouping which properties and fields should be encrypted.</param>
        void Encrypt(
            IGrouping<string, MemberInfo> g)
        {
            // build a JObject
            var jo = new JObject();

            foreach (var mi in g)
            {
                var pi = mi as PropertyInfo;
                var fi = pi != null ? null : mi as FieldInfo;

                jo.Add(pi != null
                            ? new JProperty(pi.Name, pi.GetValue(_instance))
                            : new JProperty(fi.Name, fi.GetValue(_instance)));
            }

            // serialize the JObject to JSON string
            var json = jo.ToString();
            string decryptedJsonString;

            // if the new JSON is the same as the cached JSON - no need for encryption.
            if (_decryptedJsonStrings.TryGetValue(g.Key, out decryptedJsonString)  &&
                json == decryptedJsonString)
                return;

            // encrypt the JSON
            var piEncrypted = _type.GetProperty(g.Key, EncryptedPartsFlags);
            var fiEncrypted = piEncrypted != null ? null : _type.GetField(g.Key, EncryptedPartsFlags);
            var newValue    = _cipher.Encrypt(json);

            // set the value of the EncryptedIn property or field
            if (piEncrypted != null)
                piEncrypted.SetValue(_instance, newValue);
            else
                fiEncrypted.SetValue(_instance, newValue);
        }

        /// <summary>
        /// Decrypts the properties and fields of the associated <see cref="_instance"/>.
        /// </summary>
        public void Decrypt()
        {
            var decryptedParts = ListDecryptedParts();

            // for each grouping
            foreach (var g in decryptedParts)
                Decrypt(g);
        }

        /// <summary>
        /// Decrypts the properties and fields encrypted in the property or field named <paramref name="encryptedIn"/>.
        /// </summary>
        /// <param name="encryptedIn">The property or field which needs to be populated with the encrypted value of the associated properties and fields.</param>
        public void Decrypt(
            string encryptedIn)
        {
            var decryptedParts = ListDecryptedParts();
            var g = decryptedParts.FirstOrDefault(gr => gr.Key == encryptedIn);

            if (g == null)
                throw new ArgumentException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The property {0} is not specified in a "+nameof(DecryptedAttribute)+"."+nameof(DecryptedAttribute.EncryptedIn)+" property.",
                                encryptedIn));

            Decrypt(g);
        }

        /// <summary>
        /// Decrypts the properties and fields of the <see cref="_instance"/>.
        /// </summary>
        void Decrypt(
            IGrouping<string, MemberInfo> g)
        {
            // if it has been decrypted already - return
            if (_decryptedJsonStrings.ContainsKey(g.Key))
                return;

            // get the value of the EncryptedIn property or field
            var piEncrypted = _type.GetProperty(g.Key, EncryptedPartsFlags);
            var fiEncrypted = piEncrypted != null ? null : _type.GetField(g.Key, EncryptedPartsFlags);
            var value       = piEncrypted != null
                                    ? (byte[])piEncrypted.GetValue(_instance)
                                    : (byte[])fiEncrypted.GetValue(_instance);

            // decrypt it
            var json = _cipher.Decrypt<string>(value);

            // cache it
            _decryptedJsonStrings[g.Key] = json;

            // parse into JObject
            var jo = JObject.Parse(json);

            // set the properties and fields
            foreach (var p in jo.Properties())
            {
                var pi = _type.GetProperty(p.Name, EncryptedPartsFlags);
                var fi = pi != null ? null : _type.GetField(p.Name, EncryptedPartsFlags);

                if (pi != null)
                    pi.SetValue(_instance, pi != null
                                                ? p.ToObject(pi.PropertyType)
                                                : p.ToObject(fi.FieldType));
            }
        }
    }
}
