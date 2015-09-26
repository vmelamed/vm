using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class XmlFaultMetadata
    {
        [Dump(0)]
        public object SourceUri { get; set; }

        [Dump(1)]
        public object LineNumber { get; set; }

        [Dump(2)]
        public object LinePosition { get; set; }
    }
}