using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [ServiceContract(Name="ITestService")]
    [MessagingPattern(RequestResponseConfigurator.PatternName, true)]
    public interface ITestServiceTasks
    {
        [OperationContract(
            Action="http://tempuri.org/ITestService/AddNewEntity",
            ReplyAction="http://tempuri.org/ITestService/AddNewEntityResponse")]
        Task AddNewEntityAsync();

        [OperationContract(
            Action="http://tempuri.org/ITestService/UpdateEntities",
            ReplyAction="http://tempuri.org/ITestService/UpdateEntitiesResponse")]
        Task UpdateEntitiesAsync();

        [OperationContract(
            Action="http://tempuri.org/ITestService/CountOfEntities",
            ReplyAction="http://tempuri.org/ITestService/CountOfEntitiesResponse")]
        Task<int> CountOfEntitiesAsync();

        [OperationContract(
            Action="http://tempuri.org/ITestService/GetEntities",
            ReplyAction="http://tempuri.org/ITestService/GetEntitiesResponse")]
        Task<ICollection<Entity>> GetEntitiesAsync(

            int skip,

            int take);
    }
}
