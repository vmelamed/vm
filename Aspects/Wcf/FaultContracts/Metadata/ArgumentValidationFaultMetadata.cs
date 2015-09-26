using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class ArgumentValidationFaultMetadata
    {
        [Dump(0)]
        public object ValidationElements { get; set; }

        [Dump(1)]
        public object ValidationResults { get; set; }

        [Dump(2)]
        public object Message { get; set; }
    }
}