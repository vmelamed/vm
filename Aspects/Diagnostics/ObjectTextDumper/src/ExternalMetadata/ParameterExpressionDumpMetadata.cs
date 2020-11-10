#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

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
