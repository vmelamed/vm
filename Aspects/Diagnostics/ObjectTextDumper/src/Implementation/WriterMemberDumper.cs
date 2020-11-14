using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    class WriterMemberDumper : IMemberDumper
    {
        public WriterMemberDumper(DumpState dumpState)
        {
            DumpState = dumpState;
            // shortcuts:
            Dumper        = dumpState.Dumper;
            Writer        = dumpState.Dumper.Writer;
            Instance      = dumpState.Instance;
            InstanceType  = dumpState.InstanceType;
        }

        DumpState DumpState { get; }
        // shortcuts:
        ObjectTextDumper Dumper { get; }
        TextWriter Writer { get; }
        object Instance { get; }
        Type InstanceType { get; }
        DumpAttribute DumpAttribute => DumpState.CurrentDumpAttribute!;
        MemberInfo MemberInfo => DumpState.CurrentMember!;

        public void DumpSeenAlready(string reference)
        {
            Writer.Write(
                DumpFormat.CyclicalReference,
                InstanceType.GetTypeName(),
                reference);
        }

        public void DumpType()
        {
            Writer.Write(
                DumpFormat.Type,
                InstanceType.GetTypeName(),
                InstanceType.Namespace,
                InstanceType.AssemblyQualifiedName);
        }

        public void DumpExpressionCSharpText(string cSharpText)
        {
            Indent();
            Writer.WriteLine();
            Writer.Write(DumpFormat.CSharpDumpLabel);
            Indent();
            Writer.WriteLine();
            Writer.Write(cSharpText);
            Unindent();
            Unindent();
        }

        public void IncrementMaxDepth() => Dumper.IncrementMaxDepth();

        public void DecrementMaxDepth() => Dumper.DecrementMaxDepth();

        public void Indent() => Dumper.Indent();

        public void Unindent() => Dumper.Unindent();

        public void DumpDelegate() => Writer.Dumped((Delegate)Instance);

        public void DumpedMemberInfo() => Writer.Dumped(Instance as MemberInfo);

        public void DumpProperty(
            object? value,
            Type type)
        {
            // should we dump a null value of the current property
            if (value == null ||
                DumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                DumpAttribute.DumpNullValues==ShouldDump.Default && DumpState.DumpNullValues==ShouldDump.Skip)
                return;

            // write the property header
            Dumper.Writer.WriteLine();
            Dumper.Writer.Write(
                DumpAttribute.LabelFormat,
                MemberInfo.Name);

            if (!(DumpState.DumpedPropertyCustom(value, type)              ||   // dump the property value using caller's customization (see ValueFormat="ToString", DumpClass, DumpMethod) if any.
                  Dumper.Writer.DumpedBasicValue(value, DumpAttribute)     ||
                  Dumper.Writer.DumpedBasicNullable(value, DumpAttribute)  ||
                  Dumper.Writer.Dumped(value as Delegate)                  ||
                  Dumper.Writer.Dumped(value as MemberInfo)                ||
                  DumpState.DumpedCollection(value, MemberInfo, DumpAttribute)))
            {
                // dump a property representing an associated class or struct object
                var currentPropertyDumpAttribute = !DumpAttribute.IsDefaultAttribute() ? DumpAttribute : null;

                Dumper.DumpObject(value, null, currentPropertyDumpAttribute, DumpState);
            }
        }

        public bool DumpExpando(
            IEnumerable sequence,
            DumpAttribute dumpAttribute) =>
            Dumper.Writer.DumpedExpando(
                            sequence,
                            dumpAttribute,
                            o => Dumper.DumpObject(o, null, null, DumpState),
                            Dumper.Indent,
                            Dumper.Unindent);

        public bool DumpDictionary(
            object sequence,
            DumpAttribute dumpAttribute,
            MemberInfo? _) =>
            Dumper.Writer.DumpedDictionary(
                sequence,
                dumpAttribute,
                o => Dumper.DumpObject(o, null, null, DumpState),
                Indent,
                Unindent);

        public bool DumpSequence(
            IEnumerable sequence,
            DumpAttribute dumpAttribute,
            MemberInfo? _,
            bool newLineForCustom = false)
        {
            Dumper.Writer.WriteLine();
            return Dumper.Writer.DumpedCollection(
                                    sequence,
                                    dumpAttribute,
                                    o => Dumper.DumpObject(o, null, null, DumpState),
                                    Dumper.Indent,
                                    Dumper.Unindent);
        }

        public void DumpToString(object value)
        {
            Dumper.Writer.Write(value.ToString());
        }

        public bool CustomDumpProperty(
            object value,
            MethodInfo dumpMethod)
        {
            var text = (dumpMethod.IsStatic
                                    ? dumpMethod.Invoke(null, new object[] { value })
                                    : dumpMethod.Invoke(value, null)) as string;

            if (text is not null)
            {
                Dumper.Writer.Write(text);
                return true;
            }

            return false;
        }

        public void Write(string message) => Dumper.Writer.Write(message);
    }
}
