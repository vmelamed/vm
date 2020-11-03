#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class UpdateExceptionDumpMetadata
    {
        [Dump(RecurseDump = ShouldDump.Skip)]
        public object StateEntries { get; set; }
    }
}
