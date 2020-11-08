#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class MetadataItemDumpMetadata
    {
        [Dump(0)]
        public object? BuiltInTypeKind { get; set; }

        [Dump(1)]
        public object? Documentation { get; set; }

        [Dump(2, RecurseDump = ShouldDump.Skip)]
        public object? MetadataProperties { get; set; }
    }
}
