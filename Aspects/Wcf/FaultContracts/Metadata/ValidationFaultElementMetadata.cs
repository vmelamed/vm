using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    abstract class ValidationFaultElementMetadata
    {
        [Dump(0)]
        public object Message { get; set; }

        [Dump(1)]
        public object TargetTypeName { get; set; }

        [Dump(2)]
        public object Key { get; set; }

        [Dump(3)]
        public object Tag { get; set; }

        [Dump(4)]
        public object NestedValidationElements { get; set; }
    }
}