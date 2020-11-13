using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Threading;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class CSharpDumpExpression : IDisposable
    {
        readonly StringWriter _textWriter = new();
        readonly DumpTextWriter _writer;

        int Indent() => ++_writer.Indent;

        int Unindent() => --_writer.Indent;

        bool NeedsSemicolon { get; set; } = true;

        bool PrintedText { get; set; } = true;

        public bool ShortenNamesOfGeneratedClasses { get; set; }

        public bool DumpDebugInfo { get; set; }

        public CSharpDumpExpression()
        {
            _writer = new DumpTextWriter(_textWriter)
            {
                Indent     = 0,
                IndentSize = 4,
            };
        }

        public string DumpText => !IsDisposed ? _textWriter.GetStringBuilder().ToString() : "";

        struct ExpressionMetadata
        {
            public int Weight;
            public bool LeftAssociative;
            public bool IsPostfix;
            public bool IsChecked;
            public string Operator;
        }

        Expression Visit(
            Expression node,
            Expression parentNode)
        {
            _metadata.TryGetValue(parentNode.NodeType, out var parentMeta);

            return Visit(node, parentMeta);
        }

        Expression Visit(
            Expression node,
            ExpressionMetadata parentMeta)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            _metadata.TryGetValue(node.NodeType, out var meta);

            if (meta.Weight > parentMeta.Weight ||
                meta.Weight == parentMeta.Weight && meta.LeftAssociative == parentMeta.LeftAssociative)
                Visit(node);
            else
            {
                _writer.Write('(');
                Visit(node);
                _writer.Write(')');
            }

            return node;
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

        static readonly IReadOnlyDictionary<ExpressionType, ExpressionMetadata> _metadata = new ReadOnlyDictionary<ExpressionType, ExpressionMetadata>(
            new Dictionary<ExpressionType, ExpressionMetadata>
            {
                [ExpressionType.Constant]               = new ExpressionMetadata { Weight = 15, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "" },
                [ExpressionType.Parameter]              = new ExpressionMetadata { Weight = 15, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "" },

                [ExpressionType.MemberAccess]           = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true, IsChecked = false, Operator = "." },
                [ExpressionType.Invoke]                 = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true, IsChecked = false, Operator = "" },
                [ExpressionType.Call]                   = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true, IsChecked = false, Operator = "" },
                [ExpressionType.Index]                  = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true, IsChecked = false, Operator = "" },
                [ExpressionType.ArrayIndex]             = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true, IsChecked = false, Operator = "" },
                [ExpressionType.PostIncrementAssign]    = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true, IsChecked = false, Operator = "++" },
                [ExpressionType.PostDecrementAssign]    = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true, IsChecked = false, Operator = "--" },
                [ExpressionType.New]                    = new ExpressionMetadata { Weight = 14, LeftAssociative =  true, IsPostfix = true, IsChecked = false, Operator = "new " },

                [ExpressionType.NegateChecked]          = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked =  true, Operator = "-" },
                [ExpressionType.Negate]                 = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "-" },
                [ExpressionType.UnaryPlus]              = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "+" },
                [ExpressionType.Not]                    = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "!" },
                [ExpressionType.OnesComplement]         = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "~" },
                [ExpressionType.PreIncrementAssign]     = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "++" },
                [ExpressionType.PreDecrementAssign]     = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "--" },
                [ExpressionType.ConvertChecked]         = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked =  true, Operator = "" },
                [ExpressionType.Convert]                = new ExpressionMetadata { Weight = 13, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "" },

                [ExpressionType.MultiplyChecked]        = new ExpressionMetadata { Weight = 12, LeftAssociative =  true, IsPostfix = false, IsChecked =  true, Operator = " * " },
                [ExpressionType.Multiply]               = new ExpressionMetadata { Weight = 12, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " * " },
                [ExpressionType.Divide]                 = new ExpressionMetadata { Weight = 12, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " / " },
                [ExpressionType.Modulo]                 = new ExpressionMetadata { Weight = 12, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " % " },

                [ExpressionType.AddChecked]             = new ExpressionMetadata { Weight = 11, LeftAssociative =  true, IsPostfix = false, IsChecked =  true, Operator = " + " },
                [ExpressionType.Add]                    = new ExpressionMetadata { Weight = 11, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " + " },
                [ExpressionType.SubtractChecked]        = new ExpressionMetadata { Weight = 11, LeftAssociative =  true, IsPostfix = false, IsChecked =  true, Operator = " - " },
                [ExpressionType.Subtract]               = new ExpressionMetadata { Weight = 11, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " - " },

                [ExpressionType.LeftShift]              = new ExpressionMetadata { Weight = 10, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " << " },
                [ExpressionType.RightShift]             = new ExpressionMetadata { Weight = 10, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " >> " },

                [ExpressionType.GreaterThan]            = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " > " },
                [ExpressionType.GreaterThanOrEqual]     = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " >= " },
                [ExpressionType.LessThan]               = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " < " },
                [ExpressionType.LessThanOrEqual]        = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " <= " },
                [ExpressionType.TypeAs]                 = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " as " },
                [ExpressionType.TypeIs]                 = new ExpressionMetadata { Weight =  9, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " is " },

                [ExpressionType.Equal]                  = new ExpressionMetadata { Weight =  8, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " == " },
                [ExpressionType.NotEqual]               = new ExpressionMetadata { Weight =  8, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " != " },

                [ExpressionType.And]                    = new ExpressionMetadata { Weight =  7, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " & " },

                [ExpressionType.ExclusiveOr]            = new ExpressionMetadata { Weight =  6, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " ^ " },

                [ExpressionType.Or]                     = new ExpressionMetadata { Weight =  5, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " | " },

                [ExpressionType.AndAlso]                = new ExpressionMetadata { Weight =  4, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " && " },

                [ExpressionType.OrElse]                 = new ExpressionMetadata { Weight =  3, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = " || " },

                [ExpressionType.Conditional]            = new ExpressionMetadata { Weight =  2, LeftAssociative =  true, IsPostfix = false, IsChecked = false, Operator = "" },

                [ExpressionType.Assign]                 = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " = " },
                [ExpressionType.MultiplyAssignChecked]  = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked =  true, Operator = " *= " },
                [ExpressionType.MultiplyAssign]         = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " *= " },
                [ExpressionType.DivideAssign]           = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " /= " },
                [ExpressionType.ModuloAssign]           = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " %= " },
                [ExpressionType.AddAssign]              = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " += " },
                [ExpressionType.AddAssignChecked]       = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked =  true, Operator = " += " },
                [ExpressionType.SubtractAssign]         = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " -= " },
                [ExpressionType.SubtractAssignChecked]  = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked =  true, Operator = " -= " },
                [ExpressionType.LeftShiftAssign]        = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " <<= " },
                [ExpressionType.RightShiftAssign]       = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " >>= " },
                [ExpressionType.AndAssign]              = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " &= " },
                [ExpressionType.OrAssign]               = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " |= " },
                [ExpressionType.ExclusiveOrAssign]      = new ExpressionMetadata { Weight =  1, LeftAssociative = false, IsPostfix = false, IsChecked = false, Operator = " ^= " },
            });

        static readonly IReadOnlyDictionary<Type, Action<TextWriter, object>> _dumpBasicValues = new ReadOnlyDictionary<Type, Action<TextWriter, object>>(
            new Dictionary<Type, Action<TextWriter, object>>
            {
                [typeof(DBNull)]         = (w, _) => w.Write("DBNull.Value"),
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
    }
}
