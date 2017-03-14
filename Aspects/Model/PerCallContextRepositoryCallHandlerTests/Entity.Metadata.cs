using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    abstract class EntityMetadata
    {
        [Dump(1)]
        public object UniqueId { get; set; }

        [Dump(2)]
        public object Name { get; set; }

        [Dump(3)]
        public object ValuesList { get; set; }

        [Dump(4)]
        public object CreatedOn { get; set; }

        [Dump(5)]
        public object UpdatedOn { get; set; }
    }
}