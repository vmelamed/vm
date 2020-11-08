#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ExceptionDumpMetadata
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

        [Dump(int.MinValue+2, DumpNullValues = ShouldDump.Skip, MaxLength = -1)]
        public object? Data { get; set; }

        [Dump(int.MinValue+1, ValueFormat = "\r\n{0}")]
        public object? StackTrace { get; set; }

        [Dump(int.MinValue, ValueFormat = "\r\n{0}", DumpNullValues = ShouldDump.Skip)]
        public object? RemoteStackTrace { get; set; }

        [Dump(false)]
        public object? WatsonBuckets { get; set; }

        [Dump(false)]
        public object? IPForWatsonBuckets { get; set; }
    }
}
