#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

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
