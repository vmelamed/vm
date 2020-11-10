#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class SecurityExceptionDumpMetadata
    {
        [Dump(RecurseDump = ShouldDump.Skip, DefaultProperty = "CodeBase")]
        public object? FailedAssemblyInfo { get; set; }
    }
}
