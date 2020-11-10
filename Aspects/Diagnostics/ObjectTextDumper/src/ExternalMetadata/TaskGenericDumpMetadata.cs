#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class TaskGenericDumpMetadata
    {
        [Dump(false)]
        public object? Result { get; set; }
    }
}
