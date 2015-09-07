#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class BinaryExpressionDumpMetadata
    {
        [Dump(0)]
        public object Left;

        [Dump(1)]
        public object Right;

        [Dump(-1)]
        public object Method;

        [Dump(-2)]
        public object Conversion;

        [Dump(-3)]
        public object IsLifted;

        [Dump(-4)]
        public object IsLiftedToNull;
    }
}
