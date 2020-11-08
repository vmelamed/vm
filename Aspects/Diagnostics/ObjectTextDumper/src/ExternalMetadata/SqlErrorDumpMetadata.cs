#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class SqlErrorDumpMetadata
    {
        [Dump(0)]
        public object? Number { get; set; }

        [Dump(1)]
        public object? Message { get; set; }

        [Dump(2, LabelFormat = "Class (severity)")]
        public object? Class { get; set; }

        [Dump(3)]
        public object? State { get; set; }

        [Dump(4)]
        public object? Server { get; set; }

        [Dump(5)]
        public object? Source { get; set; }

        [Dump(6)]
        public object? Procedure { get; set; }

        [Dump(7)]
        public object? LineNumber { get; set; }
    }
}
