#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ExceptionDumpMetadata
    {
        [Dump(0)]
        public object Message;

        [Dump(-1)]
        public object InnerException;

        [Dump(-2)]
        public object TargetSite;

        [Dump(-3)]
        public object Source;

        [Dump(-4, DumpNullValues=ShouldDump.Skip)]
        public object HelpLink;

        [Dump(-5, ValueFormat="0x{0:X8}")]
        public object HResult;

        [Dump(-6)]
        public object IsTransient;

        [Dump(int.MinValue+2, DumpNullValues=ShouldDump.Skip, MaxLength = -1)]
        public object Data;

        [Dump(int.MinValue+1, ValueFormat="\r\n{0}")]
        public object StackTrace;

        [Dump(int.MinValue, ValueFormat="\r\n{0}", DumpNullValues=ShouldDump.Skip)]
        public object RemoteStackTrace;

        [Dump(false)]
        public object WatsonBuckets;

        [Dump(false)]
        public object IPForWatsonBuckets;
    }
}
