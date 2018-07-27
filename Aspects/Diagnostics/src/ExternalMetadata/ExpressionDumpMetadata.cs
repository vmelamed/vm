using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ExpressionDumpMetadata
    {
        [Dump(0)]
        public object NodeType { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "see ExpressionDump")]
        [Dump(1)]
        public object Type { get; set; }

        [Dump(2, DumpNullValues = ShouldDump.Skip)]
        public object Name { get; set; }

        [Dump(int.MinValue)]
        public object CanReduce { get; set; }

        [Dump(false)]
        public object DebugView { get; set; }
    }
}
