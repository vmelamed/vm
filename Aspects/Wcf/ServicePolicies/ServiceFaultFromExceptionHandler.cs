using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using System.ServiceModel;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Transforms 
    /// </summary>
    /// <seealso cref="IExceptionHandler" />
    public class ServiceFaultFromExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// When implemented by a class, handles an <see cref="Exception"/> by converting it to
        /// the corresponding <see cref="FaultException{T}"/>.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="handlingInstanceId">The unique ID attached to the handling chain for this handling instance.</param>
        /// <returns>Modified exception to pass to the next exceptionHandlerData in the chain.</returns>
        public Exception HandleException(Exception exception, Guid handlingInstanceId)
        {
            var factory = Fault.TryGetExceptionToFaultFactory(exception.GetType()) ?? Fault.TryGetExceptionToFaultFactory<Exception>();
            var fault = factory(exception);

            fault.Data["handlingInstanceId"] = handlingInstanceId.ToString();

            return (Exception)typeof(FaultException<>)
                        .MakeGenericType(fault.GetType())
                        .GetConstructor(new Type[] { fault.GetType(), typeof(string) })
                        .Invoke(new object[] { fault, fault.Message })
                        ;
        }
    }
}
