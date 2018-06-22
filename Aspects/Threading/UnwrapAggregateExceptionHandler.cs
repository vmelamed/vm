using System;
using System.Linq;

using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

using vm.Aspects.Facilities;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// Handles <see cref="AggregateException"/>-s. If the exception contains a single exception in its <see cref="AggregateException.InnerExceptions"/>,
    /// it returns the inner exception. It is recommended to combine this handler with a logging exception handler to reveal the call stack.
    /// </summary>
    /// <seealso cref="IExceptionHandler" />
    public class UnwrapAggregateExceptionHandler : IExceptionHandler
    {
        readonly string _exceptionPolicyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnwrapAggregateExceptionHandler"/> class.
        /// </summary>
        /// <param name="exceptionPolicyName">Name of the exception policy.</param>
        public UnwrapAggregateExceptionHandler(
            string exceptionPolicyName)
        {
            _exceptionPolicyName = exceptionPolicyName;
        }

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
            var innerException = Unwrap(exception as AggregateException, handlingInstanceId);

            if (innerException == null  ||
                innerException == exception)
                return exception;

            // pass the inner exception to the requested exception handler
            if (Facility.ExceptionManager.HandleException(innerException, _exceptionPolicyName, out var newException))
                if (newException != null)
                {
                    if (handlingInstanceId != default)
                        newException.Data["HandlingInstanceId"] = handlingInstanceId;
                    return newException;
                }
                else
                    return exception;

            return null;
        }

        /// <summary>
        /// Gets the single inner exception wrapped in one or more <see cref="AggregateException" />-s.
        /// If there are more than one inner exceptions - returns the original argument.
        /// </summary>
        /// <param name="exception">The exception to be unwrapped.</param>
        /// <param name="handlingInstanceId">The handling instance identifier.</param>
        /// <returns>The single inner exception or the argument.</returns>
        public static Exception Unwrap(
            AggregateException exception,
            Guid handlingInstanceId = default)
        {
            Exception x = exception;

            for (var ax = exception; ax != null; ax = x as AggregateException)
            {
                if (ax.InnerExceptions.Count() != 1)
                    // cannot be unwrapped to a single exception - return the original exception
                    return exception;

                // unwrap
                x = ax.InnerExceptions[0];
            }

            x.Data["HandlingInstanceId"] = handlingInstanceId;
            return x;
        }
    }
}
