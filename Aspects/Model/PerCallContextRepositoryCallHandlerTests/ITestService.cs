using System.Collections.Generic;
using System.ServiceModel;
using vm.Aspects.Wcf.Bindings;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [ServiceContract]
    [MessagingPattern(RequestResponseConfigurator.PatternName, true)]
    public interface ITestService
    {
        [OperationContract]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        void AddNewEntity();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        void UpdateEntities();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        int CountOfEntities();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        int CountOfValues();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        ICollection<Entity> GetEntities(int skip, int take);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        [FaultContract(typeof(UnauthorizedAccessFault))]
        [FaultContract(typeof(AggregateFault))]
        [FaultContract(typeof(InvalidOperationFault))]
        [FaultContract(typeof(DataFault))]
        [FaultContract(typeof(RepeatableOperationFault))]
        EntitiesAndValuesCountsDto GetCounts();
    }
}
