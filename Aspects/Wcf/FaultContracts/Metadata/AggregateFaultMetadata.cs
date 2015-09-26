using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class AggregateFaultMetadata
    {
        [Dump(0)]
        public object InnerExceptions { get; set; }
    }
}