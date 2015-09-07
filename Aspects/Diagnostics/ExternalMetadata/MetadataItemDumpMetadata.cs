#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class MetadataItemDumpMetadata
    {
        [Dump(0)]
        public object BuiltInTypeKind;

        [Dump(1)]
        public object Documentation;

        [Dump(2, RecurseDump=ShouldDump.Skip)]
        public object MetadataProperties;
    }
}
