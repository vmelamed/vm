using System;
using System.Collections;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    interface IMemberDumper
    {
        void DumpSeenAlready(string reference);
        void DumpType();
        void DumpExpressionCSharpText(string cSharpText);
        void IncrementMaxDepth();
        void DecrementMaxDepth();
        void Indent();
        void Unindent();
        void DumpDelegate();
        void DumpedMemberInfo();
        void DumpProperty(object? value, Type type);
        bool DumpExpando(IEnumerable sequence, DumpAttribute dumpAttribute);
        bool DumpDictionary(object sequence, DumpAttribute dumpAttribute, MemberInfo? mi);
        bool DumpSequence(IEnumerable sequence, DumpAttribute dumpAttribute, MemberInfo? _, bool newLineForCustom = false);
        void DumpToString(object value);
        bool CustomDumpProperty(object value, MethodInfo dumpMethod);
        void Write(string message);
    }
}
