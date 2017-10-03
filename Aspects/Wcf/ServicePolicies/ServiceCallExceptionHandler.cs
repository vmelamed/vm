using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
    /// <seealso cref="IExceptionHandler" />
    public class ServiceCallExceptionHandler : IExceptionHandler
    {
        static readonly IReadOnlyDictionary<Type, Func<Exception, Type[], Exception>> _exceptionDispatcher = new ReadOnlyDictionary<Type, Func<Exception, Type[], Exception>>(
            new Dictionary<Type, Func<Exception, Type[], Exception>>
            {
                [typeof(WebException)]       = ProcessWebException,
                [typeof(ProtocolException)]  = ProcessProtocolException,
                [typeof(AggregateException)] = ProcessAggregateException,
            });

        /// <summary>
        /// When implemented by a class, handles an <see cref="Exception" />.
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

        /// <summary>
        /// Does the handling of the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="expectedFaults">Expected fault types to check for. Can be <see langword="null"/></param>
        /// <returns>Exception.</returns>
        public static Exception DoHandleException(
            Exception exception,
            params Type[] expectedFaults)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            if (expectedFaults == null)
                throw new ArgumentNullException(nameof(expectedFaults));

            Func<Exception, Type[], Exception> handler;

            if (_exceptionDispatcher.TryGetValue(exception.GetType(), out handler))
                return handler(exception, expectedFaults);
            else
            {
                var faultException = exception as FaultException;

                if (faultException != null)
                    return faultException.ToException();
            }

            return exception;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        static Exception ProcessAggregateException(
            Exception x,
            params Type[] expectedFaults)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (!(x is AggregateException))
                throw new ArgumentException("The argument must be an AggregateException.");

            var exception = (AggregateException)x;

            if (exception.InnerExceptions.Count == 1)
                return DoHandleException(exception.InnerExceptions[0], expectedFaults);
            else
                return new AggregateException(exception.InnerExceptions.Select(ex => DoHandleException(ex, expectedFaults)));
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        static Exception ProcessProtocolException(
            Exception x,
            params Type[] expectedFaults)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (!(x is ProtocolException))
                throw new ArgumentException("The argument must be a ProtocolException.");

            string responseText;

            var fault = ProtocolExceptionToWebFaultResolver.Resolve((ProtocolException)x, expectedFaults, out responseText);

            return GetFaultException(fault, responseText);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        static Exception ProcessWebException(
            Exception x,
            params Type[] expectedFaults)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (!(x is WebException))
                throw new ArgumentException("The argument must be a WebException.");

            string responseText;

            var fault = ProtocolExceptionToWebFaultResolver.Resolve((WebException)x, expectedFaults, out responseText);

            if (fault != null)
                return GetFaultException(fault, responseText);
            else
                return x;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.ServiceModel.FaultReasonText.#ctor(System.String)", Justification = "It is OK in a fault.")]
        static Exception GetFaultException(
            Fault fault,
            string responseText)
        {
            Exception x = null;

            if (fault == null)
                return new FaultException(
                            new FaultReason(
                                    new[]
                                    {
                                        new FaultReasonText("Unresolved ProtocolException."),
                                        new FaultReasonText(responseText),
                                    }));

            var exceptionFactory = Fault.GetFaultToExceptionFactory(fault.GetType());

            if (exceptionFactory != null)
                x = exceptionFactory(fault);
            else
                x = (FaultException)typeof(FaultException<>)
                                            .MakeGenericType(fault.GetType())
                                            .GetConstructor(new Type[] { fault.GetType() })
                                            .Invoke(new object[] { fault })
                                            ;

            x.Data["ResponseText"] = responseText;
            return x;
        }
    }
}
