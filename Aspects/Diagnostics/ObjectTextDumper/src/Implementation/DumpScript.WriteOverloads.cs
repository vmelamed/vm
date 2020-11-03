using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class DumpScript
    {
        static readonly MethodInfo _miWriteLine0 = typeof(TextWriter).GetMethod(nameof(TextWriter.WriteLine), BindingFlags.Public|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly MethodInfo _miWrite1     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), }, null);
        static readonly MethodInfo _miWrite2     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object), }, null);
        static readonly MethodInfo _miWrite3     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object), typeof(object), }, null);
        static readonly MethodInfo _miWrite4     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object), typeof(object), typeof(object), }, null);

        Expression WriteLine()
            => Expression.Call(
                        _writer,
                        _miWriteLine0);

        Expression Write(
            Expression text)
            => Expression.Call(
                        _writer,
                        _miWrite1,
                        text);

        Expression Write(
            string text)
            => Write(Expression.Constant(text));

        Expression Write(
            Expression format,
            Expression parameter1)
            => Expression.Call(
                        _writer,
                        _miWrite2,
                        format,
                        parameter1);

        Expression Write(
            string format,
            Expression parameter1)
            => Expression.Call(
                        _writer,
                        _miWrite2,
                        Expression.Constant(format),
                        parameter1);

        Expression Write(
            string format,
            object parameter1)
            => Write(
                    Expression.Constant(format),
                    Expression.Constant(parameter1));

        Expression Write(
            Expression format,
            Expression parameter1,
            Expression parameter2)
            => Expression.Call(
                        _writer,
                        _miWrite3,
                        format,
                        parameter1,
                        parameter2);

        Expression Write(
            string format,
            Expression parameter1,
            Expression parameter2)
            => Expression.Call(
                        _writer,
                        _miWrite3,
                        Expression.Constant(format),
                        parameter1,
                        parameter2);

        Expression Write(
            string format,
            object parameter1,
            object parameter2)
            => Write(
                    Expression.Constant(format),
                    Expression.Constant(parameter1),
                    Expression.Constant(parameter2));

        Expression Write(
            Expression format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3)
            => Expression.Call(
                        _writer,
                        _miWrite4,
                        format,
                        parameter1,
                        parameter2,
                        parameter3);

        Expression Write(
            string format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3)
            => Write(
                    Expression.Constant(format),
                    parameter1,
                    parameter2,
                    parameter3);

        Expression Write(
            string format,
            object parameter1,
            object parameter2,
            object parameter3)
            => Write(
                    Expression.Constant(format),
                    Expression.Constant(parameter1),
                    Expression.Constant(parameter2),
                    Expression.Constant(parameter3));
    }
}
