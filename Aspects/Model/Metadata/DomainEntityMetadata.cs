using vm.Aspects.Diagnostics;
using vm.Aspects.Validation;

namespace vm.Aspects.Model.Metadata
{
    abstract class DomainEntityMetadata
    {
        [Dump(0)]
        public object HasIdentity
        { get; set; }

        [Dump(2)]
        [NonnegativeValidator]
        public object Id
        { get; set; }

        [Dump(1)]
        public object Key
        { get; set; }
    }
}