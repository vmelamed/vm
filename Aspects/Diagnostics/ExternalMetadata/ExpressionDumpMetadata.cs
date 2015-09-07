using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ExpressionDumpMetadata
    {
        [Dump(0)]
        public object NodeType;

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification="see ExpressionDump")]
        [Dump(-1)]
        public object Type;

        [Dump(int.MinValue)]
        public object CanReduce;

        [Dump(false)]
        public object DebugView;
    }
}
