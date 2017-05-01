using System;
using System.Threading.Tasks;

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
        public const int DefaultMaxRetries = 10;
        /// <summary>
        /// The default minimum delay between retries.
        /// </summary>
        public const int DefaultMinDelay = 50;
        /// <summary>
        /// The default maximum delay between retries.
        /// </summary>
        public const int DefaultMaxDelay = 150;

        /// <summary>
        /// The default method testing if the operation has failed.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation failed and cannot be retried, <see langword="false" /> otherwise.</returns>
        /// <remarks>The default implementation is:
        /// <code><![CDATA[public static bool DefaultIsFailure(T result, Exception exception, int attempt) => exception != null;]]></code>
        /// </remarks>
        public static bool DefaultIsFailure<T>(T result, Exception exception, int attempt) => exception != null;

        /// <summary>
        /// The default method testing if the operation has succeeded.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation succeeded and should not be retried, <see langword="false" /> otherwise.</returns>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public static bool DefaultIsSuccess(T result, Exception exception, int attempt) => exception == null;]]>
        /// </code>
        /// </remarks>
        public static bool DefaultIsSuccess<T>(T result, Exception exception, int attempt) => exception == null;

        /// <summary>
        /// The default epilogue method throws the raised exception or returns the result of the operation.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns>If the last operation threw an exception - this will re-throw it otherwise the result from the last operation.</returns>
        /// <remarks>The default implementation is:
        /// <code><![CDATA[public static T DefaultEpilogue(T result, Exception exception, int attempt)
        /// {
        ///     if (exception!=null)
        ///         throw exception;
        ///     return result;
        /// }]]></code></remarks>
        public static T DefaultEpilogue<T>(T result, Exception exception, int attempt)
        {
            if (exception!=null)
                throw exception;
            return result;
        }

        /// <summary>
        /// The default method testing if the operation has failed.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation failed and cannot be retried, <see langword="false" /> otherwise.</returns>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public static Task<bool> DefaultIsFailure(T result, Exception exception, int attempt) => Task.FromResult(exception != null);]]>
        /// </code>
        /// </remarks>
        public static Task<bool> DefaultIsFailureAsync<T>(T result, Exception exception, int attempt) => Task.FromResult(exception != null);

        /// <summary>
        /// The default method testing if the operation has succeeded.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns><see langword="true" /> if the operation succeeded and should not be retried, <see langword="false" /> otherwise.</returns>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public static Task<bool> DefaultIsSuccessAsync<T>(T result, Exception exception, int attempt) => Task.FromResult(exception == null);]]>
        /// </code>
        /// </remarks>
        public static Task<bool> DefaultIsSuccessAsync<T>(T result, Exception exception, int attempt) => Task.FromResult(exception == null);

        /// <summary>
        /// The default epilogue method throws the raised exception or returns the result of the operation.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="exception">The exception that was thrown by the operation (if any).</param>
        /// <param name="attempt">The number of the current attempt.</param>
        /// <returns>If the last operation threw an exception - this will re-throw it otherwise the result from the last operation.</returns>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public static Task<T> DefaultEpilogue(T result, Exception exception, int attempt)
        /// {
        ///     if (exception!=null)
        ///         throw exception;
        ///     return Task.FromResult(result);
        /// }]]>
        /// </code>
        /// </remarks>
        public static Task<T> DefaultEpilogueAsync<T>(T result, Exception exception, int attempt)
        {
            if (exception!=null)
                throw exception;
            return Task.FromResult(result);
        }
    }
}
