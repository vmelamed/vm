using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using System.Data.Entity.Infrastructure;
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
        /// Initializes a new instance of the <see cref="EFRepositoryExceptionHandler"/> class.
        /// </summary>
        /// <param name="optimisticConcurrencyStrategy">The optimistic concurrency strategy.</param>
        public EFRepositoryExceptionHandler(
            OptimisticConcurrencyStrategy optimisticConcurrencyStrategy = OptimisticConcurrencyStrategy.StoreWins)
        {
            OptimisticConcurrencyStrategy = optimisticConcurrencyStrategy;
        }

        /// <summary>
        /// Gets or sets the optimistic concurrency strategy: client wins vs store wins. Determines whether the 
        /// </summary>
        public OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        /// <summary>
        /// When implemented by a class, handles an <see cref="Exception" />.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="handlingInstanceId">The unique ID attached to the handling chain for this handling instance.</param>
        /// <returns>Modified exception to pass to the next exceptionHandlerData in the chain.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Exception HandleException(
            Exception exception,
            Guid handlingInstanceId)
        {
            if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.None)
                return exception;

            if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.ClientWins  &&
                exception is DbUpdateConcurrencyException)
                return null;

            // The strategy is StoreWins or the exception is not 
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
