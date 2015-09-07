#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ArgumentValidationExceptionDumpMetadata
    {
        [Dump(0, Enumerate=ShouldDump.Dump)]
        public object ValidationResults;
    }
}
