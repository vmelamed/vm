using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [ServiceContract(Name="IService")]
    [MessagingPattern(RequestResponseConfigurator.PatternName, true)]
    public interface IServiceTasks
    {
        [OperationContract(
            Action="http://tempuri.org/IService/AddNewEntity",
            ReplyAction="http://tempuri.org/IService/AddNewEntityResponse")]
        Task AddNewEntityAsync();

        [OperationContract(
            Action="http://tempuri.org/IService/UpdateEntities",
            ReplyAction="http://tempuri.org/IService/UpdateEntitiesResponse")]
        Task UpdateEntitiesAsync();

        [OperationContract(
            Action="http://tempuri.org/IService/CountOfEntities",
            ReplyAction="http://tempuri.org/IService/CountOfEntitiesResponse")]
        Task<int> CountOfEntitiesAsync();

        [OperationContract(
            Action="http://tempuri.org/IService/GetEntities",
            ReplyAction="http://tempuri.org/IService/GetEntitiesResponse")]
        Task<ICollection<Entity>> GetEntitiesAsync(

            int skip,

            int take);
    }
}
