using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using vm.Aspects.Exceptions;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// Constants and default implementations of methods for the <see cref="Retry{T}"/> and <see cref="RetryTasks{T}"/> classes.
    /// </summary>
    public static class RetryConstants
    {
        /// <summary>
        /// The default maximum number of retries.
        /// </summary>
        public const int DefaultMaxRetries = 5;
        /// <summary>
        /// The default minimum delay between retries.
        /// </summary>
        public const int DefaultMinDelay = 10;
        /// <summary>
        /// The default maximum delay between retries.
        /// </summary>
        public const int DefaultMaxDelay = 100;

        /// <summary>
        /// The method testing if the operation has failed is: 
        /// <code>
        /// <![CDATA[public static bool DefaultIsFailure(T result, Exception exception, int attempt) => exception != null  &&  !exception.IsTransient() &&  !(exception is RepeatableOperationException);]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation failed and cannot be retried, <see langword="false" /> otherwise.</returns>
        public static bool IsFailure<T>(T result, Exception exception, int attempt) => exception != null  &&  !(exception is RepeatableOperationException);

        /// <summary>
        /// The method testing if the operation has succeeded is: 
        /// <code>
        /// <![CDATA[public static bool DefaultIsSuccess(T result, Exception exception, int attempt) => exception == null;]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation succeeded and should not be retried, <see langword="false" /> otherwise.</returns>
        /// <remarks>
        /// The default implementation is:
        /// </remarks>
        public static bool IsSuccess<T>(T result, Exception exception, int attempt) => exception == null;

        /// <summary>
        /// The method testing if the operation has succeeded is: 
        /// <code>
        /// <![CDATA[public static bool DefaultIsSuccess(T result, Exception exception, int attempt) => exception == null;]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation succeeded and should not be retried, <see langword="false" /> otherwise.</returns>
        /// <remarks>
        /// The default implementation is:
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "attempt")]
        public static bool IsSuccessResult<T>(T result, Exception exception, int attempt) => exception == null  &&  !EqualityComparer<T>.Default.Equals(result, default(T));

        /// <summary>
        /// The epilogue method throws the raised exception or returns the result of the operation:
        /// <code>
        /// <![CDATA[public static T DefaultEpilogue(T result, Exception exception, int attempt)
        /// {
        ///     if (exception!=null)
        ///         throw exception;
        ///     return result;
        /// }]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns>If the last operation threw an exception - this will re-throw it otherwise the result from the last operation.</returns>
        public static T Epilogue<T>(T result, Exception exception, int attempt)
        {
            if (exception!=null)
                throw exception;

            return result;
        }

        /// <summary>
        /// The default method testing if the operation has failed is:
        /// <code>
        /// <![CDATA[public static Task<bool> DefaultIsFailure(T result, Exception exception, int attempt) => Task.FromResult(exception != null  &&  !exception.IsTransient()  &&  !(exception is RepeatableOperationException));]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation failed and cannot be retried, <see langword="false" /> otherwise.</returns>
        public static Task<bool> IsFailureAsync<T>(T result, Exception exception, int attempt) => Task.FromResult(exception != null  &&  !(exception is RepeatableOperationException));

        /// <summary>
        /// The default method testing if the operation has succeeded is:
        /// <code>
        /// <![CDATA[public static Task<bool> DefaultIsSuccessAsync<T>(T result, Exception exception, int attempt) => Task.FromResult(exception == null);]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation succeeded and should not be retried, <see langword="false" /> otherwise.</returns>
        public static Task<bool> IsSuccessAsync<T>(T result, Exception exception, int attempt) => Task.FromResult(exception == null);

        /// <summary>
        /// The default method testing if the operation has succeeded is:
        /// <code>
        /// <![CDATA[public static Task<bool> DefaultIsSuccessAsync<T>(T result, Exception exception, int attempt) => Task.FromResult(exception == null);]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation succeeded and should not be retried, <see langword="false" /> otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "attempt")]
        public static Task<bool> IsSuccessResultAsync<T>(T result, Exception exception, int attempt) => Task.FromResult(exception == null  &&  !EqualityComparer<T>.Default.Equals(result, default(T)));

        /// <summary>
        /// The epilogue method throws the raised exception or returns the result of the operation:
        /// <code>
        /// <![CDATA[public static Task<T> DefaultEpilogue(T result, Exception exception, int attempt)
        /// {
        ///     if (exception!=null)
        ///         throw exception;
        ///     return Task.FromResult(result);
        /// }]]>
        /// </code>
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns>If the last operation threw an exception - this will re-throw it otherwise the result from the last operation.</returns>
        public static Task<T> EpilogueAsync<T>(T result, Exception exception, int attempt)
        {
            if (exception!=null)
                throw exception;

            return Task.FromResult(result);
        }
    }
}
