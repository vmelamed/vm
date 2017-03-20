using System.Runtime.Serialization;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [DataContract]
    public class EntitiesAndValuesCountsDto
    {
        [DataMember]
        public int Entities { get; set; }

        [DataMember]
        public int Values { get; set; }
    }
}
