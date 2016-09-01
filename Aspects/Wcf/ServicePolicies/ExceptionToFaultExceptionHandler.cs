using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.ServiceModel;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Transforms 
    /// </summary>
    /// <seealso cref="IExceptionHandler" />
    public class ExceptionToFaultExceptionHandler : IExceptionHandler
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
            var fault = Fault.FaultFactory(exception);

            fault.Data["handlingInstanceId"] = handlingInstanceId.ToString();

            return (Exception)typeof(FaultException<>)
                        .MakeGenericType(fault.GetType())
                        .GetConstructor(new Type[] { fault.GetType(), typeof(string) })
                        .Invoke(new object[] { fault, fault.Message })
                        ;
        }
    }
}
