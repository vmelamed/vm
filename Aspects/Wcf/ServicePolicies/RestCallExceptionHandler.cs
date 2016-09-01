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
    /// Class RestCallExceptionHandler implements <see cref="IExceptionHandler"/> for <see cref="WebException"/>, <see cref="ProtocolException"/> and <see cref="AggregateException"/>,
    /// by possibly extracting the fault out of the response and converting it to a <see cref="FaultException"/>.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.IExceptionHandler" />
    public class RestCallExceptionHandler : IExceptionHandler
    {
        static readonly IReadOnlyDictionary<Type, Func<Exception, Exception>> _exceptionDispatcher = new ReadOnlyDictionary<Type, Func<Exception, Exception>>(
            new SortedDictionary<Type, Func<Exception, Exception>>
            {
                [typeof(WebException)]       = ProcessWebException,
                [typeof(ProtocolException)]  = ProcessProtocolException,
                [typeof(AggregateException)] = ProcessAggregateException,
            });

        /// <summary>
        /// When implemented by a class, handles an <see cref="T:System.Exception" />.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="handlingInstanceId">The unique ID attached to the handling chain for this handling instance.</param>
        /// <returns>Modified exception to pass to the next exceptionHandlerData in the chain.</returns>
        public Exception HandleException(
            Exception exception,
            Guid handlingInstanceId)
        {
            var ex = DoHandleException(exception);

            ex.Data["handlingInstanceId"] = handlingInstanceId;
            return ex;
        }

        static Exception DoHandleException(
            Exception exception)
        {
            Func<Exception, Exception> handler;

            if (_exceptionDispatcher.TryGetValue(exception.GetType(), out handler))
                return handler(exception);
            else
                return exception;
        }

        static Exception ProcessProtocolException(
            Exception x)
        {
            Contract.Requires<ArgumentNullException>(x != null, nameof(x));
            Contract.Requires<ArgumentException>(x is ProtocolException, "The argument must be a ProtocolException.");
            Contract.Ensures(Contract.Result<Exception>() != null);

            string faultJson;

            var fault = ProtocolExceptionToWebFaultResolver.Resolve((ProtocolException)x, out faultJson);

            return GetFaultException(fault, faultJson);
        }

        static Exception ProcessWebException(
            Exception x)
        {
            Contract.Requires<ArgumentNullException>(x != null, nameof(x));
            Contract.Requires<ArgumentException>(x is WebException, "The argument must be a WebException.");
            Contract.Ensures(Contract.Result<Exception>() != null);

            string faultJson;

            var fault = ProtocolExceptionToWebFaultResolver.Resolve((WebException)x, out faultJson);

            return GetFaultException(fault, faultJson);
        }

        static Exception ProcessAggregateException(
            Exception x)
        {
            Contract.Requires<ArgumentNullException>(x != null, nameof(x));
            Contract.Requires<ArgumentException>(x is AggregateException, "The argument must be an AggregateException.");
            Contract.Ensures(Contract.Result<Exception>() != null);

            var exception = (AggregateException)x;

            if (exception.InnerExceptions.Count == 1)
                return DoHandleException(exception.InnerExceptions[0]);

            return new AggregateException(exception.InnerExceptions.Select(ex => DoHandleException(ex)));
        }

        static Exception GetFaultException(
            Fault fault,
            string faultJson)
        {
            if (fault == null)
                return new FaultException(
                            new FaultReason(
                                    new[]
                                    {
                                        new FaultReasonText("Unresolved ProtocolException."),
                                        new FaultReasonText(faultJson),
                                    }));

            // TODO: should we convert the fault to an exception?
            //var exception = ExceptionFactory.CreateException(fault);

            //if (exception != null)
            //    return exception;

            return (FaultException)typeof(FaultException<>)
                                .MakeGenericType(fault.GetType())
                                .GetConstructor(new Type[] { fault.GetType() })
                                .Invoke(new object[] { fault })
                                ;
        }
    }
}
