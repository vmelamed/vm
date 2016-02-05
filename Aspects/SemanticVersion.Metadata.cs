using vm.Aspects.Diagnostics;

namespace vm.Aspects
{
    abstract class SemanticVersionMetadata
    {
        [Dump(0)]
        public object Major { get; set; }

        [Dump(1)]
        public object Minor { get; set; }

        [Dump(2)]
        public object Patch { get; set; }

        [Dump(3)]
        public object Prerelease { get; set; }

        [Dump(4)]
        public object Build { get; set; }
    }
}