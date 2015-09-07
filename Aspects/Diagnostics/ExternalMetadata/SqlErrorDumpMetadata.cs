#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class SqlErrorDumpMetadata
    {
        [Dump(0)]
        public object Number;

        [Dump(1)]
        public object Message;

        [Dump(2, LabelFormat="Class (severity)")]
        public object Class;

        [Dump(3)]
        public object State;

        [Dump(4)]
        public object Server;

        [Dump(5)]
        public object Source;

        [Dump(6)]
        public object Procedure;

        [Dump(7)]
        public object LineNumber;
    }
}
