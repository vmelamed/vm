using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
        static readonly IReadOnlyDictionary<Type, Func<Exception, Type[], Exception>> _exceptionDispatcher = new ReadOnlyDictionary<Type, Func<Exception, Type[], Exception>>(
            new Dictionary<Type, Func<Exception, Type[], Exception>>
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
            Contract.Requires<ArgumentNullException>(exception      != null, nameof(exception));
            Contract.Requires<ArgumentNullException>(expectedFaults != null, nameof(expectedFaults));
            Contract.Ensures(Contract.Result<Exception>() != null);

            Func<Exception, Type[], Exception> handler;

            if (_exceptionDispatcher.TryGetValue(exception.GetType(), out handler))
                return handler(exception, expectedFaults);
            else
                return exception;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        static Exception ProcessAggregateException(
            Exception x,
            params Type[] expectedFaults)
        {
            Contract.Requires<ArgumentNullException>(x != null, nameof(x));
            Contract.Requires<ArgumentException>(x is AggregateException, "The argument must be an AggregateException.");
            Contract.Ensures(Contract.Result<Exception>() != null);

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
            Contract.Requires<ArgumentNullException>(x != null, nameof(x));
            Contract.Requires<ArgumentException>(x is ProtocolException, "The argument must be a ProtocolException.");
            Contract.Ensures(Contract.Result<Exception>() != null);

            string responseText;

            var fault = ProtocolExceptionToWebFaultResolver.Resolve((ProtocolException)x, expectedFaults, out responseText);

            return GetFaultException(fault, responseText);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        static Exception ProcessWebException(
            Exception x,
            params Type[] expectedFaults)
        {
            Contract.Requires<ArgumentNullException>(x != null, nameof(x));
            Contract.Requires<ArgumentException>(x is WebException, "The argument must be a WebException.");
            Contract.Ensures(Contract.Result<Exception>() != null);

            string responseText;

            var fault = ProtocolExceptionToWebFaultResolver.Resolve((WebException)x, expectedFaults, out responseText);

            return GetFaultException(fault, responseText);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.ServiceModel.FaultReasonText.#ctor(System.String)", Justification = "It is OK in a fault.")]
        static Exception GetFaultException(
            Fault fault,
            string responseText)
        {
            Exception ex;

            if (fault == null)
                ex = new FaultException(
                            new FaultReason(
                                    new[]
                                    {
                                        new FaultReasonText("Unresolved ProtocolException."),
                                        new FaultReasonText(responseText),
                                    }));
            else
                ex = (FaultException)typeof(FaultException<>)
                                    .MakeGenericType(fault.GetType())
                                    .GetConstructor(new Type[] { fault.GetType() })
                                    .Invoke(new object[] { fault })
                                    ;

            return ex;
        }
    }
}
