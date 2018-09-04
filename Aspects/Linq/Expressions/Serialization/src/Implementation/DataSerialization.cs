using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    /// <summary>
    /// Contains utilities for serializing and de-serializing objects.
    /// </summary>
    public static class DataSerialization
    {
        const string _anonymousTypePrefix = "<>f__AnonymousType";

        #region constant serializers
        /// <summary>
        /// The map of base type constants serializers
        /// </summary>
        static IDictionary<Type, Action<object, Type, XElement>> _constantSerializers = new Dictionary<Type, Action<object, Type, XElement>>
        {
            { typeof(bool),     (c, t, x) => x.Add(new XElement(XNames.Elements.Boolean,       c!=null ? XmlConvert.ToString((bool)c)     : null)) },
            { typeof(byte),     (c, t, x) => x.Add(new XElement(XNames.Elements.UnsignedByte,  c!=null ? XmlConvert.ToString((byte)c)     : null)) },
            { typeof(sbyte),    (c, t, x) => x.Add(new XElement(XNames.Elements.Byte,          c!=null ? XmlConvert.ToString((sbyte)c)    : null)) },
            { typeof(short),    (c, t, x) => x.Add(new XElement(XNames.Elements.Short,         c!=null ? XmlConvert.ToString((short)c)    : null)) },
            { typeof(ushort),   (c, t, x) => x.Add(new XElement(XNames.Elements.UnsignedShort, c!=null ? XmlConvert.ToString((ushort)c)   : null)) },
            { typeof(int),      (c, t, x) => x.Add(new XElement(XNames.Elements.Int,           c!=null ? XmlConvert.ToString((int)c)      : null)) },
            { typeof(uint),     (c, t, x) => x.Add(new XElement(XNames.Elements.UnsignedInt,   c!=null ? XmlConvert.ToString((uint)c)     : null)) },
            { typeof(long),     (c, t, x) => x.Add(new XElement(XNames.Elements.Long,          c!=null ? XmlConvert.ToString((long)c)     : null)) },
            { typeof(ulong),    (c, t, x) => x.Add(new XElement(XNames.Elements.UnsignedLong,  c!=null ? XmlConvert.ToString((ulong)c)    : null)) },
            { typeof(float),    (c, t, x) => x.Add(new XElement(XNames.Elements.Float,         c!=null ? XmlConvert.ToString((float)c)    : null)) },
            { typeof(double),   (c, t, x) => x.Add(new XElement(XNames.Elements.Double,        c!=null ? XmlConvert.ToString((double)c)   : null)) },
            { typeof(decimal),  (c, t, x) => x.Add(new XElement(XNames.Elements.Decimal,       c!=null ? XmlConvert.ToString((decimal)c)  : null)) },
            { typeof(Guid),     (c, t, x) => x.Add(new XElement(XNames.Elements.Guid,          c!=null ? XmlConvert.ToString((Guid)c)     : null)) },
            { typeof(Uri),      (c, t, x) => x.Add(new XElement(XNames.Elements.AnyURI,        c!=null ? ((Uri)c).ToString()              : null)) },
            { typeof(TimeSpan), (c, t, x) => x.Add(new XElement(XNames.Elements.Duration,      c!=null ? XmlConvert.ToString((TimeSpan)c) : null)) },
            { typeof(string),   (c, t, x) => x.Add(new XElement(XNames.Elements.String,        c))                                                 },
            { typeof(DBNull),   (c, t, x) => x.Add(new XElement(XNames.Elements.DBNull))                                                           },
            { typeof(char),     (c, t, x) => x.Add(new XElement(XNames.Elements.Char,          c!=null ? XmlConvert.ToString(Convert.ToInt32((char)c, CultureInfo.InvariantCulture))
                                                                                                                                          : null)) },
            { typeof(DateTime), (c, t, x) => x.Add(new XElement(XNames.Elements.DateTime,      c!=null ? XmlConvert.ToString((DateTime)c, XmlDateTimeSerializationMode.RoundtripKind)
                                                                                                                                          : null)) },
        };
        #endregion

        #region constant de-serializers
        /// <summary>
        /// The map of base type constants de-serializers
        /// </summary>
        static readonly IDictionary<XName, Func<XElement, Type, object>> _constantDeserializers = new Dictionary<XName, Func<XElement, Type, object>>
        {
            { XNames.Elements.Boolean,       (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToBoolean(x.Value)  : default },
            { XNames.Elements.UnsignedByte,  (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToByte(x.Value)     : default },
            { XNames.Elements.Byte,          (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToSByte(x.Value)    : default },
            { XNames.Elements.Short,         (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToInt16(x.Value)    : default },
            { XNames.Elements.UnsignedShort, (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToUInt16(x.Value)   : default },
            { XNames.Elements.Int,           (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToInt32(x.Value)    : default },
            { XNames.Elements.UnsignedInt,   (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToUInt32(x.Value)   : default },
            { XNames.Elements.Long,          (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToInt64(x.Value)    : default },
            { XNames.Elements.UnsignedLong,  (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToUInt64(x.Value)   : default },
            { XNames.Elements.Float,         (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToSingle(x.Value)   : default },
            { XNames.Elements.Double,        (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToDouble(x.Value)   : default },
            { XNames.Elements.Decimal,       (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToDecimal(x.Value)  : default },
            { XNames.Elements.Guid,          (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToGuid(x.Value)     : default },
            { XNames.Elements.Duration,      (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToTimeSpan(x.Value) : default },
            { XNames.Elements.AnyURI,        (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? new Uri(x.Value)               : null    },
            { XNames.Elements.String,        (x, t) => x.Value                                                                        },
            { XNames.Elements.DBNull,        (x, t) => DBNull.Value                                                                   },
            { XNames.Elements.Char,          (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? Convert.ToChar(Convert.ToInt32(x.Value, CultureInfo.InvariantCulture))
                                                                                                                            : default },
            { XNames.Elements.DateTime,      (x, t) => !string.IsNullOrWhiteSpace(x.Value) ? XmlConvert.ToDateTime(x.Value, XmlDateTimeSerializationMode.RoundtripKind)
                                                                                                                            : default },
            { XNames.Elements.Nullable,      DeserializeNullable                                                                      },
            { XNames.Elements.Enum,          DeserializeEnum                                                                          },
            { XNames.Elements.Custom,        DeserializeCustom                                                                        },
            { XNames.Elements.Anonymous,     DeserializeAnonymous                                                                     },
        };
        #endregion

        /// <summary>
        /// Adds a serializer.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="constantsType">Type of the constants.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <exception cref="System.ArgumentNullException">
        /// serializer
        /// or
        /// constantsType
        /// or
        /// elementName
        /// </exception>
        public static void AddSerializer(
            IConstantXmlSerializer serializer,
            Type constantsType,
            XName elementName)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (constantsType == null)
                throw new ArgumentNullException(nameof(constantsType));
            if (elementName == null)
                throw new ArgumentNullException(nameof(elementName));

            _constantSerializers[constantsType] = (c, t, x) => x.Add(serializer.Serialize(c, t));
            _constantDeserializers[elementName] = (x, t) => serializer.Deserialize(x, t);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> can be serialized.
        /// </summary>
        /// <remarks>
        /// A type is serializable if it is:
        /// <list type="bullet">
        ///     <item>primitive type (char, byte, int, long, etc.)</item>
        ///     <item>enum</item>
        ///     <item>DBNull</item>
        ///     <item>decimal</item>
        ///     <item>string</item>
        ///     <item>Guid</item>
        ///     <item>Uri</item>
        ///     <item>DateTime</item>
        ///     <item>TimeSpan</item>
        ///     <item>DateTimeOffset</item>
        ///     <item>IntPtr</item>
        ///     <item>UIntPtr</item>
        ///     <item>Anonymous</item>
        ///     <item>The type is marked with <see cref="DataContractAttribute"/></item>
        ///     <item>The type is marked with <see cref="SerializableAttribute"/></item>
        ///     <item>The type is array, enumerable, dictionary or Nullable&lt;&gt; of one of the types above.</item>
        /// </list>
        /// </remarks>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <see langword="true"/> if <paramref name="type"/> can be serialized; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool CanSerialize(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // yes if this is a basic type - perhaps the most common case
            if (type.IsBasicType())
                return true;

            // yes if this is a type marked with [DataContract], [MessageContract] or [CollectionDataContract]
            if (Attribute.IsDefined(type, typeof(DataContractAttribute))     ||
                Attribute.IsDefined(type, typeof(MessageContractAttribute))  ||
                Attribute.IsDefined(type, typeof(CollectionDataContractAttribute)))
                return true;

            // yes if it is an anonymous type and all of its properties serializable (recursively)
            if (type.Name.StartsWith(_anonymousTypePrefix, StringComparison.Ordinal))
            {
                var properties = type.GetProperties();

                for (var i = 0; i < properties.Length; i++)
                    if (!CanSerialize(properties[i].PropertyType))
                        return false;

                return true;
            }

            // no - if the type is not serializable
            if (!type.IsSerializable)
                return false;

            var type2 = type;

            // if the type is array - continue the test for the element type
            if (type2.IsArray)
                type2 = type.GetElementType();

            Debug.Assert(type2 != null);

            // if the type is generic collection - continue the test for the element type
            if (type2.IsGenericType &&
                type2.Namespace.StartsWith("System.Collections", StringComparison.Ordinal))
            {
                var enumerable = type2.GetInterfaces()
                                      .FirstOrDefault(
                                            i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (enumerable != null)
                {
                    type2 = enumerable.GetGenericArguments()[0];

                    Debug.Assert(type2 != null);

                    // if the collection is a dictionary - continue the test for both the key and the value types
                    if (type2.IsGenericType &&
                        type2.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                        return CanSerialize(type2.GetGenericArguments()[0]) &&
                               CanSerialize(type2.GetGenericArguments()[1]);
                }
            }

            // if the type is nullable - continue the test for the underlying type
            if (type2.IsGenericType &&
                type2.GetGenericTypeDefinition() == typeof(Nullable<>))
                type2 = type2.GetGenericArguments()[0];

            // if this is all we can do - return true, at least we know that the type is serializable. In the worst case it will throw serialization 
            // exception.
            if (type == type2)
                return true;

            // continue the test for the underlying type
            Debug.Assert(type2 != null);
            return CanSerialize(type2);
        }

        /// <summary>
        /// Gets a serializer for a value of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// A delegate that can serialize a value of the specified <paramref name="type"/> into an XML element (<see cref="XElement"/>).
        /// </returns>
        /// <exception cref="System.Runtime.Serialization.SerializationException"></exception>
        internal static Action<object, Type, XElement> GetSerializer(
            Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!CanSerialize(type))
                throw new SerializationException(
                            $"Don't know how to serialize type \"{type.AssemblyQualifiedName}\".");

            // get the serializer from the table, or
            if (_constantSerializers.TryGetValue(type, out var serializer))
                return serializer;

            // if it is an enum - return the SerializeEnum
            if (type.IsEnum)
                return SerializeEnum;

            // if it is a nullable - get nullable serializer or
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return SerializeNullable;

            // if it is an anonymous - get anonymous serializer or
            if (type.IsGenericType &&
                type.Name.StartsWith(_anonymousTypePrefix, StringComparison.Ordinal))
                return SerializeAnonymous;

            // get general object serializer
            return SerializeCustom;
        }

        /// <summary>
        /// Gets the constant value de-serializing delegate corresponding to the <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element which holds serialized constant value.</param>
        /// <returns>The de-serializing delegate corresponding to the <paramref name="element"/>.</returns>
        internal static Func<XElement, Type, object> GetDeserializer(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return _constantDeserializers[element.Name];
        }

        #region Enums serialization
        /// <summary>
        /// Serializes enum values.
        /// </summary>
        /// <param name="enum">The enum value.</param>
        /// <param name="type">The type.</param>
        /// <param name="parent">The parent.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        static void SerializeEnum(
            object @enum,
            Type type,
            XElement parent)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (@enum == null)
                throw new ArgumentNullException(nameof(@enum));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            parent.Add(
                new XElement(
                        XNames.Elements.Enum,
                        new XAttribute(XNames.Attributes.Type, type.AssemblyQualifiedName),
                        @enum.ToString()));
        }

        /// <summary>
        /// Deserializes the enum.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="type">The type.</param>
        /// <returns>The deserialized constant.</returns>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        /// </exception>
        static object DeserializeEnum(XElement element, Type type)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var enumType = GetType(element);

            if (enumType == null)
                throw new SerializationException(
                            $"Don't know how to de-serialize {element.ToString()}");

            try
            {
                return Enum.Parse(enumType, element.Value);
            }
            catch (ArgumentException ex)
            {
                throw new SerializationException(
                            $"Cannot de-serialize {element.Value} to {enumType.FullName} value.", ex);
            }
            catch (OverflowException ex)
            {
                throw new SerializationException(
                            $"Cannot de-serialize {element.Value} to {enumType.FullName} value.", ex);
            }
        }
        #endregion

        #region Nullables serialization
        /// <summary>
        /// Serializes a nullable value.
        /// </summary>
        /// <param name="nullable">The nullable value to be serialized.</param>
        /// <param name="type">The type of the value.</param>
        /// <param name="parent">The parent element where to add the serialized.</param>
        static void SerializeNullable(
            object nullable,
            Type type,
            XElement parent)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (type.GetGenericArguments().Length <= 0)
                throw new ArgumentException("The type must be generic.", nameof(type));

            var typeArgument = type.GetGenericArguments()[0];
            var nullableElement = new XElement(
                                        XNames.Elements.Nullable,
                                        new XAttribute(XNames.Attributes.IsNull, XmlConvert.ToString(nullable == null)),
                                        nullable == null
                                            ? new XAttribute(XNames.Attributes.Type, TypeNameResolver.GetTypeName(typeArgument))
                                            : null);

            parent.Add(nullableElement);

            if (nullable == null)
                return;

            // get the serializer for the type argument from the table or
            if (_constantSerializers.TryGetValue(typeArgument, out var serializer))
                serializer(nullable, typeArgument, nullableElement);
            else
            {
                var valueElement = new XElement(
                                        XNames.Elements.Custom,
                                        new XAttribute(
                                                XNames.Attributes.Type, TypeNameResolver.GetTypeName(type)));

                nullableElement.Add(valueElement);

                // create a data contract serializer (should work with [Serializable] types too)
                var dcSerializer = new DataContractSerializer(type, Type.EmptyTypes);

                using (var writer = valueElement.CreateWriter())
                    dcSerializer.WriteObject(writer, nullable);
            }
        }

        /// <summary>
        /// De-serializes a nullable constant.
        /// </summary>
        /// <param name="element">The element from which to de-serialize.</param>
        /// <param name="type">The type.</param>
        /// <returns>The de-serialized constant.</returns>
        static object DeserializeNullable(
            XElement element,
            Type type)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (XmlConvert.ToBoolean(element.Attribute(XNames.Attributes.IsNull).Value))
                return null;

            var valueType = type.GetGenericArguments()[0];
            var valueNode = element.Elements().First();
            var value = _constantDeserializers[valueNode.Name](valueNode, valueType);

            return type.GetConstructor(new Type[] { valueType })
                       .Invoke(new object[] { value });
        }
        #endregion

        #region Anonymous types serialization
        /// <summary>
        /// Serializes an anonymous object.
        /// </summary>
        /// <param name="anonymous">The anonymous object to be serialized.</param>
        /// <param name="type">The type of the anonymous object.</param>
        /// <param name="parent">The parent element where to serialize the anonymous object to.</param>
        static void SerializeAnonymous(
            object anonymous,
            Type type,
            XElement parent)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var anonymousElement = new XElement(
                                        XNames.Elements.Anonymous,
                                        new XAttribute(XNames.Attributes.Type, type.AssemblyQualifiedName));

            parent.Add(anonymousElement);

            if (anonymous == null)
                return;

            var props = type.GetProperties();

            for (var i = 0; i < props.Length; i++)
            {
                var curElement = new XElement(
                                        XNames.Elements.Property,
                                        new XAttribute(XNames.Attributes.Name, props[i].Name));
                var serializer = GetSerializer(props[i].PropertyType);

                serializer(
                    props[i].GetValue(anonymous, null),
                    props[i].PropertyType,
                    curElement);

                anonymousElement.Add(curElement);
            }
        }

        /// <summary>
        /// De-serializes an anonymous object constant.
        /// </summary>
        /// <param name="element">The element from which to de-serialize the constant.</param>
        /// <param name="type">The type.</param>
        /// <returns>The de-serialized anonymous object constant.</returns>
        static object DeserializeAnonymous(
            XElement element,
            Type type)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (!element.Elements(XNames.Elements.Property).Any())
                return null;

            type = GetType(element);

            var constructor = type.GetConstructors()[0];
            var constructorParameters = constructor.GetParameters();
            var parameters = new object[constructorParameters.Length];

            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var propElement = element
                                    .Elements(XNames.Elements.Property)
                                    .Where(e => e.Attribute(XNames.Attributes.Name)
                                                 .Value == constructorParameters[i].Name)
                                    .First()
                                    .Elements()
                                    .FirstOrDefault();

                if (propElement != null)
                    parameters[i] = GetDeserializer(propElement)(propElement, GetDataType(propElement));
            }

            return constructor.Invoke(parameters);
        }
        #endregion

        #region Custom types (classes and structs) serialization
        /// <summary>
        /// Serializes an object using <see cref="DataContractSerializer"/>.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="type">The type of the object.</param>
        /// <param name="parent">The parent element where to serialize the object to.</param>
        static void SerializeCustom(
            object obj,
            Type type,
            XElement parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var custom = new XElement(
                                XNames.Elements.Custom,
                                new XAttribute(
                                        XNames.Attributes.Type, TypeNameResolver.GetTypeName(type)));

            parent.Add(custom);

            if (obj == null)
                return;

            // create a data contract serializer (works with [Serializable] types too)
            var dcSerializer = new DataContractSerializer(obj.GetType(), Type.EmptyTypes);

            using (var writer = custom.CreateWriter())
                dcSerializer.WriteObject(writer, obj);
        }

        /// <summary>
        /// De-serializes an object constant.
        /// </summary>
        /// <param name="element">The element from which to de-serialize the constant.</param>
        /// <param name="type">The type.</param>
        /// <returns>The de-serialized object constant.</returns>
        /// <exception cref="System.Runtime.Serialization.SerializationException">Expected type attribute.</exception>
        static object DeserializeCustom(
            XElement element,
            Type type)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (element.Elements().FirstOrDefault() == null)
                return null;

            string typeString;
            var typeAttribute = element.Attribute(XNames.Attributes.Type);

            if (typeAttribute == null)
                throw new SerializationException("Expected type attribute.");

            typeString = typeAttribute.Value;

            if (typeString.StartsWith(_anonymousTypePrefix, StringComparison.Ordinal))
                return DeserializeAnonymous(element, type);

            var serializer = new DataContractSerializer(TypeNameResolver.GetType(typeString));

            using (var reader = element.Elements().First().CreateReader())
                return serializer.ReadObject(reader);
        }
        #endregion

        /// <summary>
        /// Gets the type corresponding to the attribute @type in the given element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The specified type.</returns>
        internal static Type GetType(XElement element)
        {
            if (element == null)
                return null;

            return GetType(element.Attribute(XNames.Attributes.Type));
        }

        /// <summary>
        /// Gets the type corresponding to a type name written in an XML attribute.
        /// </summary>
        /// <param name="typeAttribute">The type attribute.</param>
        /// <returns>The specified type.</returns>
        internal static Type GetType(XAttribute typeAttribute)
        {
            if (typeAttribute == null)
                return null;

            return TypeNameResolver.GetType(typeAttribute.Value);
        }

        internal static Type GetDataType(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var type = TypeNameResolver.GetType(element.Name.LocalName);

            if (type == null && element.Name==XNames.Elements.Anonymous)
            {
                type = DataSerialization.GetType(element);

                if (type == null)
                    throw new SerializationException("Expected constant element's type.");
            }

            if (type == typeof(Nullable<>))
            {
                Type valueType = GetType(element);

                if (valueType != null)
                    return typeof(Nullable<>).MakeGenericType(valueType);

                var valueElement = element.Elements().FirstOrDefault();

                if (valueElement == null)
                    return null;

                valueType = GetDataType(valueElement);

                if (valueType == null)
                    return null;

                return typeof(Nullable<>).MakeGenericType(valueType);
            }

            if (type == typeof(object))
                return GetCustomConstantType(element);

            if (type == typeof(Enum))
                return GetEnumConstantType(element);

            return type;
        }

        static Type GetCustomConstantType(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var typeAttribute = element.Attribute(XNames.Attributes.Type);

            if (typeAttribute == null)
                throw new SerializationException("Expected type attribute.");

            return TypeNameResolver.GetType(typeAttribute.Value);
        }

        static Type GetEnumConstantType(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var typeAttribute = element.Attribute(XNames.Attributes.Type);

            if (typeAttribute == null)
                throw new SerializationException("Expected type attribute.");

            return TypeNameResolver.GetType(typeAttribute.Value);
        }
    }
}
