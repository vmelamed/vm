using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    abstract class ValueMetadata
    {
        [Dump(0)]
        public object Name { get; set; }

        [Dump(1, RecurseDump = ShouldDump.Skip)]
        public object Entity { get; set; }

        [Dump(2)]
        public object EntityId { get; set; }

        [Dump(3)]
        public object CreatedOn { get; set; }

        [Dump(4)]
        public object UpdatedOn { get; set; }
    }
}