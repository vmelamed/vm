#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    [Dump(Enumerate = ShouldDump.Dump)]
    public abstract class ValidationResultsDumpMetadata
    {
        [Dump(0)]
        public object? IsValid { get; set; }

        [Dump(1)]
        public object? Count { get; set; }
    }
}
