using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using vm.Aspects.Wcf.Bindings;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [ServiceContract(Name = "ITestService")]
    [MessagingPattern(RequestResponseConfigurator.PatternName, true)]
    public interface ITestServiceTasks
    {
        [OperationContract(
            Action = "http://tempuri.org/ITestService/AddNewEntity",
            ReplyAction = "http://tempuri.org/ITestService/AddNewEntityResponse")]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        Task AddNewEntityAsync();

        [OperationContract(
            Action = "http://tempuri.org/ITestService/UpdateEntities",
            ReplyAction = "http://tempuri.org/ITestService/UpdateEntitiesResponse")]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        Task UpdateEntitiesAsync();

        [OperationContract(
            Action = "http://tempuri.org/ITestService/CountOfEntities",
            ReplyAction = "http://tempuri.org/ITestService/CountOfEntitiesResponse")]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        Task<int> CountOfEntitiesAsync();

        [OperationContract(
            Action = "http://tempuri.org/ITestService/CountOfValues",
            ReplyAction = "http://tempuri.org/ITestService/CountOfValuesResponse")]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        Task<int> CountOfValuesAsync();

        [OperationContract(
            Action = "http://tempuri.org/ITestService/GetEntities",
            ReplyAction = "http://tempuri.org/ITestService/GetEntitiesResponse")]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        Task<ICollection<Entity>> GetEntitiesAsync(

            int skip,

            int take);

        [OperationContract(
            Action = "http://tempuri.org/ITestService/GetCounts",
            ReplyAction = "http://tempuri.org/ITestService/GetCountsResponse")]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        Task<EntitiesAndValuesCountsDto> GetCountsAsync();
    }
}
