using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class ArgumentFaultMetadata
    {
        [Dump(0)]
        public object ParamName { get; set; }
    }
}