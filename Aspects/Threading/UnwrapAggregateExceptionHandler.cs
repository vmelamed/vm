using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using System.Linq;
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
            var innerException     = exception;
            var aggregateException = innerException as AggregateException;

            while (aggregateException != null)
            {
                if (aggregateException.InnerExceptions.Count() == 1)
                    // unwrap
                    innerException = aggregateException.InnerExceptions[0];
                else
                    // cannot be unwrapped to a single exception - return the original exception
                    return exception;

                aggregateException = innerException as AggregateException;
            }

            if (innerException == exception)
                return exception;

            Exception newException;

            // call the exception handler on the inner exception
            if (Facility.ExceptionManager.HandleException(innerException, _exceptionPolicyName, out newException)  &&  newException != null)
                return newException;

            return innerException;
        }
    }
}
