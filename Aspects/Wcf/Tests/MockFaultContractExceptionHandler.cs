using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;

namespace vm.Aspects.Wcf.Tests
{
    [ConfigurationElementType(typeof(MockFaultContractExceptionHandlerData))]
    public class MockFaultContractExceptionHandler : IExceptionHandler
    {
        public Exception HandledException;

        #region IExceptionHandler Members

        public Exception HandleException(Exception exception, Guid handlingInstanceId)
        {
            this.HandledException = exception;
            return new FaultContractWrapperException(new MockFaultContract(exception.Message), handlingInstanceId);
        }

        #endregion
    }

    public class MockFaultContractExceptionHandlerData : ExceptionHandlerData
    {
        public MockFaultContractExceptionHandlerData()
        {
        }

        public MockFaultContractExceptionHandlerData(string name)
            : base(name, typeof(FaultContractExceptionHandler))
        {
        }

        public override IExceptionHandler BuildExceptionHandler() => new MockFaultContractExceptionHandler();
    }
}
