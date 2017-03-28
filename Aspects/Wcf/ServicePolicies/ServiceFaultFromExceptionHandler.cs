using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
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
        public Exception HandleException(
            Exception exception,
            Guid handlingInstanceId)
        {
            var exceptionType = exception.GetType();
            Func<Exception, Fault> factory = null;

            // if we cannot find exact match go up the inheritance tree. finally we must stop at Exception.
            do
            {
                factory = Fault.GetExceptionToFaultFactory(exceptionType);
                if (factory == null)
                    exceptionType = exceptionType.BaseType;
            }
            while (factory == null);

            var fault = factory(exception);

            exception.Data[nameof(fault.HandlingInstanceId)] = handlingInstanceId;
            fault.HandlingInstanceId                         = handlingInstanceId;

            var faultType = fault.GetType();

            return (Exception)typeof(WebFaultException<>)
                                .MakeGenericType(faultType)
                                .GetConstructor(new Type[] { faultType, typeof(HttpStatusCode) })
                                .Invoke(new object[] { fault, fault.HttpStatusCode })
                                ;
        }
    }
}
