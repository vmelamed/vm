using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class ValidationResultsFaultMetadata
    {
        [Dump(0)]
        public object ValidationElements { get; set; }

        [Dump(false)]
        public object ValidationResults { get; set; }

        [Dump(false)]
        public object Message { get; set; }
    }
}