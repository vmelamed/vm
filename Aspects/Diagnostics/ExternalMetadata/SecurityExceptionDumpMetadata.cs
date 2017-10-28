#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class SecurityExceptionDumpMetadata
    {
        [Dump(RecurseDump = ShouldDump.Skip, DefaultProperty = "CodeBase")]
        public object FailedAssemblyInfo { get; set; }
    }
}
