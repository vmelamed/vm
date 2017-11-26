using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vm.Aspects.Model.Repository;
using vm.Aspects.Threading;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Constants and default implementations of methods for the <see cref="RetryUnitOfWork{T}"/> and <see cref="RetryUnitOfWorkTasks{T}"/> classes.
    /// </summary>
    public static class RetryUnitOfWorkConstants
    {
        /// <summary>
        /// The method testing if the operation has failed is:
        /// <code><![CDATA[public bool DefaultIsFailure(T result, Exception exception, int attempt) => exception != null  &&  !exception.IsTransient() &&  !(exception is RepeatableOperationException);]]></code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation failed and cannot be retried, <see langword="false" /> otherwise.</returns>
        public static bool IsFailure<T>(T result, Exception exception, int attempt)
            => RetryConstants.IsFailure(result, exception, attempt) && !exception.IsTransient();

        /// <summary>
        /// The default method testing if the operation has failed is:
        /// <code>
        /// <![CDATA[public Task<bool> DefaultIsFailure(T result, Exception exception, int attempt) => Task.FromResult(exception != null  &&  !exception.IsTransient()  &&  !(exception is RepeatableOperationException));]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation failed and cannot be retried, <see langword="false" /> otherwise.</returns>
        public static Task<bool> IsFailureAsync<T>(T result, Exception exception, int attempt)
            => Task.FromResult(RetryConstants.IsFailure(result, exception, attempt) && !exception.IsTransient());
    }
}
