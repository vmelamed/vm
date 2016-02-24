using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    partial class ExpressionDeserializingVisitor
    {
        /// <summary>
        /// Holds the parameters of the lambda expressions that are referred to inside the lambda expressions' bodies.
        /// </summary>
        readonly IDictionary<string, ParameterExpression> _references = new SortedDictionary<string, ParameterExpression>();

        #region Expression XML deserializing visitors
        /// <summary>
        /// Holds dictionary of expression element name - delegate to the respective de-serializer.
        /// </summary>
        static readonly IDictionary<XName, Func<ExpressionDeserializingVisitor, XElement, Expression>> _deserializers = new Dictionary<XName, Func<ExpressionDeserializingVisitor, XElement, Expression>>
        {
            { XNames.Elements.Constant,             (v, e) => ExpressionDeserializingVisitor.VisitConstant(e) },
            { XNames.Elements.Parameter,            (v, e) => v.VisitParameter(e)           },
            { XNames.Elements.Lambda,               (v, e) => v.VisitLambda(e)              },
            // unary
            { XNames.Elements.ArrayLength,          (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.Convert,              (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.ConvertChecked,       (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.Negate,               (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.NegateChecked,        (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.Not,                  (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.OnesComplement,       (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.Quote,                (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.TypeAs,               (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.UnaryPlus,            (v, e) => v.VisitUnary(e)               },
            // change by one
            { XNames.Elements.Decrement,            (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.Increment,            (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.PostDecrementAssign,  (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.PostIncrementAssign,  (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.PreDecrementAssign,   (v, e) => v.VisitUnary(e)               },
            { XNames.Elements.PreIncrementAssign,   (v, e) => v.VisitUnary(e)               },
            // binary
            { XNames.Elements.Add,                  (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.AddChecked,           (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.And,                  (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.AndAlso,              (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Coalesce,             (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Divide,               (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Equal,                (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.ExclusiveOr,          (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.GreaterThan,          (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.GreaterThanOrEqual,   (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.LeftShift,            (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.LessThan,             (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.LessThanOrEqual,      (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Modulo,               (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Multiply,             (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.MultiplyChecked,      (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.NotEqual,             (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Or,                   (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.OrElse,               (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Power,                (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.RightShift,           (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Subtract,             (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.SubtractChecked,      (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.ArrayIndex,           (v, e) => v.VisitBinary(e)              },
            // assignments
            { XNames.Elements.AddAssign,            (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.AddAssignChecked,     (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.AndAssign,            (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.Assign,               (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.DivideAssign,         (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.LeftShiftAssign,      (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.ModuloAssign,         (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.MultiplyAssign,       (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.MultiplyAssignChecked,(v, e) => v.VisitBinary(e)              },
            { XNames.Elements.OrAssign,             (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.PowerAssign,          (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.RightShiftAssign,     (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.SubtractAssign,       (v, e) => v.VisitBinary(e)              },
            { XNames.Elements.SubtractAssignChecked,(v, e) => v.VisitBinary(e)              },
            { XNames.Elements.ExclusiveOrAssign,    (v, e) => v.VisitBinary(e)              },

            { XNames.Elements.TypeIs,               (v, e) => v.VisitTypeBinary(e)          },
            { XNames.Elements.Block,                (v, e) => v.VisitBlock(e)               },
            { XNames.Elements.Conditional,          (v, e) => v.VisitConditional(e)         },
            { XNames.Elements.Index,                (v, e) => v.VisitIndex(e)               },
            { XNames.Elements.New,                  (v, e) => v.VisitNew(e)                 },
            { XNames.Elements.Throw,                (v, e) => v.VisitThrow(e)               },
            { XNames.Elements.Default,              (v, e) => ExpressionDeserializingVisitor.VisitDefault(e) },
            { XNames.Elements.Extension,            (v, e) => ExpressionDeserializingVisitor.VisitDefault(e) },
            { XNames.Elements.MemberAccess,         (v, e) => v.VisitMember(e)              },
            { XNames.Elements.Call,                 (v, e) => v.VisitMethodCall(e)          },
            { XNames.Elements.Exception,            (v, e) => v.VisitParameter(e)           },
            { XNames.Elements.Label,                (v, e) => v.VisitLabel(e)               },
            { XNames.Elements.Goto,                 (v, e) => v.VisitGoto(e)                },
            { XNames.Elements.Loop,                 (v, e) => v.VisitLoop(e)                },
            { XNames.Elements.Switch,               (v, e) => v.VisitSwitch(e)              },
            { XNames.Elements.Try,                  (v, e) => v.VisitTry(e)                 },
            { XNames.Elements.Filter,               (v, e) => v.VisitExpressionContainer(e) },
            { XNames.Elements.Fault,                (v, e) => v.VisitExpressionContainer(e) },
            { XNames.Elements.Finally,              (v, e) => v.VisitExpressionContainer(e) },
            { XNames.Elements.ListInit,             (v, e) => v.VisitListInit(e)            },
            { XNames.Elements.NewArrayInit,         (v, e) => v.VisitNewArrayInit(e)        },
            { XNames.Elements.NewArrayBounds,       (v, e) => v.VisitNewArrayBounds(e)      },
            { XNames.Elements.MemberInit,           (v, e) => v.VisitMemberInit(e)          },
            { XNames.Elements.RuntimeVariables,     (v, e) => v.VisitRuntimeVariables(e)    },
        };
        #endregion

        /// <summary>
        /// This is the starting point of the visitor.
        /// </summary>
        /// <param name="element">The element to be visited.</param>
        /// <returns>The created expression.</returns>
        public Expression Visit(XElement element)
        {
            if (element == null)
                return null;

            return _deserializers[element.Name](this, element);
        }

        static ExpressionType GetExpressionType(
            XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return (ExpressionType)Enum.Parse(
                                        typeof(ExpressionType),
                                        element.Name.LocalName,
                                        true);
        }

        static ConstantExpression VisitConstant(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var constantElement = element.Elements().FirstOrDefault();

            if (constantElement == null)
                throw new SerializationException("Expected constant element's contents.");

            var type = DataSerialization.GetDataType(constantElement);
            var value = DataSerialization.GetDeserializer(constantElement)(constantElement, type);

            return Expression.Constant(value, type);
        }

        static Expression VisitDefault(XElement element) => Expression.Default(DataSerialization.GetType(element));

        ParameterExpression VisitParameter(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var name = GetName(element);
            var type = DataSerialization.GetType(element);
            ParameterExpression parameter;

            if (type == null &&
                _references.TryGetValue(name, out parameter))
                return parameter;

            parameter = Expression.Parameter(type, name);

            ParameterExpression reference;

            if (_references.TryGetValue(name, out reference) &&
                reference.CanReduce == parameter.CanReduce   &&
                reference.IsByRef   == parameter.IsByRef     &&
                reference.NodeType  == parameter.NodeType)
                return reference;

            _references[name] = parameter;

            return parameter;
        }

        IEnumerable<ParameterExpression> VisitParameters(XElement element)
        {
            if (element == null)
                return new ParameterExpression[0];

            return element.Elements(XNames.Elements.Parameter)
                                    .Select(p => VisitParameter(p))
                                    .ToList();
        }

        IEnumerable<Expression> VisitArguments(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var arguments = element.Elements(XNames.Elements.Arguments).FirstOrDefault();

            if (arguments == null)
                return new Expression[0];

            return VisitExpressions(arguments.Elements());
        }

        IEnumerable<Expression> VisitExpressions(XElement element)
        {
            if (element == null)
                return new Expression[0];

            return VisitExpressions(element.Elements());
        }

        IEnumerable<Expression> VisitExpressions(IEnumerable<XElement> elements)
        {
            if (elements == null)
                return new Expression[0];

            return elements.Select(e => Visit(e));
        }

        LambdaExpression VisitLambda(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var parameters = VisitParameters(element.Element(XNames.Elements.Parameters));
            var tailCallAttribute = element.Attribute(XNames.Attributes.TailCall);
            var delegateType = DataSerialization.GetType(element.Attribute(XNames.Attributes.DelegateType));
            var name = GetName(element);

            return delegateType != null
                ? Expression.Lambda(
                            delegateType,
                            Visit(element.Element(XNames.Elements.Body)
                                         .Elements()
                                         .First()),
                            name,
                            tailCallAttribute != null
                                    ? XmlConvert.ToBoolean(tailCallAttribute.Value)
                                    : false,
                            parameters)
                : Expression.Lambda(
                            Visit(element.Element(XNames.Elements.Body)
                                         .Elements()
                                         .First()),
                            name,
                            tailCallAttribute != null
                                        ? XmlConvert.ToBoolean(tailCallAttribute.Value)
                                        : false,
                            parameters);
        }

        UnaryExpression VisitUnary(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return Expression.MakeUnary(
                        GetExpressionType(element),
                        Visit(element.Elements().First()),
                        ConvertTo(element),
                        GetMethodInfo(element));
        }

        BinaryExpression VisitBinary(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var liftedAttribute = element.Attribute(XNames.Attributes.IsLiftedToNull);

            return Expression.MakeBinary(
                        GetExpressionType(element),
                        Visit(element.Elements().ElementAt(0)),
                        Visit(element.Elements().ElementAt(1)),
                        liftedAttribute!=null ? XmlConvert.ToBoolean(liftedAttribute.Value) : false,
                        GetMethodInfo(element));
        }

        TypeBinaryExpression VisitTypeBinary(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return Expression.TypeIs(
                        Visit(element.Elements().First()),
                        DataSerialization.GetType(element));
        }

        BlockExpression VisitBlock(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var variablesElement = element.Element(XNames.Elements.Variables);
            var variables = VisitParameters(variablesElement);

            try
            {
                return Expression.Block(
                            DataSerialization.GetType(element) ?? typeof(void),
                            variables,
                            VisitExpressions(element.Elements().Skip(variablesElement!=null ? 1 : 0)));
            }
            finally
            {
                foreach (var v in variables)
                    _references.Remove(v.Name);
            }
        }

        ConditionalExpression VisitConditional(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return Expression.Condition(
                         Visit(element.Elements().ElementAt(0)),
                         Visit(element.Elements().ElementAt(1)),
                         Visit(element.Elements().ElementAt(2)),
                         DataSerialization.GetType(element) ?? typeof(void));
        }

        MemberExpression VisitMember(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var memberElement = element.Element(XNames.Elements.Property);

            if (memberElement == null)
                memberElement = element.Element(XNames.Elements.Field);

            return Expression.MakeMemberAccess(
                                Visit(element.Elements().First()),
                                GetMemberInfo(
                                    DataSerialization.GetType(memberElement),
                                    memberElement));
        }

        IndexExpression VisitIndex(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return Expression.ArrayAccess(
                                Visit(element.Elements().First()),
                                VisitExpressions(element.Element(XNames.Elements.Indexes)));
        }

        Expression VisitMethodCall(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var methodElement = element.Elements().First();
            var mi = GetMethodInfo(element);

            if (mi == null)
                throw new SerializationException("Expected method info's contents.");

            if (methodElement.Name == XNames.Elements.Method)
                return Expression.Call(mi, VisitArguments(element));
            else
                return Expression.Call(Visit(methodElement), mi, VisitArguments(element));
        }

        UnaryExpression VisitThrow(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return Expression.Throw(
                        Visit(element.Elements().First()));
        }

        NewExpression VisitNew(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var constructor = GetConstructorInfo(element);
            var arguments = VisitArguments(element);
            var members = GetMembers(constructor.DeclaringType, element);

            if (!members.Any())
                return Expression.New(
                            constructor,
                            arguments);
            else
                return Expression.New(
                            constructor,
                            arguments,
                            members);
        }

        IDictionary<string, LabelTarget> _uidLabelTargets;

        LabelTarget GetLabelTarget(string uid, string name, Type type)
        {
            Contract.Requires<ArgumentNullException>(uid != null, nameof(uid));

            if (_uidLabelTargets == null)
                _uidLabelTargets = new Dictionary<string, LabelTarget>();

            LabelTarget target;

            if (!_uidLabelTargets.TryGetValue(uid, out target))
            {
                target = type != null
                            ? name != null
                                ? Expression.Label(type, name)
                                : Expression.Label(type)
                            : name != null
                                ? Expression.Label(name)
                                : Expression.Label();
                _uidLabelTargets[uid] = target;
            }

            return target;
        }

        LabelTarget VisitLabelTarget(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var name = GetName(element);

            return GetLabelTarget(
                        (element.Attribute(XNames.Attributes.Uid) ?? element.Attribute(XNames.Attributes.Uidref)).Value,
                        name,
                        DataSerialization.GetType(element) ?? typeof(void));
        }

        LabelExpression VisitLabel(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var target = VisitLabelTarget(element.Element(XNames.Elements.LabelTarget));
            var expression = Visit(element.Elements().Skip(1).FirstOrDefault());

            return expression!=null
                        ? Expression.Label(target, expression)
                        : Expression.Label(target);
        }

        LabelTarget VisitBreakLabel(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return VisitLabelTarget(
                        element.Element(XNames.Elements.LabelTarget));
        }

        LabelTarget VisitContinueLabel(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return VisitLabelTarget(
                        element.Element(XNames.Elements.LabelTarget));
        }

        GotoExpression VisitGoto(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            return Expression.MakeGoto(
                                (GotoExpressionKind)Enum.Parse(
                                                typeof(GotoExpressionKind),
                                                element.Attribute(XNames.Attributes.Kind).Value,
                                                true),
                                VisitLabelTarget(element.Elements().ElementAt(0)),
                                Visit(element.Elements().Skip(1).FirstOrDefault()),
                                DataSerialization.GetType(element));
        }

        LoopExpression VisitLoop(XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            var count = element.Elements().Count();

            if (count == 1)
                return Expression.Loop(
                            Visit(
                                element.Elements().ElementAt(0)));

            if (count == 2)
                return Expression.Loop(
                            Visit(
                                element.Elements().ElementAt(0)),
                            VisitBreakLabel(
                                element.Elements().ElementAt(1)));

            return Expression.Loop(
                        Visit(
                            element.Elements().ElementAt(0)),
                        VisitBreakLabel(
                            element.Elements().ElementAt(1)),
                        VisitContinueLabel(
                            element.Elements().ElementAt(2)));
        }

        Expression VisitSwitch(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            var defaultElement = e.Elements(XNames.Elements.DefaultCase).FirstOrDefault();

            return Expression.Switch(
                        DataSerialization.GetType(e),
                        Visit(e.Elements().FirstOrDefault()),
                        defaultElement!=null
                                ? Visit(defaultElement.Elements().FirstOrDefault())
                                : null,
                        GetMethodInfo(e),
                        e.Elements(XNames.Elements.Case).Select(c => VisitSwitchCase(c)));
        }

        SwitchCase VisitSwitchCase(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            return Expression.SwitchCase(
                        Visit(e.Elements().Last()),
                        e.Elements(XNames.Elements.Value)
                              .Select(v => Visit(v.Elements()
                                                  .FirstOrDefault())));
        }

        Expression VisitTry(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            return Expression.MakeTry(
                        DataSerialization.GetType(e),
                        Visit(e.Elements().First()),
                        Visit(e.Elements(XNames.Elements.Finally).FirstOrDefault()),
                        Visit(e.Elements(XNames.Elements.Fault).FirstOrDefault()),
                        e.Elements(XNames.Elements.Catch).Select(c => VisitCatchBlock(c)));
        }

        CatchBlock VisitCatchBlock(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            var exceptionElement = e.Elements(XNames.Elements.Exception)
                                    .FirstOrDefault();
            var exception = exceptionElement != null
                                        ? Expression.Parameter(
                                                        DataSerialization.GetType(exceptionElement),
                                                        GetName(exceptionElement))
                                        : null;

            try
            {

                if (exception != null)
                    _references[exception.Name] = exception;

                var filter = Visit(e.Elements(XNames.Elements.Filter).FirstOrDefault());
                var catchBodyIndex = (exceptionElement==null ? 0 : 1) + (filter==null ? 0 : 1);

                return Expression.MakeCatchBlock(
                                        DataSerialization.GetType(e),
                                        exception,
                                        Visit(e.Elements().ElementAt(catchBodyIndex)),
                                        filter);
            }
            finally
            {
                if (exception != null)
                    _references.Remove(exception.Name);
            }
        }

        Expression VisitExpressionContainer(XElement e)
        {
            if (e == null)
                return null;

            return Visit(e.Elements().SingleOrDefault());
        }

        Expression VisitListInit(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            return Expression.ListInit(
                        VisitNew(e.Elements().First()),
                        e.Elements()
                                .Skip(1)
                                .Take(1)
                                .Elements()
                                .Select(el => VisitElementInit(el)));
        }

        ElementInit VisitElementInit(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            var mi = GetMethodInfo(e);

            if (mi == null)
                throw new SerializationException("Expected method info's contents.");

            return Expression.ElementInit(mi, VisitArguments(e));
        }

        Expression VisitNewArrayInit(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            return Expression.NewArrayInit(
                        DataSerialization.GetType(e),
                        VisitExpressions(e.Element(XNames.Elements.ArrayElements).Elements()));
        }

        Expression VisitNewArrayBounds(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            return Expression.NewArrayBounds(
                        DataSerialization.GetType(e),
                        VisitExpressions(e.Element(XNames.Elements.Bounds).Elements()));
        }

        Expression VisitMemberInit(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            return Expression.MemberInit(
                        Visit(e.Elements().First()) as NewExpression,
                        VisitMemberBindings(e.Element(XNames.Elements.Bindings)));
        }

        IEnumerable<MemberBinding> VisitMemberBindings(XElement xElement)
        {
            Contract.Requires<ArgumentNullException>(xElement != null, nameof(xElement));

            return VisitMemberBindings(
                        xElement.Elements());
        }

        IEnumerable<MemberBinding> VisitMemberBindings(IEnumerable<XElement> xElements)
        {
            Contract.Requires<ArgumentNullException>(xElements != null, nameof(xElements));

            return xElements.Select(
                        e => VisitMemberBinding(e));
        }

        MemberBinding VisitMemberBinding(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            return _typeToMemberBinding[e.Name](this, e);
        }

        readonly IDictionary<XName, Func<ExpressionDeserializingVisitor, XElement, MemberBinding>> _typeToMemberBinding =
            new Dictionary<XName, Func<ExpressionDeserializingVisitor, XElement, MemberBinding>>
            {
                { XNames.Elements.AssignmentBinding,    (v, e) => Expression.Bind(
                                                                        GetMemberInfo(e.Elements().First()),
                                                                        v.Visit(e.Elements().Skip(1).First())) },
                { XNames.Elements.MemberMemberBinding,  (v, e) => Expression.MemberBind(
                                                                        GetMemberInfo(e.Elements().First()),
                                                                        v.VisitMemberBindings(e.Elements().Skip(1).First())) },
                { XNames.Elements.ListBinding,          (v, e) => Expression.ListBind(
                                                                        GetMemberInfo(e.Elements().First()),
                                                                        e.Elements()
                                                                                .Skip(1)
                                                                                .Take(1)
                                                                                .Elements()
                                                                                .Select(el => v.VisitElementInit(el))) },
            };

        Expression VisitRuntimeVariables(XElement e)
        {
            Contract.Requires<ArgumentNullException>(e != null, nameof(e));

            return Expression.RuntimeVariables(
                        VisitParameters(
                            e.Element(
                                XNames.Elements.Variables)));
        }
    }
}
