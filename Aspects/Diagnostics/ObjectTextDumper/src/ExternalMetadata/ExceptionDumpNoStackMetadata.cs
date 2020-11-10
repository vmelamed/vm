#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ExceptionDumpNoStackMetadata
    {
        [Dump(0)]
        public object? Message { get; set; }

        [Dump(-1)]
        public object? InnerException { get; set; }

        [Dump(-2)]
        public object? TargetSite { get; set; }

        [Dump(-3)]
        public object? Source { get; set; }

        [Dump(-4, DumpNullValues = ShouldDump.Skip)]
        public object? HelpLink { get; set; }

        [Dump(-5, ValueFormat = "0x{0:X8}")]
        public object? HResult { get; set; }

        [Dump(-6)]
        public object? IsTransient { get; set; }

        [Dump(int.MinValue+2, DumpNullValues = ShouldDump.Skip)]
        public object? Data { get; set; }

        [Dump(false)]
        public object? StackTrace { get; set; }

        [Dump(false)]
        public object? RemoteStackTrace { get; set; }

        [Dump(false)]
        public object? WatsonBuckets { get; set; }

        [Dump(false)]
        public object? IPForWatsonBuckets { get; set; }
    }
}
