using vm.Aspects.Diagnostics;

namespace vm.Aspects.Wcf.FaultContracts.Metadata
{
    [Dump(DumpNullValues=ShouldDump.Skip)]
    abstract class FaultMetadata
    {
        [Dump(0)]
        public object HandlingInstanceId { get; set; }

        [Dump(1, ValueFormat="\n{0}")]
        public object Message { get; set; }

        [Dump(2, ValueFormat="\n{0}")]
        public object InnerExceptionsMessages { get; set; }

#if DEBUG
        #region Debug properties
        [Dump(-1)]
        public object User { get; set; }

        [Dump(-2)]
        public object MachineName { get; set; }

        [Dump(-3)]
        public object ProcessName { get; set; }

        [Dump(-4)]
        public object ProcessId { get; set; }

        [Dump(-5)]
        public object ThreadId { get; set; }

        [Dump(int.MinValue+2)]
        public object Data { get; set; }

        [Dump(int.MinValue+1)]
        public object StackTrace { get; set; }

        [Dump(int.MinValue)]
        public object Source { get; set; }
        #endregion
#endif
    }
}
