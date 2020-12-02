using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    class ScriptMemberDumper : IMemberDumper
    {
        public ScriptMemberDumper(DumpState dumpState)
        {
            DumpState = dumpState;

            Debug.Assert(dumpState.DumpScript is not null);

            // shortcuts:
            Script        = DumpState.DumpScript!;
        }

        DumpState DumpState { get; }
        // shortcuts:
        DumpScript Script { get; }
        DumpAttribute DumpAttribute => DumpState.CurrentDumpAttribute!;
        MemberInfo MemberInfo => DumpState.CurrentMember!;

        public void DumpSeenAlready(string reference) => Script.AddDumpSeenAlready(reference);

        public void DumpType() => Script.AddDumpType();

        public void DumpExpressionCSharpText(string cSharpText) => Script.AddDumpExpressionText(cSharpText);

        public void IncrementMaxDepth() => Script.AddIncrementMaxDepth();

        public void DecrementMaxDepth() => Script.AddDecrementMaxDepth();

        public void Indent() => Script.AddIndent();

        public void Unindent() => Script.AddUnindent();

        public void DumpDelegate() => Script.AddDumpedDelegate();

        public void DumpedMemberInfo() => Script.AddDumpedMemberInfo();

        public void DumpProperty(
            object? value,
            Type type)
        {
            var dontDumpNulls = DumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                                DumpAttribute.DumpNullValues==ShouldDump.Default && DumpState.DumpNullValues==ShouldDump.Skip;

            // write the property header
            Script.BeginDumpProperty(MemberInfo, DumpAttribute);

            if (!DumpState.DumpedPropertyCustom(value, type))   // dump the property value using caller's customization (see ValueFormat="ToString", DumpClass, DumpMethod) if any.
                Script.AddDumpPropertyOrCollectionValue(MemberInfo, DumpAttribute);

            Script.EndDumpProperty(MemberInfo, dontDumpNulls);
        }

        public bool DumpExpando(
            IEnumerable _,
            DumpAttribute dumpAttribute)
        {
            Script.AddDumpedExpando(dumpAttribute);
            return true;
        }

        public bool DumpDictionary(
            object sequence,
            DumpAttribute dumpAttribute,
            MemberInfo? mi)
        {
            if (!sequence.GetType().DictionaryTypeArguments().isDictionary)
                return false;

            if (mi is null)
                Script.AddDumpedDictionary(dumpAttribute);
            else
                Script.AddDumpedDictionary(mi, dumpAttribute);
            return true;
        }

        public bool DumpSequence(
            IEnumerable sequence,
            DumpAttribute dumpAttribute,
            MemberInfo? mi,
            bool _) =>
            (mi is not null
                ? Script.AddDumpedCollection(mi, dumpAttribute)
                : Script.AddDumpedCollection(dumpAttribute)) is not null;

        public void DumpToString(object _) => Script.AddCustomDumpPropertyOrField(MemberInfo, null);

        public bool CustomDumpProperty(object _, MethodInfo dumpMethod)
        {
            Script.AddCustomDumpPropertyOrField(MemberInfo, dumpMethod);
            return true;
        }

        public void Write(string message) => Script.AddWrite(message);
    }
}
