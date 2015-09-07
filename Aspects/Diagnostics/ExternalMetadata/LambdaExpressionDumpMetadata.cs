#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class LambdaExpressionDumpMetadata
    {
        [Dump(0)]
        public object Name;

        [Dump(1)]
        public object ReturnType;

        [Dump(2)]
        public object Parameters;

        [Dump(3)]
        public object Body;

        [Dump(int.MinValue)]
        public object TailCall;
    }
}
