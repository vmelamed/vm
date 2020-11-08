using System;
using System.Collections;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    interface IDumpState
    {
        void DumpSeenAlready();
        void DumpType();
        void DumpExpressionCSharpText(string cSharpText);
        void IncrementMaxDepth();
        void DecrementMaxDepth();
        void Indent();
        void Unindent();
        void DumpDelegate();
        void DumpedMemberInfo();
        void DumpProperty(object? value, Type type);
        bool DumpDictionary(object sequence, DumpAttribute dumpAttribute);
        bool DumpSequence(IEnumerable sequence, DumpAttribute dumpAttribute, bool newLineForCustom = false);
        void DumpToString(object value);
        bool CustomDumpProperty(object value, MethodInfo dumpMethod);
        void Write(string message);
    }
}
