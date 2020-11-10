#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ArgumentExceptionDumpMetadata
    {
        [Dump(false)]
        public object? Message { get; set; }

        [Dump(0)]
        public object? ParamName { get; set; }
    }
}
