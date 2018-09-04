using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    partial class ExpressionDeserializingVisitor
    {
        static string GetName(XElement e)
            => e!=null
                    ? e.Attribute(XNames.Attributes.Name)?.Value
                    : throw new ArgumentNullException(nameof(e));

        static Type ConvertTo(XElement e)
            => e!=null
                    ? DataSerialization.GetType(e.Attribute(XNames.Attributes.Type))
                    : throw new ArgumentNullException(nameof(e));

        static IEnumerable<MemberInfo> GetMembers(Type type, XElement e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var members = e.Elements(XNames.Elements.Members).FirstOrDefault();

            if (members == null)
                return new MemberInfo[0];

            return members
                    .Elements()
                    .Select(x => GetMemberInfo(type, x))
                    .ToList();
        }

        static ConstructorInfo GetConstructorInfo(XElement e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var constructor = e.Elements(XNames.Elements.Constructor).FirstOrDefault();

            if (constructor == null)
                return null;

            return GetConstructorInfo(
                        DataSerialization.GetType(constructor),
                        constructor);
        }

        static MethodInfo GetMethodInfo(XElement e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var method = e.Elements(XNames.Elements.Method).FirstOrDefault();

            if (method == null)
                return null;

            return GetMethodInfo(
                        DataSerialization.GetType(method),
                        method);
        }

        static MemberInfo GetMemberInfo(XElement e)
        {
            if (e == null)
                return null;

            var type = DataSerialization.GetType(e);

            if (!_memberInfoDeserializers.TryGetValue(e.Name, out var getMemberInfo))
                throw new SerializationException("Expected a member info type of element.");

            return getMemberInfo(type, e);
        }

        static MemberInfo GetMemberInfo(Type type, XElement e)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (e == null)
                return null;


            if (!_memberInfoDeserializers.TryGetValue(e.Name, out var getMemberInfo))
                throw new SerializationException("Expected a member info type of element.");

            return getMemberInfo(type, e);
        }

        static IDictionary<XName, Func<Type, XElement, MemberInfo>> _memberInfoDeserializers = new Dictionary<XName, Func<Type, XElement, MemberInfo>>
        {
            { XNames.Elements.Property,    GetPropertyInfo },
            { XNames.Elements.Method,      GetMethodInfo },
            { XNames.Elements.Field,       GetFieldInfo },
            { XNames.Elements.Event,       GetEventInfo },
            { XNames.Elements.Constructor, GetConstructorInfo },
        };

        static PropertyInfo GetPropertyInfo(Type type, XElement e)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetProperty(
                            GetName(e),
                            GetBindingFlags(e));
        }

        static MethodInfo GetMethodInfo(Type type, XElement e)
        {
            if (e != null && type == null)
                throw new ArgumentNullException(nameof(type));

            return e == null
                        ? null
                        : type.GetMethod(
                                    GetName(e),
                                    e.Element(XNames.Elements.Parameters)
                                           .Elements(XNames.Elements.Parameter)
                                           .Select(p => DataSerialization.GetType(p))
                                           .ToArray());
        }

        static FieldInfo GetFieldInfo(Type type, XElement e)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetField(
                            GetName(e),
                            GetBindingFlags(e));
        }

        static EventInfo GetEventInfo(Type type, XElement e)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetEvent(
                            GetName(e),
                            GetBindingFlags(e));
        }

        static ConstructorInfo GetConstructorInfo(Type type, XElement e)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (e == null)
                return null;

            return type.GetConstructor(
                            e.Element(XNames.Elements.Parameters)
                                   .Elements()
                                   .Select(p => DataSerialization.GetType(p))
                                   .ToArray());
        }

        static BindingFlags GetBindingFlags(XElement e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var visibility = e.Attribute(XNames.Attributes.Visibility);

            BindingFlags flags = visibility == null
                                    ? BindingFlags.Public
                                    : visibility.Value == "public"
                                            ? BindingFlags.Public
                                            : BindingFlags.NonPublic;

            var stat = e.Attribute(XNames.Attributes.Static);

            flags |= stat == null
                        ? BindingFlags.Instance
                        : XmlConvert.ToBoolean(stat.Value)
                                            ? BindingFlags.Static
                                            : BindingFlags.Instance;

            return flags;
        }
    }
}
