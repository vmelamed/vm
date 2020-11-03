#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class TaskGenericDumpMetadata
    {
        [Dump(false)]
        public object Result { get; set; }
    }
}
