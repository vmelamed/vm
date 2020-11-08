#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    [Dump(DefaultProperty = "Name")]
    public abstract class ParameterExpressionDumpMetadata
    {
        [Dump(0)]
        public object? Name { get; set; }

        [Dump(1)]
        public object? IsByRef { get; set; }
    }
}
