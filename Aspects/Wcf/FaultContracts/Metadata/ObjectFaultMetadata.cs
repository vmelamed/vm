using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class ObjectFaultMetadata
    {
        [Dump(0)]
        public object ObjectType { get; set; }

        [Dump(1)]
        public object ObjectIdentifier { get; set; }
    }
}