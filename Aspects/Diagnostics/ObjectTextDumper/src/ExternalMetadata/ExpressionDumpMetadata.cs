#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ExpressionDumpMetadata
    {
        [Dump(0)]
        public object? NodeType { get; set; }

        [Dump(1)]
        public object? Type { get; set; }

        [Dump(false)]
        public object? TypeCore { get; set; }

        [Dump(3, DumpNullValues = ShouldDump.Skip)]
        public object? Name { get; set; }

        [Dump(int.MinValue+1)]
        public object? CanReduce { get; set; }

        [Dump(false)]
        public object? DebugView { get; set; }
    }
}
