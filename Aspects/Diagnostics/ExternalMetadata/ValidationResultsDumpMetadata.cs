#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    [Dump(Enumerate=ShouldDump.Dump)]
    public abstract class ValidationResultsDumpMetadata
    {
        [Dump(0)]
        public object IsValid;

        [Dump(1)]
        public object Count;
    }
}
