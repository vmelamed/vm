using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using vm.Aspects.Exceptions;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Class EFRepositoryExceptionHandler.
    /// </summary>
    /// <seealso cref="IExceptionHandler" />
    public class EFRepositoryExceptionHandler : IExceptionHandler
    {
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
            // if the exception is transient - wrap it in RepeatableOperationException
            if (exception.IsTransient())
            {
                var ex = new RepeatableOperationException(exception);

                ex.Data["HandlingInstanceId"] = handlingInstanceId;
                return ex;
            }

            return exception;
        }
    }
}
