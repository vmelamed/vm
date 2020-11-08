#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ValidationResultDumpMetadata
    {
        [Dump(0)]
        public object? Message { get; set; }

        [Dump(1)]
        public object? Key { get; set; }

        [Dump(2)]
        public object? Tag { get; set; }

        [Dump(3, RecurseDump = ShouldDump.Skip)]
        public object? Target { get; set; }

        [Dump(4, RecurseDump = ShouldDump.Skip)]
        public object? Validator { get; set; }

        [Dump(5)]
        public object? NestedValidationResults { get; set; }
    }
}
