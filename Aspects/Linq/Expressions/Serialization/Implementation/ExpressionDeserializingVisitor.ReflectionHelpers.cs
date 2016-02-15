using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    partial class ExpressionDeserializingVisitor
    {
        static string GetName(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var nameAttribute = element.Attribute(XNames.Attributes.Name);

            return nameAttribute != null
                        ? nameAttribute.Value
                        : null;
        }

        static Type ConvertTo(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return DataSerialization.GetType(element.Attribute(XNames.Attributes.Type));
        }

        static IEnumerable<MemberInfo> GetMembers(Type type, XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var members = element.Elements(XNames.Elements.Members).FirstOrDefault();

            if (members == null)
                return new MemberInfo[0];

            return members
                    .Elements()
                    .Select(e => GetMemberInfo(type, e))
                    .ToList();
        }

        static ConstructorInfo GetConstructorInfo(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var constructor = element.Elements(XNames.Elements.Constructor).FirstOrDefault();

            if (constructor == null)
                return null;

            return GetConstructorInfo(
                        DataSerialization.GetType(constructor),
                        constructor);
        }

        static MethodInfo GetMethodInfo(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var method = element.Elements(XNames.Elements.Method).FirstOrDefault();

            if (method == null)
                return null;

            return GetMethodInfo(
                        DataSerialization.GetType(method),
                        method);
        }

        static MemberInfo GetMemberInfo(XElement element)
        {
            if (element == null)
                return null;

            var type = DataSerialization.GetType(element);
            Func<Type, XElement, MemberInfo> getMemberInfo;

            if (!_memberInfoDeserializers.TryGetValue(element.Name, out getMemberInfo))
                throw new SerializationException("Expected a member info type of element.");

            return getMemberInfo(type, element);
        }

        static MemberInfo GetMemberInfo(Type type, XElement element)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (element == null)
                return null;

            Func<Type, XElement, MemberInfo> getMemberInfo;

            if (!_memberInfoDeserializers.TryGetValue(element.Name, out getMemberInfo))
                throw new SerializationException("Expected a member info type of element.");

            return getMemberInfo(type, element);
        }

        static IDictionary<XName, Func<Type, XElement, MemberInfo>> _memberInfoDeserializers = new Dictionary<XName, Func<Type, XElement, MemberInfo>>
        {
            { XNames.Elements.Property,    GetPropertyInfo },
            { XNames.Elements.Method,      GetMethodInfo },
            { XNames.Elements.Field,       GetFieldInfo },
            { XNames.Elements.Event,       GetEventInfo },
            { XNames.Elements.Constructor, GetConstructorInfo },
        };

        static PropertyInfo GetPropertyInfo(Type type, XElement element)
        {
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));

            return type.GetProperty(
                            GetName(element),
                            GetBindingFlags(element));
        }

        static MethodInfo GetMethodInfo(Type type, XElement element)
        {
            Contract.Requires<ArgumentException>(element == null || type != null);

            return element == null
                        ? null
                        : type.GetMethod(
                                    GetName(element),
                                    element.Element(XNames.Elements.Parameters)
                                           .Elements(XNames.Elements.Parameter)
                                           .Select(p => DataSerialization.GetType(p))
                                           .ToArray());
        }

        static FieldInfo GetFieldInfo(Type type, XElement element)
        {
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));

            return type.GetField(
                            GetName(element),
                            GetBindingFlags(element));
        }

        static EventInfo GetEventInfo(Type type, XElement element)
        {
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));

            return type.GetEvent(
                            GetName(element),
                            GetBindingFlags(element));
        }

        static ConstructorInfo GetConstructorInfo(Type type, XElement element)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (element == null)
                return null;

            return type.GetConstructor(
                            element.Element(XNames.Elements.Parameters)
                                   .Elements()
                                   .Select(p => DataSerialization.GetType(p))
                                   .ToArray());
        }

        static BindingFlags GetBindingFlags(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var visibility = element.Attribute(XNames.Attributes.Visibility);

            BindingFlags flags = visibility == null
                                    ? BindingFlags.Public
                                    : visibility.Value == "public"
                                            ? BindingFlags.Public
                                            : BindingFlags.NonPublic;

            var stat = element.Attribute(XNames.Attributes.Static);

            flags |= stat == null
                        ? BindingFlags.Instance
                        : XmlConvert.ToBoolean(stat.Value)
                                            ? BindingFlags.Static
                                            : BindingFlags.Instance;

            return flags;
        }
    }
}
