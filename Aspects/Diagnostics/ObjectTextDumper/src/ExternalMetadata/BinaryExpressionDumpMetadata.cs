#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class BinaryExpressionDumpMetadata
    {
        [Dump(0)]
        public object? Left { get; set; }

        [Dump(1)]
        public object? Right { get; set; }

        [Dump(-1)]
        public object? Method { get; set; }

        [Dump(-2)]
        public object? Conversion { get; set; }

        [Dump(-3)]
        public object? IsLifted { get; set; }

        [Dump(-4)]
        public object? IsLiftedToNull { get; set; }
    }
}
