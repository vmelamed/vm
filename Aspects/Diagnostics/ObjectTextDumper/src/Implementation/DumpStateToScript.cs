using System;
using System.Collections;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    class DumpStateToScript : IDumpState
    {
        public DumpStateToScript(DumpState dumpState)
        {
            DumpState = dumpState;

            // shortcuts:
            Script        = dumpState.DumpScript!;
            DumpAttribute = dumpState.CurrentDumpAttribute!;
            MemberInfo    = dumpState.CurrentMember!;
        }

        DumpState DumpState { get; }
        // shortcuts:
        DumpScript Script { get; }
        DumpAttribute DumpAttribute { get; }
        MemberInfo MemberInfo { get; }

        public void DumpSeenAlready() => Script.AddDumpSeenAlready();

        public void DumpType() => Script.AddDumpType();

        public void DumpExpressionCSharpText(string cSharpText) => Script.AddDumpExpressionText(cSharpText);

        public void IncrementMaxDepth() => Script.AddIncrementMaxDepth();

        public void DecrementMaxDepth() => Script.AddDecrementMaxDepth();

        public void Indent() => Script.AddIndent();

        public void Unindent() => Script.AddUnindent();

        public void DumpDelegate() => Script.AddDumpedDelegate();

        public void DumpedMemberInfo() => Script.AddDumpedMemberInfo();

        public void DumpProperty(object? value, Type type)
        {
            var dontDumpNulls = DumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                                DumpAttribute.DumpNullValues==ShouldDump.Default && DumpState.DumpNullValues==ShouldDump.Skip;

            // write the property header
            Script.BeginDumpProperty(MemberInfo, DumpAttribute);

            if (!DumpState.DumpedPropertyCustom(value, type))                                             // dump the property value using caller's customization (see ValueFormat="ToString", DumpClass, DumpMethod) if any.
                Script.AddDumpPropertyOrCollectionValue(MemberInfo, DumpAttribute);

            Script.EndDumpProperty(MemberInfo, dontDumpNulls);
        }

        public bool DumpDictionary(object sequence, DumpAttribute dumpAttribute)
        {
            if (sequence.GetType().DictionaryTypeArguments().keyType == typeof(void))
                return false;

            Script.AddDumpedDictionary(dumpAttribute);
            return true;
        }

        public bool DumpSequence(IEnumerable sequence, DumpAttribute dumpAttribute, bool _)
        {
            Script.AddDumpedCollection(dumpAttribute);
            return true;
        }

        public void DumpToString(object value) => Script.AddCustomDumpPropertyOrField(MemberInfo, null);

        public bool CustomDumpProperty(object value, MethodInfo dumpMethod)
        {
            Script.AddCustomDumpPropertyOrField(MemberInfo, dumpMethod);
            return true;
        }

        public void Write(string message) => Script.AddWrite(message);
    }
}
