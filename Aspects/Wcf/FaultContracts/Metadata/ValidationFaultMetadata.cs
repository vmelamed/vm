using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class ValidationFaultMetadata
    {
        [Dump(0)]
        public object IsValid { get; set; }

        [Dump(1)]
        public object InternalDetails { get; set; }

        [Dump(2)]
        public object Details { get; set; }
    }
}