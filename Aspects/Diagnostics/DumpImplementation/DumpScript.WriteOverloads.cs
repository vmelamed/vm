using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    partial class DumpScript
    {
        static readonly MethodInfo _miWriteLine0 = typeof(TextWriter).GetMethod(nameof(TextWriter.WriteLine), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miWrite1     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), }, null);
        static readonly MethodInfo _miWrite2     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object), }, null);
        static readonly MethodInfo _miWrite3     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object), typeof(object), }, null);
        static readonly MethodInfo _miWrite4     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object), typeof(object), typeof(object), }, null);
        static readonly MethodInfo _miWriteN     = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object[]) }, null);

        public Expression WriteLine()
            => Expression.Call(
                        _writer,
                        _miWriteLine0);

        public DumpScript AddWriteLine()
            => Add(WriteLine());

        // -----------------------------

        public Expression Write(
            Expression text)
            => Expression.Call(
                        _writer,
                        _miWrite1,
                        text);

        public DumpScript AddWrite(
            Expression text)
            => Add(Write(text));

        //------------------------------

        public Expression Write(
            string text)
            => Write(Expression.Constant(text));

        public DumpScript AddWrite(
            string text)
            => Add(Write(text));

        //------------------------------

        public Expression Write(
            Expression format,
            Expression parameter1)
            => Expression.Call(
                        _writer,
                        _miWrite2,
                        format,
                        parameter1);

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1)
            => Add(Write(format, parameter1));

        //------------------------------

        public Expression Write(
            string format,
            Expression parameter1)
            => Expression.Call(
                        _writer,
                        _miWrite2,
                        Expression.Constant(format),
                        parameter1);

        public DumpScript AddWrite(
            string format,
            Expression parameter1)
            => Add(Write(format, parameter1));

        //-----------------------------

        public Expression Write(
            string format,
            object parameter1)
            => Write(
                    Expression.Constant(format),
                    Expression.Constant(parameter1));

        public DumpScript AddWrite(
            string format,
            object parameter1)
            => Add(Write(format, parameter1));

        //------------------------------

        public Expression Write(
            Expression format,
            Expression parameter1,
            Expression parameter2)
            => Expression.Call(
                        _writer,
                        _miWrite3,
                        format,
                        parameter1,
                        parameter2);

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1,
            Expression parameter2)
            => Add(Write(format, parameter1, parameter2));

        //------------------------------

        public Expression Write(
            string format,
            Expression parameter1,
            Expression parameter2)
            => Expression.Call(
                        _writer,
                        _miWrite3,
                        Expression.Constant(format),
                        parameter1,
                        parameter2);

        public DumpScript AddWrite(
            string format,
            Expression parameter1,
            Expression parameter2)
            => Add(Write(format, parameter1, parameter2));

        // ----------------------------

        public Expression Write(
            string format,
            object parameter1,
            object parameter2)
            => Write(
                    Expression.Constant(format),
                    Expression.Constant(parameter1),
                    Expression.Constant(parameter2));

        public DumpScript AddWrite(
            string format,
            object parameter1,
            object parameter2)
            => Add(Write(format, parameter1, parameter2));

        //------------------------------

        public Expression Write(
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

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3)
            => Add(Write(format, parameter1, parameter2, parameter3));

        // -----------------------------

        public Expression Write(
            string format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3)
            => Write(
                    Expression.Constant(format),
                    parameter1,
                    parameter2,
                    parameter3);

        public DumpScript AddWrite(
            string format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3)
            => Add(Write(format, parameter1, parameter2, parameter3));

        // -------------------------------

        public Expression Write(
            string format,
            object parameter1,
            object parameter2,
            object parameter3)
            => Write(
                    Expression.Constant(format),
                    Expression.Constant(parameter1),
                    Expression.Constant(parameter2),
                    Expression.Constant(parameter3));

        public DumpScript AddWrite(
            string format,
            object parameter1,
            object parameter2,
            object parameter3)
            => Add(Write(format, parameter1, parameter2, parameter3));

        // ----------------------------

        public Expression Write(
            Expression format,
            params Expression[] parameters)
        {
            var allParameters = new Expression[parameters.Length+1];

            allParameters[0] = format;
            for (var i = 0; i<parameters.Length; i++)
                allParameters[i+1] = parameters[i];

            return Expression.Call(
                        _writer,
                        _miWriteN,
                        allParameters);
        }

        public DumpScript AddWrite(
            Expression format,
            params Expression[] parameters)
            => Add(Write(format, parameters));

        // -----------------------------

        public Expression Write(
            string format,
            params Expression[] parameters)
            => Write(
                    Expression.Constant(format),
                    parameters);

        public DumpScript AddWrite(
            string format,
            params Expression[] parameters)
            => Add(Write(format, parameters));

        // -------------------------------

        public Expression Write(
            string format,
            params object[] parameters)
        {
            var allParameters = new Expression[parameters.Length+1];

            allParameters[0] = Expression.Constant(format);
            for (var i = 0; i<parameters.Length; i++)
                allParameters[i+1] = Expression.Constant(parameters[i]);

            return Expression.Call(
                        _writer,
                        _miWriteN,
                        allParameters);
        }

        public DumpScript AddWrite(
            string format,
            params object[] parameters)
        => Add(Write(format, parameters));
    }
}
