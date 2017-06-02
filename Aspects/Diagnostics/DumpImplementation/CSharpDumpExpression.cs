using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    class CSharpDumpExpression : ExpressionVisitor, IDisposable
    {
        readonly StringWriter _textWriter = new StringWriter();
        readonly DumpTextWriter _writer;

        int Indent() => ++_writer.Indent;

        int Unindent() => --_writer.Indent;

        bool NeedsSemicolon { get; set; } = true;

        public bool ShortenNamesOfGeneratedClasses { get; set; } = false;

        public bool DumpDebugInfo { get; set; } = false;

        public CSharpDumpExpression()
        {
            _writer = new DumpTextWriter(_textWriter)
            {
                Indent     = 0,
                IndentSize = 4,
            };
        }

        public string DumpText
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                return !IsDisposed ? _textWriter.GetStringBuilder().ToString() : null;
            }
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <see langword="true"/> if the object has already been disposed, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the finalizer.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/>==<see langword="true"/>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <see langword="false"/> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _writer.Dispose();
                _textWriter.Dispose();
            }
        }
        #endregion

        struct ExpressionMetadata
        {
            public int Weight;
            public bool LeftAssociative;
            public bool IsPostfix;
            public string Operator;
        }

        static readonly IReadOnlyDictionary<ExpressionType, ExpressionMetadata> _metadata = new ReadOnlyDictionary<ExpressionType, ExpressionMetadata>(
            new Dictionary<ExpressionType, ExpressionMetadata>
            {
                [ExpressionType.Constant]               = new ExpressionMetadata { Weight = 15, LeftAssociative =  true, IsPostfix = false, Operator = "" },
                [ExpressionType.Parameter]              = new ExpressionMetadata { Weight = 15, LeftAssociative =  true, IsPostfix = false, Operator = "" },

                [ExpressionType.MemberAccess]           = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true,  Operator = "." },
                [ExpressionType.Invoke]                 = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true,  Operator = "" },
                [ExpressionType.Index]                  = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true,  Operator = "" },
                [ExpressionType.ArrayIndex]             = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true,  Operator = "" },
                [ExpressionType.PostIncrementAssign]    = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true,  Operator = "++" },
                [ExpressionType.PostDecrementAssign]    = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true,  Operator = "--" },
                [ExpressionType.New]                    = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true,  Operator = "new " },

                [ExpressionType.NegateChecked]          = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "-" },
                [ExpressionType.Negate]                 = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "-" },
                [ExpressionType.UnaryPlus]              = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "+" },
                [ExpressionType.Not]                    = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "!" },
                [ExpressionType.OnesComplement]         = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "~" },
                [ExpressionType.PreIncrementAssign]     = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "++" },
                [ExpressionType.PreDecrementAssign]     = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "--" },
                [ExpressionType.ConvertChecked]         = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "" },
                [ExpressionType.Convert]                = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, Operator = "" },

                [ExpressionType.MultiplyChecked]        = new ExpressionMetadata { Weight = 12, LeftAssociative =  true, IsPostfix = false, Operator = " * " },
                [ExpressionType.Multiply]               = new ExpressionMetadata { Weight = 12, LeftAssociative =  true, IsPostfix = false, Operator = " * " },
                [ExpressionType.Divide]                 = new ExpressionMetadata { Weight = 12, LeftAssociative =  true, IsPostfix = false, Operator = " / " },
                [ExpressionType.Modulo]                 = new ExpressionMetadata { Weight = 12, LeftAssociative =  true, IsPostfix = false, Operator = " % " },

                [ExpressionType.AddChecked]             = new ExpressionMetadata { Weight = 11, LeftAssociative =  true, IsPostfix = false, Operator = " + " },
                [ExpressionType.Add]                    = new ExpressionMetadata { Weight = 11, LeftAssociative =  true, IsPostfix = false, Operator = " + " },
                [ExpressionType.SubtractChecked]        = new ExpressionMetadata { Weight = 11, LeftAssociative =  true, IsPostfix = false, Operator = " - " },
                [ExpressionType.Subtract]               = new ExpressionMetadata { Weight = 11, LeftAssociative =  true, IsPostfix = false, Operator = " - " },

                [ExpressionType.LeftShift]              = new ExpressionMetadata { Weight = 10, LeftAssociative =  true, IsPostfix = false, Operator = " << " },
                [ExpressionType.RightShift]             = new ExpressionMetadata { Weight = 10, LeftAssociative =  true, IsPostfix = false, Operator = " >> " },

                [ExpressionType.GreaterThan]            = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, Operator = " > " },
                [ExpressionType.GreaterThanOrEqual]     = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, Operator = " >= " },
                [ExpressionType.LessThan]               = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, Operator = " < " },
                [ExpressionType.LessThanOrEqual]        = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, Operator = " <= " },
                [ExpressionType.TypeAs]                 = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, Operator = " as " },
                [ExpressionType.TypeIs]                 = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, Operator = " is " },

                [ExpressionType.Equal]                  = new ExpressionMetadata { Weight =  8, LeftAssociative =  true, IsPostfix = false, Operator = " == " },
                [ExpressionType.NotEqual]               = new ExpressionMetadata { Weight =  8, LeftAssociative =  true, IsPostfix = false, Operator = " != " },

                [ExpressionType.And]                    = new ExpressionMetadata { Weight =  7, LeftAssociative =  true, IsPostfix = false, Operator = " & " },

                [ExpressionType.ExclusiveOr]            = new ExpressionMetadata { Weight =  6, LeftAssociative =  true, IsPostfix = false, Operator = " ^ " },

                [ExpressionType.Or]                     = new ExpressionMetadata { Weight =  5, LeftAssociative =  true, IsPostfix = false, Operator = " | " },

                [ExpressionType.AndAlso]                = new ExpressionMetadata { Weight =  4, LeftAssociative =  true, IsPostfix = false, Operator = " && " },

                [ExpressionType.OrElse]                 = new ExpressionMetadata { Weight =  3, LeftAssociative =  true, IsPostfix = false, Operator = " || " },

                [ExpressionType.Conditional]            = new ExpressionMetadata { Weight =  2, LeftAssociative =  true, IsPostfix = false, Operator = " ? " },

                [ExpressionType.Assign]                 = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " = " },
                [ExpressionType.MultiplyAssignChecked]  = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " *= " },
                [ExpressionType.MultiplyAssign]         = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " *= " },
                [ExpressionType.DivideAssign]           = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " /= " },
                [ExpressionType.ModuloAssign]           = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " %= " },
                [ExpressionType.AddAssign]              = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " += " },
                [ExpressionType.AddAssignChecked]       = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " += " },
                [ExpressionType.SubtractAssign]         = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " -= " },
                [ExpressionType.SubtractAssignChecked]  = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " -= " },
                [ExpressionType.LeftShiftAssign]        = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " <<= " },
                [ExpressionType.RightShiftAssign]       = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " >>= " },
                [ExpressionType.AndAssign]              = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " &= " },
                [ExpressionType.OrAssign]               = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " |= " },
                [ExpressionType.ExclusiveOrAssign]      = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, Operator = " ^= " },
            });

        Expression Visit(
            Expression node,
            Expression parentNode)
        {
            Contract.Requires<ArgumentNullException>(node       != null, nameof(node));
            Contract.Requires<ArgumentNullException>(parentNode != null, nameof(parentNode));
            Contract.Ensures(Contract.Result<Expression>() != null);

            ExpressionMetadata parentMeta;

            _metadata.TryGetValue(parentNode.NodeType, out parentMeta);

            return Visit(node, parentMeta);
        }

        Expression Visit(
            Expression node,
            ExpressionMetadata parentMeta)
        {
            Contract.Requires<ArgumentNullException>(node != null, nameof(node));
            Contract.Ensures(Contract.Result<Expression>() != null);

            ExpressionMetadata meta;

            _metadata.TryGetValue(node.NodeType, out meta);

            if (meta.Weight > parentMeta.Weight  ||
                meta.Weight == parentMeta.Weight  &&  meta.LeftAssociative == parentMeta.LeftAssociative)
                Visit(node);
            else
            {
                _writer.Write('(');
                Visit(node);
                _writer.Write(')');
            }

            return node;
        }

        void Semicolon()
        {
            if (NeedsSemicolon)
                _writer.Write(';');
            else
                NeedsSemicolon = true;
        }

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

        static readonly IReadOnlyDictionary<Type, Action<TextWriter, object>> _dumpBasicValues = new ReadOnlyDictionary<Type, Action<TextWriter, object>>(
            new Dictionary<Type, Action<TextWriter, object>>
            {
                [typeof(DBNull)]         = (w, v) => w.Write("DBNull.Value"),
                [typeof(bool)]           = (w, v) => w.Write(v.ToString()),
                [typeof(byte)]           = (w, v) => w.Write("(byte){0}", v.ToString()),
                [typeof(sbyte)]          = (w, v) => w.Write("(sbyte){0}", v.ToString()),
                [typeof(char)]           = (w, v) => w.Write("'{0}'", v.ToString()),
                [typeof(short)]          = (w, v) => w.Write("(short){0}", v.ToString()),
                [typeof(int)]            = (w, v) => w.Write(v.ToString()),
                [typeof(long)]           = (w, v) => w.Write("{0}L", v.ToString()),
                [typeof(ushort)]         = (w, v) => w.Write("(ushort){0}", v.ToString()),
                [typeof(uint)]           = (w, v) => w.Write("{0}U", v.ToString()),
                [typeof(ulong)]          = (w, v) => w.Write("{0}UL", v.ToString()),
                [typeof(float)]          = (w, v) => w.Write("{0}F", v.ToString()),
                [typeof(double)]         = (w, v) => w.Write("{0}D", v.ToString()),
                [typeof(decimal)]        = (w, v) => w.Write("{0}M", v.ToString()),
                [typeof(DateTime)]       = (w, v) => w.Write("DateTime(\"{0}\")", ((DateTime)v).ToString("o")),
                [typeof(DateTimeOffset)] = (w, v) => w.Write("DateTimeOffset(\"{0}\")", ((DateTimeOffset)v).ToString("o")),
                [typeof(TimeSpan)]       = (w, v) => w.Write("TimeSpan(\"{0}\")", ((TimeSpan)v).ToString("c")),
                [typeof(Uri)]            = (w, v) => w.Write(v.ToString()),
                [typeof(Guid)]           = (w, v) => w.Write(v.ToString()),
                [typeof(IntPtr)]         = (w, v) => w.Write($"0x{v:x16}"),
                [typeof(UIntPtr)]        = (w, v) => w.Write($"0x{v:x16}"),
                [typeof(string)]         = (w, v) => w.Write("\"{0}\"", (string)v),
            });

        protected override Expression VisitConstant(
            ConstantExpression node)
        {
            Action<TextWriter, object> writeValue;

            if (node.Value == null)
                _writer.Write("null");
            else
            if (_dumpBasicValues.TryGetValue(node.Type, out writeValue))
                writeValue(_writer, node.Value);
            else
                _writer.Write("[\"{0}\"]", node.Value.ToString());
            return node;
        }

        protected override Expression VisitNew(
            NewExpression node)
        {
            _writer.Write("new {0}(", node.Type.GetTypeName(ShortenNamesOfGeneratedClasses));

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
            _writer.Write("new {0} ", node.Type.GetTypeName(ShortenNamesOfGeneratedClasses));

            var inits = node.Initializers.Count;

            if (inits > 1)
            {
                _writer.WriteLine();
                _writer.WriteLine('{');
                Indent();
            }

            var first = true;

            foreach (var i in node.Initializers)
            {
                if (inits > 1)
                {
                    if (first)
                        first = false;
                    else
                        _writer.WriteLine(",");
                }

                VisitElementInit(i);
            }

            if (inits > 1)
            {
                Unindent();
                _writer.Write('}');
            }

            return node;
        }

        void VisitElementInits(
            ReadOnlyCollection<ElementInit> elementInits)
        {
            Contract.Requires<ArgumentNullException>(elementInits != null, nameof(elementInits));

            var inits = elementInits.Count;

            if (inits > 1)
            {
                _writer.WriteLine();
                _writer.WriteLine('{');
                Indent();
            }

            var first = true;

            foreach (var i in elementInits)
            {
                if (inits > 1)
                {
                    if (first)
                        first = false;
                    else
                        _writer.WriteLine(",");
                }

                VisitElementInit(i);
            }

            if (inits > 1)
            {
                Unindent();
                _writer.Write('}');
            }
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
            // TODO: figure it out!
            return node;
        }

        protected override Expression VisitLoop(
            LoopExpression node)
        {
            // TODO: handle the various loops here?
            Visit(node.Body);
            return base.VisitLoop(node);
        }

        protected override Expression VisitMember(
            MemberExpression node)
        {
            ExpressionMetadata meta;

            _metadata.TryGetValue(node.NodeType, out meta);

            Visit(node.Expression, meta);
            _writer.Write(meta.Operator);

            PropertyInfo pi = node.Member as PropertyInfo;

            if (pi != null)
            {
                _writer.Write(pi.Name);
                return node;
            }

            FieldInfo fi = node.Member as FieldInfo;

            if (fi != null)
            {
                _writer.Write(fi.Name);
                return node;
            }

            EventInfo ei = node.Member as EventInfo;

            if (ei != null)
            {
                _writer.Write(ei.Name);
                return node;
            }

            MethodInfo mi = node.Member as MethodInfo;

            if (mi != null)
            {
                _writer.Write(mi.Name);
                return node;
            }

            return node;
        }

        protected override Expression VisitBinary(
            BinaryExpression node)
        {
            ExpressionMetadata meta;

            _metadata.TryGetValue(node.NodeType, out meta);

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
            _writer.Write("{0}{1}", meta.Operator, node.TypeOperand.GetTypeName());
            return node;
        }

        protected override Expression VisitUnary(
            UnaryExpression node)
        {
            ExpressionMetadata meta;

            _metadata.TryGetValue(node.NodeType, out meta);

            if (!meta.IsPostfix)
            {
                if (node.NodeType == ExpressionType.Convert)
                    _writer.Write("({0})", node.Type.GetTypeName());
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
            _writer.Write("{0}:", node.Name);
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
                _writer.Write("{0} {1}", p.Type.GetTypeName(ShortenNamesOfGeneratedClasses), p.Name);
            }
            _writer.Write(") => ");

            if (node.NodeType != ExpressionType.Loop   &&
                (node.NodeType != ExpressionType.Conditional || node.Type != typeof(void)))
            {
                Visit(node.Body);
                return node;
            }
            else
            {
                _writer.WriteLine();
                _writer.WriteLine('{');
                Indent();
                Visit(node.Body);
                Semicolon();
                Unindent();
                _writer.Write('}');
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
            _writer.Write("default({0})", node.Type.GetTypeName(ShortenNamesOfGeneratedClasses));
            return node;
        }

        protected override Expression VisitDynamic(
            DynamicExpression node)
        {
            _writer.Write("/* TODO: dynamic expression() */");
            return node;
        }

        protected override Expression VisitBlock(
            BlockExpression node)
        {
            _writer.WriteLine();
            _writer.Write('{');
            Indent();

            foreach (var v in node.Variables)
            {
                _writer.WriteLine();
                _writer.Write(v.Type.GetTypeName(ShortenNamesOfGeneratedClasses));
                if (v.Type.IsArray)
                    _writer.Write("[]");
                _writer.Write(" {0}", v.Name);
                Semicolon();
            }

            if (node.Variables.Any())
                _writer.WriteLine();

            foreach (var e in node.Expressions)
                if (e.NodeType != ExpressionType.DebugInfo || DumpDebugInfo)
                {
                    _writer.WriteLine();
                    Visit(e);
                    Semicolon();
                }

            _writer.WriteLine();

            Unindent();
            _writer.Write('}');
            NeedsSemicolon = false;

            return node;
        }

        protected override Expression VisitConditional(
            ConditionalExpression node)
        {
            ExpressionMetadata meta;

            _metadata.TryGetValue(node.NodeType, out meta);

            if (node.Type != typeof(void))
            {
                // test ? a : b
                Visit(node.Test, meta);
                _writer.Write(" ? ");
                Visit(node.IfTrue, meta);
                _writer.Write(" : ");
                Visit(node.IfFalse, meta);
            }
            else
            {
                // if (test) a; else b;
                _writer.Write("if (");
                Visit(node.Test);
                _writer.Write(')');
                if (node.IfTrue.NodeType != ExpressionType.Block)
                {
                    _writer.WriteLine();
                    Indent();
                    Visit(node.IfTrue);
                    Unindent();
                }
                else
                    Visit(node.IfTrue);
                if (node.IfFalse.NodeType != ExpressionType.Default  ||  node.IfFalse.Type != typeof(void))
                {
                    Semicolon();
                    _writer.WriteLine();
                    _writer.Write("else");
                    if (node.IfFalse.NodeType != ExpressionType.Block)
                    {
                        _writer.WriteLine();
                        Indent();
                        Visit(node.IfFalse);
                        Unindent();
                    }
                    else
                        Visit(node.IfFalse);
                }
            }
            return node;
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
                    _writer.Write(node.Method.DeclaringType.GetTypeName(ShortenNamesOfGeneratedClasses));
            }
            else
                Visit(node.Object, node);
            _writer.Write(".{0}(", node.Method.Name);

            var first = true;

            for (; i<node.Arguments.Count; i++)
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
                _writer.Write(" ({0}", node.Test.GetTypeName(ShortenNamesOfGeneratedClasses));
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
    }
}
