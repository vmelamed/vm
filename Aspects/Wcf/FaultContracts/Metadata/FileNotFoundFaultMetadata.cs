using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class FileNotFoundFaultMetadata
    {
        [Dump(0)]
        public object FileName { get; set; }

        [Dump(1)]
        public object FusionLog { get; set; }
    }
}