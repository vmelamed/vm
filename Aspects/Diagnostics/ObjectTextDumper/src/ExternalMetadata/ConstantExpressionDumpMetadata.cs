#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ConstantExpressionDumpMetadata
    {
        [Dump(0)]
        public object? Value { get; set; }
    }
}
