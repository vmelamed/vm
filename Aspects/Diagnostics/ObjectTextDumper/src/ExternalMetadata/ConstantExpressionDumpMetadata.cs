#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ConstantExpressionDumpMetadata
    {
        [Dump(0)]
        public object? Value { get; set; }
    }
}
