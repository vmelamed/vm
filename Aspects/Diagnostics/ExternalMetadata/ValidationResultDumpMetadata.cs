#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ValidationResultDumpMetadata
    {
        [Dump(0)]
        public object Message;

        [Dump(1)]
        public object Key;

        [Dump(2)]
        public object Tag;

        [Dump(3, RecurseDump=ShouldDump.Skip)]
        public object Target;

        [Dump(4, RecurseDump=ShouldDump.Skip)]
        public object Validator;

        [Dump(5)]
        public object NestedValidationResults;
    }
}
