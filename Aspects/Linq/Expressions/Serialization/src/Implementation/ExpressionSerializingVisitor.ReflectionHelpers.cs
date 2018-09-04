using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    static class XElementExtensions
    {
        /// <summary>
        /// Adds the type attribute.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="defaultType">The default type.</param>
        /// <returns>XElement.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// element
        /// or
        /// expression
        /// </exception>
        public static XElement AddTypeAttribute(
            this XElement element,
            Expression expression,
            Type defaultType = null)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression.Type != defaultType)
                element.Add(
                    new XAttribute(
                        XNames.Attributes.Type,
                        TypeNameResolver.GetTypeName(expression.Type)));

            return element;
        }
    }

    partial class ExpressionSerializingVisitor
    {
        /// <summary>
        /// If the unary expression has type conversion component, this method creates an &quot;asType&quot; attribute.
        /// </summary>
        /// <param name="node">The unary node.</param>
        /// <returns>An &quot;asType&quot; attribute or <see langword="null"/> if <paramref name="node"/> does not contain type conversion component.</returns>
        static XAttribute VisitAsType(UnaryExpression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.NodeType != ExpressionType.TypeAs && node.NodeType != ExpressionType.Convert)
                return null;

            return new XAttribute(XNames.Attributes.Type, TypeNameResolver.GetTypeName(node.Type));
        }

        #region Parameters and arguments
        /// <summary>
        /// Creates a sequence of XML elements for each of the <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A sequence of elements.</returns>
        static IEnumerable<XElement> VisitParameters(IEnumerable<ParameterInfo> parameters)
        {
            if (parameters == null)
                yield break;

            foreach (var param in parameters)
                yield return new XElement(
                                    XNames.Elements.Parameter,
                                    new XAttribute(XNames.Attributes.Type, TypeNameResolver.GetTypeName(param.ParameterType)),
                                    new XAttribute(XNames.Attributes.Name, param.Name),
                                    param.IsOut || param.ParameterType.IsByRef
                                        ? new XAttribute(XNames.Attributes.IsByRef, true)
                                        : null);
        }

        IEnumerable<XElement> PopExpressions(int numberOfExpressions)
        {
            var stack = new Stack<XElement>();

            // pop the expressions:
            for (var i = 0; i < numberOfExpressions; i++)
                stack.Push(_elements.Pop());

            return stack;
        }

        IEnumerable<XElement> PopParameters(IEnumerable<ParameterExpression> parameters)
            => parameters!=null
                    ? PopExpressions(parameters.Count())
                    : throw new ArgumentNullException(nameof(parameters));

        IEnumerable<XElement> PopArguments(IEnumerable<Expression> arguments)
            => arguments!=null
                    ? PopExpressions(arguments.Count())
                    : throw new ArgumentNullException(nameof(arguments));

        XElement AddParameters(IEnumerable<ParameterExpression> parameters, XElement node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            node.Add(PopParameters(parameters));
            return node;
        }

        XElement AddArguments(IEnumerable<Expression> arguments, XElement node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            node.Add(PopArguments(arguments));
            return node;
        }

        static XElement ReplaceParametersWithReferences(XElement parameters, XElement body)
        {
            if (parameters == null)
                return body;

            var varRefs = from p in parameters.Elements(XNames.Elements.Parameter)
                          from a in body.Descendants(XNames.Elements.Parameter)
                          let pName = p.Attribute(XNames.Attributes.Name).Value
                          let aa = a.Attribute(XNames.Attributes.Name)
                          let aName = aa?.Value
                          where aName == pName
                          select a;

            // ... with references to parameters (parameter-s without Type attribute)
            foreach (var a in varRefs.ToArray())
            {
                a.AddAfterSelf(
                    new XElement(
                        XNames.Elements.Parameter,
                        new XAttribute(
                            XNames.Attributes.Name,
                            a.Attribute(XNames.Attributes.Name).Value)));

                a.Remove();
            }

            return body;
        }

        static XElement ReplaceParameterWithReference(XElement parameter, XElement body)
        {
            if (parameter != null && body == null)
                throw new ArgumentNullException(nameof(body));

            if (parameter == null)
                return body;

            var pName = parameter.Attribute(XNames.Attributes.Name).Value;

            // replace all parameters in the body...
            var varRefs = from p in body.Descendants(XNames.Elements.Parameter)
                          let aa = p.Attribute(XNames.Attributes.Name)
                          let aName = aa?.Value
                          where aName == pName
                          select p;

            // ... with references to the parameter (parameter without Type attribute)
            foreach (var p in varRefs.ToArray())
            {
                p.AddAfterSelf(new XElement(
                                        XNames.Elements.Parameter,
                                        new XAttribute(
                                                XNames.Attributes.Name,
                                                pName)));
                p.Remove();
            }

            return body;
        }
        #endregion

        #region Visit MemberInfo-s
        /// <summary>
        /// If the binary expression has overloading method, creates an XML &quot;method&quot; element.
        /// </summary>
        /// <param name="node">The binary node.</param>
        /// <returns>The created method or <see langword="null"/> if there is no overloading method.</returns>
        static XElement VisitMethodInfo(BinaryExpression node)
            => node!=null
                    ? VisitMethodInfo(node.Method)
                    : throw new ArgumentNullException(nameof(node));

        /// <summary>
        /// If the unary expression has overloading method, creates an XML &quot;method&quot; element.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The created method or <see langword="null"/> if there is no overloading method.</returns>
        static XElement VisitMethodInfo(UnaryExpression node)
            => node!=null
                    ? VisitMethodInfo(node.Method)
                    : throw new ArgumentNullException(nameof(node));

        /// <summary>
        /// Creates an XML element out of <paramref name="memberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>An XML element or <see langword="null"/> if <paramref name="memberInfo"/> is <see langword="null"/>.</returns>
        static XElement VisitMemberInfo(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo p:
                    return VisitPropertyInfo(p);

                case ConstructorInfo c:
                    return VisitConstructorInfo(c);

                case MethodInfo m:
                    return VisitMemberInfo(m);

                case FieldInfo f:
                    return VisitFieldInfo(f);

                case EventInfo e:
                    return VisitEventInfo(e);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates an XML element out of <paramref name="methodInfo"/>.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>An XML element or <see langword="null"/> if <paramref name="methodInfo"/> is <see langword="null"/>.</returns>
        static XElement VisitMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return null;

            XAttribute visibility = null;

            if (!methodInfo.IsPublic)
            {
                if (methodInfo.IsPrivate)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Private);
                else
                if (methodInfo.IsAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Assembly);
                else
                if (methodInfo.IsFamily)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Family);
                else
                if (methodInfo.IsFamilyAndAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.FamilyAndAssembly);
                else
                if (methodInfo.IsFamilyOrAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.FamilyOrAssembly);
            }

            return new XElement(
                    XNames.Elements.Method,
                    new XAttribute(XNames.Attributes.Type, TypeNameResolver.GetTypeName(methodInfo.DeclaringType)),
                    visibility,
                    methodInfo.IsStatic ? new XAttribute(XNames.Attributes.Static, true) : null,
                    new XAttribute(XNames.Attributes.Name, methodInfo.Name),
                    new XElement(
                            XNames.Elements.Parameters,
                            VisitParameters(methodInfo.GetParameters())));
        }

        /// <summary>
        /// Creates an XML element out of <paramref name="constructorInfo"/>.
        /// </summary>
        /// <param name="constructorInfo">The method info.</param>
        /// <returns>An XML element or <see langword="null"/> if <paramref name="constructorInfo"/> is <see langword="null"/>.</returns>
        static XElement VisitConstructorInfo(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
                return null;

            XAttribute visibility = null;

            if (!constructorInfo.IsPublic)
            {
                if (constructorInfo.IsPrivate)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Private);
                else
                if (constructorInfo.IsAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Assembly);
                else
                if (constructorInfo.IsFamily)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Family);
                else
                if (constructorInfo.IsFamilyAndAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.FamilyAndAssembly);
                else
                if (constructorInfo.IsFamilyOrAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.FamilyOrAssembly);
            }

            return new XElement(
                    XNames.Elements.Constructor,
                    new XAttribute(XNames.Attributes.Type, TypeNameResolver.GetTypeName(constructorInfo.DeclaringType)),
                    visibility,
                    constructorInfo.IsStatic ? new XAttribute(XNames.Attributes.Static, true) : null,
                    new XElement(
                            XNames.Elements.Parameters,
                            VisitParameters(constructorInfo.GetParameters())));
        }

        /// <summary>
        /// Creates an XML element out of <paramref name="eventInfo"/>.
        /// </summary>
        /// <param name="eventInfo">The property info.</param>
        /// <returns>An XML element or <see langword="null"/> if <paramref name="eventInfo"/> is <see langword="null"/>.</returns>
        static XElement VisitEventInfo(EventInfo eventInfo)
            => eventInfo != null
                ? new XElement(
                    XNames.Elements.Property,
                    new XAttribute(XNames.Attributes.Type, TypeNameResolver.GetTypeName(eventInfo.DeclaringType)),
                    new XAttribute(XNames.Attributes.Name, eventInfo.Name))
                : null;

        /// <summary>
        /// Creates an XML element out of <paramref name="propertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>An XML element or <see langword="null"/> if <paramref name="propertyInfo"/> is <see langword="null"/>.</returns>
        static XElement VisitPropertyInfo(PropertyInfo propertyInfo)
            => propertyInfo!=null
                    ? new XElement(
                            XNames.Elements.Property,
                            new XAttribute(XNames.Attributes.Type, TypeNameResolver.GetTypeName(propertyInfo.DeclaringType)),
                            new XAttribute(XNames.Attributes.Name, propertyInfo.Name),
                            VisitParameters(propertyInfo.GetIndexParameters()))
                    : null;

        /// <summary>
        /// Creates an XML element out of <paramref name="fieldInfo"/>.
        /// </summary>
        /// <param name="fieldInfo">The property info.</param>
        /// <returns>An XML element or <see langword="null"/> if <paramref name="fieldInfo"/> is <see langword="null"/>.</returns>
        static XElement VisitFieldInfo(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return null;

            XAttribute visibility = null;

            if (!fieldInfo.IsPublic)
            {
                if (fieldInfo.IsAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Assembly);
                else
                if (fieldInfo.IsFamily)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Family);
                else
                if (fieldInfo.IsPrivate)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.Private);
                else
                if (fieldInfo.IsFamilyAndAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.FamilyAndAssembly);
                else
                if (fieldInfo.IsFamilyOrAssembly)
                    visibility = new XAttribute(XNames.Attributes.Visibility, XNames.Attributes.FamilyOrAssembly);
            }

            return new XElement(
                    XNames.Elements.Field,
                    new XAttribute(XNames.Attributes.Type, TypeNameResolver.GetTypeName(fieldInfo.DeclaringType)),
                    visibility,
                    fieldInfo.IsStatic ? new XAttribute(XNames.Attributes.Static, true) : null,
                    new XAttribute(XNames.Attributes.Name, fieldInfo.Name));
        }
        #endregion
    }
}
