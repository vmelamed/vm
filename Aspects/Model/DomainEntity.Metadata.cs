using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model
{
    abstract class DomainEntityMetadata
    {
        [Dump(0)]
        public object HasIdentity { get; set; }

        [Dump(1)]
        public object Key { get; set; }

        [Dump(2)]
        public object Id { get; set; }
    }
}