#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ArgumentValidationExceptionDumpMetadata
    {
        [Dump(0, Enumerate = ShouldDump.Dump)]
        public object? ValidationResults { get; set; }
    }
}
