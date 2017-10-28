#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class LambdaExpressionDumpMetadata
    {
        [Dump(0)]
        public object Name { get; set; }

        [Dump(1)]
        public object ReturnType { get; set; }

        [Dump(2)]
        public object Parameters { get; set; }

        [Dump(3)]
        public object Body { get; set; }

        [Dump(int.MinValue)]
        public object TailCall { get; set; }
    }
}
