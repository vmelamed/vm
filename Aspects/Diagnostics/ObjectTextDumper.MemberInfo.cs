using System;
using System.Reflection;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    sealed partial class ObjectTextDumper
    {
        bool DumpedMemberInfoValue(object value)
        {
            var memberInfo = value as MemberInfo;

            if (memberInfo == null)
                return false;

            _writer.Write(
                DumpFormat.MemberInfoMemberType,
                memberInfo.MemberType.ToString());

            if (!DumpedType(value as Type) &&
                !DumpedMethodInfo(value as MethodInfo) &&
                !DumpedPropertyInfo(value as PropertyInfo) &&
                !DumpedFieldInfo(value as FieldInfo) &&
                !DumpedEventInfo(value as EventInfo))

                _writer.Write(memberInfo.Name);

            return true;
        }

        private bool DumpedType(Type type)
        {
            if (type == null)
                return false;

            _writer.Write(
                DumpFormat.TypeInfo,
                type.Name,
                type.Namespace,
                type.AssemblyQualifiedName);
            return true;
        }

        bool DumpedEventInfo(EventInfo eventInfo)
        {
            if (eventInfo == null)
                return false;

            _writer.Write(
                DumpFormat.MethodInfo,
                eventInfo.EventHandlerType.Name,
                eventInfo.EventHandlerType.Namespace,
                eventInfo.EventHandlerType.AssemblyQualifiedName,
                eventInfo.DeclaringType.Name,
                eventInfo.DeclaringType.Namespace,
                eventInfo.DeclaringType.AssemblyQualifiedName,
                eventInfo.Name);
            return true;
        }

        bool DumpedFieldInfo(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return false;

            _writer.Write(
                DumpFormat.MethodInfo,
                fieldInfo.FieldType.Name,
                fieldInfo.FieldType.Namespace,
                fieldInfo.FieldType.AssemblyQualifiedName,
                fieldInfo.DeclaringType.Name,
                fieldInfo.DeclaringType.Namespace,
                fieldInfo.DeclaringType.AssemblyQualifiedName,
                fieldInfo.Name);

            return true;
        }

        bool DumpedPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return false;

            var indexes = propertyInfo.GetIndexParameters();

            _writer.Write(
                DumpFormat.MethodInfo,
                propertyInfo.PropertyType.Name,
                propertyInfo.PropertyType.Namespace,
                propertyInfo.PropertyType.AssemblyQualifiedName,
                propertyInfo.DeclaringType.Name,
                propertyInfo.DeclaringType.Namespace,
                propertyInfo.DeclaringType.AssemblyQualifiedName,
                indexes.Length == 0
                    ? propertyInfo.Name
                    : Resources.IndexerStart);

            if (indexes.Length > 0)
            {
                for (var i=0; i<indexes.Length; i++)
                {
                    if (i > 0)
                        _writer.Write(Resources.ParametersSeparator);
                    _writer.Write(
                        DumpFormat.IndexerIndexType,
                        indexes[i].ParameterType.Name,
                        indexes[i].ParameterType.Namespace,
                        indexes[i].ParameterType.AssemblyQualifiedName);
                }
                _writer.Write(Resources.IndexerEnd);
            }

            _writer.Write(Resources.PropertyBegin);
            if (propertyInfo.CanRead)
                _writer.Write(Resources.PropertyGetter);
            if (propertyInfo.CanWrite)
                _writer.Write(Resources.PropertySetter);
            _writer.Write(Resources.PropertyEnd);

            return true;
        }

        bool DumpedMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return false;

            _writer.Write(
                DumpFormat.MethodInfo,
                methodInfo.ReturnType.Name,
                methodInfo.ReturnType.Namespace,
                methodInfo.ReturnType.AssemblyQualifiedName,
                methodInfo.DeclaringType.Name,
                methodInfo.DeclaringType.Namespace,
                methodInfo.DeclaringType.AssemblyQualifiedName,
                methodInfo.Name);

            if (methodInfo.ContainsGenericParameters)
            {
                var genericParameters = methodInfo.GetGenericArguments();

                _writer.Write(Resources.GenericParamListBegin);

                for (var i=0; i<genericParameters.Length; i++)
                {
                    if (i > 0)
                        _writer.Write(Resources.ParametersSeparator);
                    _writer.Write(genericParameters[i].Name);
                }

                _writer.Write(Resources.GenericParamListEnd);
            }

            var parameters = methodInfo.GetParameters();

            _writer.Write(Resources.MethodParameterListBegin);

            for (var i=0; i<parameters.Length; i++)
            {
                if (i > 0)
                    _writer.Write(Resources.ParametersSeparator);
                _writer.Write(
                    DumpFormat.MethodParameter,
                    parameters[i].ParameterType.Name,
                    parameters[i].ParameterType.Namespace,
                    parameters[i].ParameterType.AssemblyQualifiedName,
                    parameters[i].Name);
            }

            _writer.Write(Resources.MethodParameterListEnd);

            return true;
        }
    }
}
