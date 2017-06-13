using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    partial class DumpScript
    {
        public DumpScript AddWriteLine()
            => Add(WriteLine());

        public DumpScript AddWrite(
            Expression text)
            => Add(Write(text));

        public DumpScript AddWrite(
            string text)
            => Add(Write(text));

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1)
            => Add(Write(format, parameter1));

        public DumpScript AddWrite(
            string format,
            Expression parameter1)
            => Add(Write(format, parameter1));

        public DumpScript AddWrite(
            string format,
            object parameter1)
            => Add(Write(format, parameter1));

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1,
            Expression parameter2)
            => Add(Write(format, parameter1, parameter2));

        public DumpScript AddWrite(
            string format,
            Expression parameter1,
            Expression parameter2)
            => Add(Write(format, parameter1, parameter2));

        public DumpScript AddWrite(
            string format,
            object parameter1,
            object parameter2)
            => Add(Write(format, parameter1, parameter2));

        public DumpScript AddWrite(
            Expression format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3)
            => Add(Write(format, parameter1, parameter2, parameter3));

        public DumpScript AddWrite(
            string format,
            Expression parameter1,
            Expression parameter2,
            Expression parameter3)
            => Add(Write(format, parameter1, parameter2, parameter3));

        public DumpScript AddWrite(
            string format,
            object parameter1,
            object parameter2,
            object parameter3)
            => Add(Write(format, parameter1, parameter2, parameter3));
    }
}
