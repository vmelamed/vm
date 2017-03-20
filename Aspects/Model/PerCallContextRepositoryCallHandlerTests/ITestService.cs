using System.Collections.Generic;
using System.ServiceModel;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [ServiceContract]
    [MessagingPattern(RequestResponseConfigurator.PatternName, true)]
    public interface ITestService
    {
        [OperationContract]
        void AddNewEntity();

        [OperationContract]
        void UpdateEntities();

        [OperationContract]
        int CountOfEntities();

        [OperationContract]
        int CountOfValues();

        [OperationContract]
        ICollection<Entity> GetEntities(int skip, int take);

        [OperationContract]
        EntitiesAndValuesCountsDto GetCounts();
    }
}
