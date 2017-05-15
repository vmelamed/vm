using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    partial class DumpScript
    {
        public DumpScript AddWriteLine(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(WriteLine(), callerFile, callerLine);

        public DumpScript AddWrite(
            Expression text,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(text), callerFile, callerLine);

        public DumpScript AddWrite(
            string text,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(text), callerFile, callerLine);

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1), callerFile, callerLine);

        public DumpScript AddWrite(
            string format,
            Expression parameter1,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1), callerFile, callerLine);

        public DumpScript AddWrite(
            string format,
            object parameter1,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1), callerFile, callerLine);

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1,
            Expression parameter2,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1, parameter2), callerFile, callerLine);

        public DumpScript AddWrite(
            string format,
            Expression parameter1,
            Expression parameter2,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1, parameter2), callerFile, callerLine);

        public DumpScript AddWrite(
            string format,
            object parameter1,
            object parameter2,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1, parameter2), callerFile, callerLine);

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1, parameter2, parameter3), callerFile, callerLine);

        public DumpScript AddWrite(
            string format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1, parameter2, parameter3), callerFile, callerLine);

        public DumpScript AddWrite(
            string format,
            object parameter1,
            object parameter2,
            object parameter3,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Write(format, parameter1, parameter2, parameter3), callerFile, callerLine);
    }
}
