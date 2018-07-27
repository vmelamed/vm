using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class CSharpDumpExpression : ExpressionVisitor
    {
        protected override Expression VisitParameter(
            ParameterExpression node)
        {
            _writer.Write(node.Name);
            return node;
        }

        protected override Expression VisitRuntimeVariables(
            RuntimeVariablesExpression node)
        {
            // TODO: figure it out
            Debugger.Break();
            foreach (var v in node.Variables)
                Visit(v);
            return node;
        }

        protected override Expression VisitConstant(
            ConstantExpression node)
        {
            if (node.Value == null)
                _writer.Write("null");
            else
            if (_dumpBasicValues.TryGetValue(node.Type, out var writeValue))
                writeValue(_writer, node.Value);
            else
            if (node.Type.IsEnum)
                _writer.Write("{0}.{1}", GetTypeName(node.Type), node.Value.ToString());
            else
                _writer.Write("[{0}: \"{1}\"]", GetTypeName(node.Type), node.Value.ToString());
            return node;
        }

        protected override Expression VisitNew(
            NewExpression node)
        {
            _writer.Write("new {0}(", GetTypeName(node.Type));

            var first = true;

            foreach (var a in node.Arguments)
            {
                if (first)
                    first = false;
                else
                    _writer.Write(", ");
                Visit(a);
            }

            _writer.Write(')');

            return node;
        }

        protected override Expression VisitNewArray(
            NewArrayExpression node)
        {
            _writer.Write("new {0}", node.Type.GetElementType().GetTypeName(ShortenNamesOfGeneratedClasses));

            var first = true;

            switch (node.NodeType)
            {
            case ExpressionType.NewArrayBounds:
                _writer.Write('[');
                foreach (var b in node.Expressions)
                {
                    if (first)
                        first = false;
                    else
                        _writer.Write(", ");
                    Visit(b);
                }
                _writer.Write(']');
                break;

            case ExpressionType.NewArrayInit:
                _writer.Write("{ ");
                foreach (var b in node.Expressions)
                {
                    if (first)
                        first = false;
                    else
                        _writer.Write(", ");
                    Visit(b);
                }
                _writer.Write(" }");
                break;
            }

            return node;
        }

        protected override Expression VisitMemberInit(
            MemberInitExpression node)
        {
            VisitNew(node.NewExpression);
            VisitMemberBindings(node.Bindings);
            return node;
        }

        void VisitMemberBindings(
            ReadOnlyCollection<MemberBinding> bindings)
        {
            _writer.WriteLine();
            _writer.WriteLine('{');
            Indent();

            foreach (var b in bindings)
            {
                VisitMemberBinding(b);
                _writer.WriteLine(',');
            }
            Unindent();
            _writer.Write('}');
        }

        protected override MemberBinding VisitMemberBinding(
            MemberBinding node)
        {
            _writer.Write("{0} = ", node.Member);
            switch (node.BindingType)
            {
            case MemberBindingType.Assignment:
                VisitMemberAssignment((MemberAssignment)node);
                break;

            case MemberBindingType.MemberBinding:
                VisitMemberMemberBinding((MemberMemberBinding)node);
                break;

            case MemberBindingType.ListBinding:
                VisitMemberListBinding((MemberListBinding)node);
                break;
            }
            _writer.Write(',');

            return node;
        }

        protected override MemberAssignment VisitMemberAssignment(
            MemberAssignment node)
        {
            Visit(node.Expression);
            return node;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(
            MemberMemberBinding node)
        {
            VisitMemberBindings(node.Bindings);
            return node;
        }

        protected override MemberListBinding VisitMemberListBinding(
            MemberListBinding node)
        {
            VisitElementInits(node.Initializers);
            return node;
        }

        protected override Expression VisitListInit(
            ListInitExpression node)
        {
            _writer.Write("new {0} ", GetTypeName(node.Type));

            VisitElementInits(node.Initializers);
            return node;
        }

        void VisitElementInits(
            ReadOnlyCollection<ElementInit> elementInits)
        {
            if (elementInits == null)
                throw new ArgumentNullException(nameof(elementInits));

            _writer.WriteLine();
            _writer.WriteLine('{');
            Indent();

            foreach (var i in elementInits)
            {
                VisitElementInit(i);
                _writer.WriteLine(",");
            }

            Unindent();
            _writer.Write('}');
        }

        protected override ElementInit VisitElementInit(
            ElementInit node)
        {
            _writer.WriteLine();
            _writer.WriteLine('{');
            Indent();

            foreach (var e in node.Arguments)
            {
                Visit(e);
                _writer.WriteLine(',');
            }

            Unindent();
            _writer.Write('}');

            return node;
        }

        protected override Expression VisitExtension(
           Expression node)
        {
            Debugger.Break();
            // TODO: what?
            return node;
        }

        protected override Expression VisitLoop(
            LoopExpression node)
        {
            // TODO: handle the various loops here?
            NewLine();
            _writer.Write("while (true)");
            NewLine();
            Visit(node.Body);
            return node;
        }

        protected override Expression VisitMember(
            MemberExpression node)
        {
            _metadata.TryGetValue(node.NodeType, out var meta);

            Visit(node.Expression, meta);
            _writer.Write(meta.Operator);

            _writer.Write(node.Member.Name);
            return node;
        }

        protected override Expression VisitBinary(
            BinaryExpression node)
        {
            _metadata.TryGetValue(node.NodeType, out var meta);

            Visit(node.Left, meta);
            _writer.Write(meta.Operator);
            Visit(node.Right, meta);

            return node;
        }

        protected override Expression VisitTypeBinary(
            TypeBinaryExpression node)
        {
            ExpressionMetadata meta = _metadata[node.NodeType];

            Visit(node.Expression, node);
            _writer.Write("{0}{1}", meta.Operator, GetTypeName(node.TypeOperand));
            return node;
        }

        protected override Expression VisitUnary(
            UnaryExpression node)
        {
            _metadata.TryGetValue(node.NodeType, out var meta);

            if (!meta.IsPostfix)
            {
                if (node.NodeType == ExpressionType.Convert)
                    _writer.Write("({0})", GetTypeName(node.Type));
                else
                if (node.NodeType == ExpressionType.TypeAs  ||
                    node.NodeType == ExpressionType.TypeIs)
                {
                    _writer.Write(node.Operand);
                    _writer.Write("{0}{1}", meta.Operator, GetTypeName(node.Type));
                    return node;
                }
                else
                    _writer.Write(meta.Operator);
            }
            Visit(node.Operand, meta);
            if (meta.IsPostfix)
                _writer.Write(meta.Operator);
            return node;
        }

        protected override Expression VisitLabel(
            LabelExpression node)
        {
            if (node.Target.Type != typeof(void)  &&  node.DefaultValue != null)
            {
                _writer.Write("return ");
                Visit(node.DefaultValue);
            }
            else
                if (!node.Target.Name.IsNullOrWhiteSpace())
            {
                Unindent();
                VisitLabelTarget(node.Target);
                Indent();
            }

            return base.VisitLabel(node);
        }

        protected override LabelTarget VisitLabelTarget(
            LabelTarget node)
        {
            if (!node.Name.IsNullOrWhiteSpace())
                _writer.Write("{0}:", node.Name);
            else
                PrintedText = false;
            NeedsSemicolon = false;
            return node;
        }

        protected override Expression VisitLambda<T>(
            Expression<T> node)
        {
            var first = true;

            _writer.Write("(");
            foreach (var p in node.Parameters)
            {
                if (first)
                    first = false;
                else
                    _writer.Write(", ");
                _writer.Write("{0} {1}", GetTypeName(p.Type), p.Name);
            }
            _writer.Write(") => ");
            PrintedText = true;

            if (node.Body.NodeType != ExpressionType.Block   &&
                node.Body.NodeType != ExpressionType.Loop    &&
                node.Body.NodeType != ExpressionType.Try     &&
                node.Body.NodeType != ExpressionType.Switch  &&
                (node.Body.NodeType != ExpressionType.Conditional || node.Body.Type != typeof(void)))
            {
                Visit(node.Body);
            }
            else
            {
                NewLine();
                if (node.Body.NodeType == ExpressionType.Block)
                    Visit(node.Body);
                else
                    Visit(Expression.Block(node.Body));
            }
            return node;
        }

        protected override Expression VisitGoto(
            GotoExpression node)
        {
            switch (node.Kind)
            {
            case GotoExpressionKind.Goto:
                _writer.Write("goto {0}", node.Target.Name);
                break;

            case GotoExpressionKind.Return:
                _writer.Write("return");
                if (node.Target.Type != typeof(void))
                {
                    _writer.Write(" ");
                    Visit(node.Value);
                }
                break;

            case GotoExpressionKind.Break:
                _writer.Write("break");
                break;

            case GotoExpressionKind.Continue:
                _writer.Write("continue");
                break;

            default:
                break;
            }

            return node;
        }

        protected override Expression VisitIndex(
            IndexExpression node)
        {
            Visit(node.Object);

            if (node.Indexer != null)
                _writer.Write(node.Indexer.Name);
            _writer.Write('[');

            var first = true;

            foreach (var i in node.Arguments)
            {
                if (first)
                    first = false;
                else
                    _writer.Write(", ");
                Visit(i);
            }

            _writer.Write(']');
            return node;
        }

        protected override Expression VisitInvocation(
            InvocationExpression node)
        {
            Visit(node.Expression, node);

            var first = true;

            foreach (var a in node.Arguments)
            {
                if (first)
                    first = false;
                else
                    _writer.Write(", ");
                Visit(a);
            }

            return node;
        }

        protected override Expression VisitDefault(
            DefaultExpression node)
        {
            _writer.Write("default({0})", GetTypeName(node.Type));
            return node;
        }

        protected override Expression VisitDynamic(
            DynamicExpression node)
        {
            // TODO:
            _writer.Write("/* dynamic_expression(); */");
            NeedsSemicolon = false;
            return node;
        }

        protected override Expression VisitBlock(
            BlockExpression node)
        {
            _writer.Write('{');
            PrintedText = true;
            Indent();

            foreach (var v in node.Variables)
            {
                NewLine();
                _writer.Write(GetTypeName(v.Type));
                if (v.Type.IsArray)
                    _writer.Write("[]");
                _writer.Write(" {0}", v.Name);
                Semicolon();
            }

            if (node.Variables.Any())
                NewLine();

            foreach (var e in node.Expressions)
                if (e.NodeType != ExpressionType.DebugInfo || DumpDebugInfo)
                {
                    NewLine();
                    Visit(e);
                    Semicolon();
                }

            NewLine();

            Unindent();
            _writer.Write('}');
            NeedsSemicolon = false;
            PrintedText    = true;

            return node;
        }

        protected override Expression VisitConditional(
            ConditionalExpression node)
        {
            if (node.Type != typeof(void))
            {
                // test ? a : b

                _metadata.TryGetValue(node.NodeType, out var meta);
                Visit(node.Test, meta);
                _writer.Write(" ? ");
                Visit(node.IfTrue, meta);
                _writer.Write(" : ");
                Visit(node.IfFalse, meta);
                return node;
            }

            Expression nd;

            // if (test)
            nd = node.Test;
            _writer.Write("if (");
            Visit(nd);
            _writer.Write(')');
            PrintedText = true;

            // then:
            VisitThenOrElse(node.IfTrue);

            if (node.IfFalse.NodeType != ExpressionType.Default  ||
                node.IfFalse.Type     != typeof(void))
            {
                // else:
                Semicolon();
                _writer.WriteLine();
                _writer.Write("else");
                VisitThenOrElse(node.IfFalse);
            }

            return node;
        }

        void VisitThenOrElse(
            Expression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            NewLine();
            if (node.NodeType != ExpressionType.Block  &&
                node.NodeType != ExpressionType.Loop   &&
                (node.NodeType != ExpressionType.Conditional || node.Type != typeof(void)))
            {
                Indent();
                Visit(node);
                Unindent();
            }
            else
            {
                if (node.NodeType != ExpressionType.Block)
                    node = Expression.Block(node);
                Visit(node);
            }
        }

        protected override Expression VisitMethodCall(
            MethodCallExpression node)
        {
            var i = 0;

            if (node.Method.IsStatic)
            {
                if (node.Method.GetCustomAttribute<ExtensionAttribute>() != null)
                    Visit(node.Arguments[i++]);
                else
                    _writer.Write(GetTypeName(node.Method.DeclaringType));
            }
            else
                Visit(node.Object, node);
            _writer.Write(".{0}(", node.Method.Name);

            var first = true;

            for (; i < node.Arguments.Count; i++)
            {
                var a = node.Arguments[i];
                if (first)
                    first = false;
                else
                    _writer.Write(", ");

                var parameters = node.Method.GetParameters();

                if (parameters[i].IsOut)
                    _writer.Write("out ");
                else
                if (parameters[i].ParameterType.IsByRef)
                    _writer.Write("ref ");

                Visit(a);
            }
            _writer.Write(')');

            return node;
        }

        protected override Expression VisitTry(
            TryExpression node)
        {
            _writer.WriteLine("try");
            if (node.NodeType != ExpressionType.Block)
                Visit(Expression.Block(node.Body));
            else
                Visit(node.Body);

            foreach (var c in node.Handlers)
                VisitCatchBlock(c);

            if (node.Finally != null)
            {
                _writer.WriteLine();
                _writer.WriteLine("finally");
            }

            if (node.Finally.NodeType == ExpressionType.Block)
                Visit(node.Finally);
            else
                Visit(Expression.Block(node.Finally));

            return node;
        }

        protected override CatchBlock VisitCatchBlock(
            CatchBlock node)
        {
            _writer.WriteLine();
            _writer.Write("catch");

            if (node.Test != null  &&  node.Test != typeof(void))
            {
                _writer.Write(" ({0}", GetTypeName(node.Test));
                if (node.Variable != null)
                    _writer.Write(" {0}", node.Variable.Name);
                _writer.Write(") ");
            }

            if (node.Filter != null)
                Visit(node.Filter);

            if (node.Body.NodeType == ExpressionType.Block)
                Visit(node.Body);
            else
                Visit(Expression.Block(node.Body));

            return node;
        }

        protected override Expression VisitSwitch(
            SwitchExpression node)
        {
            _writer.Write("switch (");
            Visit(node.SwitchValue);
            _writer.WriteLine(')');
            _writer.WriteLine('{');
            foreach (var c in node.Cases)
                VisitSwitchCase(c);
            VisitSwitchDefault(node.DefaultBody);
            _writer.WriteLine('}');
            NeedsSemicolon = false;
            return node;
        }

        protected override SwitchCase VisitSwitchCase(
            SwitchCase node)
        {
            foreach (var cte in node.TestValues)
            {
                _writer.WriteLine("case ");
                Visit(cte);
                _writer.WriteLine(':');
            }
            Indent();
            Visit(node.Body);
            _writer.WriteLine();
            _writer.Write("break;");
            Unindent();
            return node;
        }

        void VisitSwitchDefault(
            Expression node)
        {
            _writer.WriteLine("default:");
            Indent();
            Visit(node);
            _writer.WriteLine();
            _writer.Write("break;");
            Unindent();
        }

        protected override Expression VisitDebugInfo(
            DebugInfoExpression node)
        {
            _writer.Write("// {0}: {1}, {3}", node.Document.FileName, node.StartLine, node.EndLine, node.StartColumn, node.EndColumn);
            NeedsSemicolon = false;
            return node;
        }

        void Semicolon()
        {
            if (NeedsSemicolon)
                _writer.Write(';');
            else
                NeedsSemicolon = true;
        }

        void NewLine()
        {
            if (PrintedText)
                _writer.WriteLine();
            else
                PrintedText = true;
        }

        string GetTypeName(
            Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var s = type.GetTypeName(ShortenNamesOfGeneratedClasses);

            if (type.IsArray)
                s += "[]";

            return s;
        }
    }
}
