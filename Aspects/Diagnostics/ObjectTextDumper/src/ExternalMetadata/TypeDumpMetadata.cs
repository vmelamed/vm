#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    [Dump(RecurseDump = ShouldDump.Skip, DefaultProperty = "Name")]
    public abstract class TypeDumpMetadata
    {
        public object? Name { get; set; }
    }
}
