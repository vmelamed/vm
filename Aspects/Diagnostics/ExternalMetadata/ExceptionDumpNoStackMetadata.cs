#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ExceptionDumpNoStackMetadata
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

        [Dump(int.MinValue+2, DumpNullValues=ShouldDump.Skip)]
        public object Data;

        [Dump(false)]
        public object StackTrace;

        [Dump(false)]
        public object RemoteStackTrace;

        [Dump(false)]
        public object WatsonBuckets;

        [Dump(false)]
        public object IPForWatsonBuckets;
    }
}
