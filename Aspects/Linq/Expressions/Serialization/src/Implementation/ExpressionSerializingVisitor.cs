using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    /// <summary>
    /// Visits the nodes of an expression and serializes the data in them to an XML.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [SuppressMessage("Microsoft.Security", "CA2136:TransparencyAnnotationsShouldNotConflictFxCopRule")]
    [SecuritySafeCritical]
    partial class ExpressionSerializingVisitor : ExpressionVisitor
    {
        /// <summary>
        /// The intermediate results (XElements) are pushed here to be popped out and placed later as operands (sub-elements) into a parent element, 
        /// representing an expression node's operation.
        /// E.g. the sequence of operations while serializing "a+b+c" may look like this:
        /// <para>
        /// push Element(b)
        /// </para><para>
        /// push Element(a)
        /// </para><para>
        /// push AddElement(pop, pop)
        /// </para><para>
        /// push Element(c)
        /// </para><para>
        /// push AddElement(pop, pop)
        /// </para><para>
        /// As in a reversed polish record.
        /// </para>
        /// In the end of a successful visit the stack should contain only one element - the root of the whole expression.
        /// </summary>
        readonly Stack<XElement> _elements = new Stack<XElement>();

#if DEBUG
        public ExpressionSerializingVisitor()
            => Debug.IndentSize = 2;
#endif

        /// <summary>
        /// Gets the result - the only element left in the stack after a successful visit.
        /// </summary>
        public XElement Result
        {
            get
            {
                Debug.Assert(_elements.Count == 1, "There must be exactly one element in the queue - the root element of the expression.");

                return _elements.Pop();
            }
        }

#if DEBUG
        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// Used in Debug mode only for tracing the expression tree traversal.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        /// <exception cref="System.ArgumentNullException">node</exception>
        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            Debug.WriteLine("==== "+node.NodeType);
            Debug.Indent();

            var x = base.Visit(node);

            Debug.Unindent();

            return x;
        }
#endif

        /// <summary>
        /// Invokes the base class' visit method on the expression node (which may reduce it), creates the representing XML element and invokes the
        /// XML serializing delegate.
        /// </summary>
        /// <typeparam name="E">The type of the visited expression.</typeparam>
        /// <param name="expressionNode">The expression node to be serialized.</param>
        /// <param name="baseVisit">Delegate to the base class' visiting method.</param>
        /// <param name="thisVisit">Delegate to the XML serializing method.</param>
        /// <returns>The possibly reduced expression.</returns>
        Expression GenericVisit<E>(
            E expressionNode,
            Func<E, Expression> baseVisit,
            Action<E, XElement> thisVisit) where E : Expression
        {
            if (baseVisit == null)
                throw new ArgumentNullException(nameof(baseVisit));

            var reducedNode = baseVisit(expressionNode);

            if (!(reducedNode is E node))
                return reducedNode;

            var nodeName = new StringBuilder(reducedNode.NodeType.ToString());

            // make it camel-case
            nodeName[0] = char.ToLower(nodeName[0], CultureInfo.InvariantCulture);

            var element = new XElement(XNames.Xxp+nodeName.ToString());

            thisVisit(node, element);
            _elements.Push(element);

            return node;
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ConstantExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitConstant(ConstantExpression node)
            => GenericVisit(
                    node,
                    base.VisitConstant,
                    (n, e) => DataSerialization.GetSerializer(n.Type)(n.Value, n.Type, e));

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.DefaultExpression" /> and generates XML element out of it.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitDefault(DefaultExpression node)
            => GenericVisit(
                    node,
                    base.VisitDefault,
                    (n, e) => e.AddTypeAttribute(n));

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ParameterExpression" /> and generates XML element out of it.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression node)
            => GenericVisit(
                    node,
                    base.VisitParameter,
                    (n, e) => e.AddTypeAttribute(n)
                                .Add(
                                    new XAttribute(XNames.Attributes.Name, n.Name),
                                    n.IsByRef ? new XAttribute(XNames.Attributes.IsByRef, n.IsByRef) : null));

        /// <summary>
        /// Visits the children of the Expression{TDelegate} and generates XML element out of it.
        /// </summary>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
            => GenericVisit(
                    node,
                    base.VisitLambda,
                    (n, e) =>
                    {
                        // pop the parameters
                        var parameters = AddParameters(n.Parameters, new XElement(XNames.Elements.Parameters));

                        // pop the body
                        var body = new XElement(
                                            XNames.Elements.Body,
                                            _elements.Pop());

                        ReplaceParametersWithReferences(parameters, body);

                        // push the lambda
                        e.Add(
                            n.TailCall ? new XAttribute(XNames.Attributes.TailCall, n.TailCall) : null,
                            !string.IsNullOrWhiteSpace(n.Name) ? new XAttribute(XNames.Attributes.Name, n.Name) : null,
                            n.ReturnType!=null && n.ReturnType!=n.Body.Type ? new XAttribute(XNames.Attributes.DelegateType, n.Type) : null,
                            parameters,
                            body);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.UnaryExpression" /> and generates XML element out of it.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitUnary(UnaryExpression node)
            => GenericVisit(
                    node,
                    base.VisitUnary,
                    (n, e) => e.Add(
                                VisitAsType(n),
                                _elements.Pop(),
                                VisitMethodInfo(n)));

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.BinaryExpression" /> and serializes it to an XML element.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitBinary(BinaryExpression node)
            => GenericVisit(
                    node,
                    base.VisitBinary,
                    (n, e) =>
                    {
                        var op2 = _elements.Pop();
                        var op1 = _elements.Pop();

                        e.Add(
                            n.IsLiftedToNull ? new XAttribute(XNames.Attributes.IsLiftedToNull, true) : null,
                            op1,
                            op2,
                            VisitMethodInfo(n));
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.TypeBinaryExpression" /> and serializes it to an XML element.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
            => GenericVisit(
                    node,
                    base.VisitTypeBinary,
                    (n, e) => e.AddTypeAttribute(n)
                                .Add(_elements.Pop()));

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.BlockExpression" /> and serializes it to an XML element.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitBlock(BlockExpression node)
            => GenericVisit(
                    node,
                    base.VisitBlock,
                    (n, e) =>
                    {
                        // pop the variables
                        var variables = n.Variables.Count() > 0
                                                ? AddParameters(n.Variables, new XElement(XNames.Elements.Variables))
                                                : null;
                        var expressions = new Stack<XElement>();

                        // replace the parameters with references and reverse the expressions in the _elements stack
                        for (var i = 0; i<n.Expressions.Count(); i++)
                            expressions.Push(ReplaceParametersWithReferences(variables, _elements.Pop()));

                        Debug.Assert(n.Type != null, "The expression node's type is null - remove the default type value of typeof(void) below.");
                        e.AddTypeAttribute(n, typeof(void))
                            .Add(
                                variables,
                                expressions);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.ConditionalExpression" /> and serializes it to an XML element.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitConditional(ConditionalExpression node)
            => GenericVisit(
                    node,
                    base.VisitConditional,
                    (n, e) =>
                    {
                        var op3 = _elements.Pop();
                        var op2 = _elements.Pop();
                        var op1 = _elements.Pop();

                        Debug.Assert(n.Type != null, "The expression node's type is null - remove the default type value of typeof(void) below.");
                        e.AddTypeAttribute(n, typeof(void))
                            .Add(
                            op1,
                            op2,
                            op3);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.NewExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitNew(NewExpression node)
            => GenericVisit(
                    node,
                    base.VisitNew,
                    (n, e) => e.Add(
                        VisitConstructorInfo(node.Constructor),
                        AddArguments(node.Arguments, new XElement(XNames.Elements.Arguments)),
                        node.Members != null
                            ? new XElement(
                                XNames.Elements.Members,
                                node.Members.Select(x => VisitMemberInfo(x)))
                            : null));

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMember(MemberExpression node)
            => GenericVisit(
                    node,
                    base.VisitMember,
                    (n, e) =>
                    {
                        e.Add(
                            _elements.Pop(),
                            VisitMemberInfo(node.Member));
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.IndexExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitIndex(IndexExpression node)
            => GenericVisit(
                    node,
                    base.VisitIndex,
                    (n, e) =>
                    {
                        var indexes = AddArguments(node.Arguments, new XElement(XNames.Elements.Indexes));
                        var array = _elements.Pop();

                        e.Add(
                            array,
                            indexes);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
            => GenericVisit(
                    node,
                    base.VisitMethodCall,
                    (n, e) =>
                    {
                        var arguments = AddArguments(node.Arguments, new XElement(XNames.Elements.Arguments));
                        var instance = node.Object!=null ? _elements.Pop() : null;
                        var method = VisitMethodInfo(node.Method);

                        if (instance != null)
                            e.Add(instance, method, arguments);
                        else
                            e.Add(method, arguments);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.InvocationExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitInvocation(InvocationExpression node)
            => GenericVisit(
                    node,
                    base.VisitInvocation,
                    (n, e) =>
                    {
                        e.Add(
                            _elements.Pop(),
                            AddArguments(node.Arguments, new XElement(XNames.Elements.Arguments)));
                    });

        /// <summary>
        /// Visits the children of the extension expression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitExtension(Expression node)
        {
            var basePointer = _elements.Count();

            return GenericVisit(
                        node,
                        base.VisitExtension,
                        (n, e) =>
                        {
                            var extensionElement = new XElement(XNames.Elements.Extension).AddTypeAttribute(n);

                            for (int i = _elements.Count(); i>basePointer; i--)
                                extensionElement.Add(_elements.Pop());
                        });
        }

        IDictionary<LabelTarget, string> _labelTargetsUid;
        int _lastLabelUid;

        string GetLabelTargetUid(LabelTarget target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (_labelTargetsUid == null)
                _labelTargetsUid = new Dictionary<LabelTarget, string>();

            if (!_labelTargetsUid.TryGetValue(target, out var uid))
            {
                uid = $"L{++_lastLabelUid}";
                _labelTargetsUid[target] = uid;
            }

            return uid;
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.LabelTarget" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            if (node == null)
                return null;

            var targetNode = base.VisitLabelTarget(node);

            Debug.WriteLine("==== _labelTarget");

            var element = new XElement(
                                XNames.Elements.LabelTarget,
                                node.Type != null &&
                                node.Type != typeof(void)
                                    ? new XAttribute(
                                            XNames.Attributes.Type,
                                            TypeNameResolver.GetTypeName(node.Type))
                                    : null,
                                !string.IsNullOrWhiteSpace(node.Name)
                                    ? new XAttribute(
                                            XNames.Attributes.Name,
                                            node.Name)
                                    : null,
                                new XAttribute(
                                        XNames.Attributes.Uid,
                                        GetLabelTargetUid(node)));

            _elements.Push(element);

            return targetNode;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.LabelExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitLabel(LabelExpression node)
            => GenericVisit(
                    node,
                    base.VisitLabel,
                    (n, e) =>
                    {
                        XElement value = node.DefaultValue != null
                                                ? _elements.Pop()
                                                : null;

                        e.Add(
                            _elements.Pop(),
                            value);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.GotoExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitGoto(GotoExpression node)
            => GenericVisit(
                    node,
                    base.VisitGoto,
                    (n, e) =>
                    {
                        var expression = node.Value != null ? _elements.Pop() : null;
                        var kind = new StringBuilder(node.Kind.ToString());

                        kind[0] = char.ToLowerInvariant(kind[0]);

                        e.AddTypeAttribute(n)
                            .Add(
                            new XAttribute(
                                    XNames.Attributes.Kind,
                                    kind.ToString()),
                            PopLabelTargetAndReplaceUidWithUidRef(),
                            expression);
                    });

        XElement PopLabelTargetAndReplaceUidWithUidRef()
        {
            var target = _elements.Pop();
            var uid = target.Attribute(XNames.Attributes.Uid);

            target.Add(new XAttribute(XNames.Attributes.Uidref, uid.Value));
            uid.Remove();

            return target;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.LoopExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitLoop(LoopExpression node)
            => GenericVisit(
                    node,
                    base.VisitLoop,
                    (n, e) =>
                    {
                        e.Add(_elements.Pop());

                        if (node.BreakLabel != null)
                            e.Add(
                                new XElement(
                                        XNames.Elements.BreakLabel,
                                        _elements.Pop()));

                        if (node.ContinueLabel != null)
                            e.Add(
                                new XElement(
                                        XNames.Elements.ContinueLabel,
                                        _elements.Pop()));
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.SwitchExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitSwitch(SwitchExpression node)
            => GenericVisit(
                    node,
                    base.VisitSwitch,
                    (n, e) =>
                    {
                        var comparison = node.Comparison != null                // get the non-default comparison method
                                                ? VisitMethodInfo(node.Comparison)
                                                : null;
                        var @default = node.DefaultBody != null                 // the body of the default case
                                                ? new XElement(
                                                        XNames.Elements.DefaultCase,
                                                        _elements.Pop())
                                                : null;
                        var cases = PopExpressions(node.Cases.Count());         // the cases
                        var value = _elements.Pop();                            // the value to switch on

                        Debug.Assert(n.Type != null, "The expression node's type is null - remove the default type value of typeof(void) below.");
                        e.AddTypeAttribute(n, typeof(void))
                            .Add(
                            value,
                            comparison,
                            cases,
                            @default);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.SwitchCase" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Debug.WriteLine("==== switchCase");
            Debug.Indent();

            var switchCase = base.VisitSwitchCase(node);

            var caseExpression = _elements.Pop();
            var testValues = new Stack<XElement>();

            for (int i = 0; i<node.TestValues.Count(); i++)
                testValues.Push(
                    new XElement(
                            XNames.Elements.Value,
                            _elements.Pop()));

            _elements.Push(new XElement(
                                    XNames.Elements.Case,
                                    testValues,
                                    caseExpression));

            Debug.Unindent();
            return switchCase;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.TryExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitTry(TryExpression node)
            => GenericVisit(
                    node,
                    base.VisitTry,
                    (n, e) =>
                    {
                        var @finally = n.Finally!=null
                                            ? new XElement(
                                                    XNames.Elements.Finally,
                                                    _elements.Pop())
                                            : null;
                        var @catch = n.Fault!=null
                                            ? new XElement(
                                                    XNames.Elements.Fault,
                                                    _elements.Pop())
                                            : null;
                        IEnumerable<XElement> catches = null;

                        if (n.Handlers != null  &&
                            n.Handlers.Count() > 0)
                        {
                            catches = new Stack<XElement>();

                            for (var i = 0; i<n.Handlers.Count(); i++)
                                ((Stack<XElement>)catches).Push(_elements.Pop());
                        }
                        var @try = _elements.Pop();

                        e.AddTypeAttribute(node)
                            .Add(
                                @try,
                                catches,
                                @catch,
                                @finally);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.CatchBlock" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Debug.WriteLine("==== catchBlock");
            Debug.Indent();

            var catchBlock = base.VisitCatchBlock(node);

            var body = _elements.Pop();
            var filter = node.Filter!=null
                                ? new XElement(
                                        XNames.Elements.Filter,
                                        _elements.Pop())
                                : null;
            var variable = node.Variable!=null ? _elements.Pop() : null;

            if (variable!=null)
            {
                ReplaceParameterWithReference(variable, body);
                if (filter != null)
                    ReplaceParameterWithReference(variable, filter);
            }

            var exception = variable!=null
                                ? new XElement(
                                        XNames.Elements.Exception,
                                        variable.Attributes())
                                : null;

            _elements.Push(new XElement(
                                    XNames.Elements.Catch,
                                    new XAttribute(
                                            XNames.Attributes.Type,
                                            TypeNameResolver.GetTypeName(node.Test)),
                                    exception,
                                    filter,
                                    body));

            Debug.Unindent();
            return catchBlock;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.ListInitExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitListInit(ListInitExpression node)
            => GenericVisit(
                    node,
                    base.VisitListInit,
                    (n, e) =>
                    {
                        var inits = new Stack<XElement>();

                        for (var i = 0; i<node.Initializers.Count(); i++)
                            inits.Push(_elements.Pop());

                        e.Add(_elements.Pop(),                      // the new node
                                new XElement(
                                    XNames.Elements.ListInit,
                                    inits));                        // the elementsInit node
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.ElementInit" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override ElementInit VisitElementInit(ElementInit node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Debug.WriteLine("==== elementInit");
            Debug.Indent();

            var elementInit = base.VisitElementInit(node);
            var arguments = new Stack<XElement>();

            for (var i = 0; i<node.Arguments.Count(); i++)
                arguments.Push(_elements.Pop());

            _elements.Push(
                new XElement(
                        XNames.Elements.ElementInit,
                        VisitMethodInfo(node.AddMethod),
                        new XElement(
                                XNames.Elements.Arguments,
                                arguments)));

            Debug.Unindent();
            return elementInit;
        }

        void VisitNewArrayInit(NewArrayExpression node, XElement element)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var inits = new Stack<XElement>();

            for (var i = 0; i<node.Expressions.Count(); i++)
                inits.Push(_elements.Pop());

            element.Add(
                new XAttribute(
                        XNames.Attributes.Type,
                        TypeNameResolver.GetTypeName(node.Type.GetElementType())), // the new node
                new XElement(
                        XNames.Elements.ArrayElements,
                        inits));
        }

        void VisitNewArrayBounds(NewArrayExpression node, XElement element)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var bounds = new Stack<XElement>();

            for (var i = 0; i<node.Expressions.Count(); i++)
                bounds.Push(_elements.Pop());

            element.Add(
                new XAttribute(
                        XNames.Attributes.Type,
                        TypeNameResolver.GetTypeName(node.Type.GetElementType())), // the new node
                new XElement(
                        XNames.Elements.Bounds,
                        bounds));
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.NewArrayExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitNewArray(NewArrayExpression node)
            => GenericVisit(
                    node,
                    base.VisitNewArray,
                    (n, e) =>
                    {
                        if (n.NodeType == ExpressionType.NewArrayInit)
                            VisitNewArrayInit(n, e);
                        else
                            VisitNewArrayBounds(n, e);
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberInitExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMemberInit(MemberInitExpression node)
            => GenericVisit(
                    node,
                    base.VisitMemberInit,
                    (n, e) =>
                    {
                        var bindings = new Stack<XElement>();

                        for (var i = 0; i<node.Bindings.Count(); i++)
                            bindings.Push(_elements.Pop());

                        e.Add(
                            _elements.Pop(),        // the new expression
                            new XElement(
                                    XNames.Elements.Bindings,
                                    bindings));
                    });

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberBinding" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Debug.WriteLine("==== memberBinding");
            Debug.Indent();

            var bindingTypeValue = new StringBuilder(node.BindingType.ToString());

            bindingTypeValue[0] = char.ToLowerInvariant(bindingTypeValue[0]);

            var binding = base.VisitMemberBinding(node);

            Debug.Unindent();
            return binding;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberAssignment" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override MemberAssignment VisitMemberAssignment(
            MemberAssignment node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Debug.WriteLine("==== memberAssignment");
            Debug.Indent();

            var binding = base.VisitMemberAssignment(node);

            _elements.Push(
                new XElement(
                        XNames.Elements.AssignmentBinding,
                        VisitMemberInfo(node.Member),
                        _elements.Pop()));

            Debug.Unindent();
            return binding;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberMemberBinding" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Debug.WriteLine("==== memberMemberBinding");
            Debug.Indent();

            var binding = base.VisitMemberMemberBinding(node);
            var bindings = new Stack<XElement>();

            for (var i = 0; i<node.Bindings.Count(); i++)
                bindings.Push(_elements.Pop());

            _elements.Push(
                new XElement(
                        XNames.Elements.MemberMemberBinding,
                        VisitMemberInfo(node.Member),
                        new XElement(
                                XNames.Elements.Bindings,
                                bindings)));

            Debug.Unindent();
            return binding;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberListBinding" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Debug.WriteLine("==== memberListBinding");
            Debug.Indent();

            var binding = base.VisitMemberListBinding(node);

            _elements.Push(
                new XElement(
                        XNames.Elements.ListBinding,
                        VisitMemberInfo(node.Member),
                        _elements.Pop()));

            Debug.Unindent();
            return binding;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.RuntimeVariablesExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
            => GenericVisit(
                    node,
                    base.VisitRuntimeVariables,
                    (n, e) => e.Add(
                                AddParameters(
                                    n.Variables,
                                    new XElement(XNames.Elements.Variables))));

        /////////////////////////////////////////////////////////////////
        // IN PROCESS:
        /////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////
        // TODO:
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.DynamicExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitDynamic(DynamicExpression node) => GenericVisit(
                                                                                    node,
                                                                                    base.VisitDynamic,
                                                                                    (n, e) => { });
    }
}
