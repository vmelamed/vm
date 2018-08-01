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

        [Dump(int.MinValue+1)]
        public object TailCall { get; set; }


        [Dump(false)]
        public object NameCore { get; set; }

        [Dump(false)]
        public object ParameterCount { get; set; }

        [Dump(false)]
        public object PublicType { get; set; }

        [Dump(false)]
        public object TailCallCore { get; set; }
    }
}
