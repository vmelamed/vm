using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// Tries to execute an operation one or more times with random delays between attempts until the operation succeeds, fails or runs out of tries.
    /// </summary>
    /// <typeparam name="T">The result of the operation.
    /// Hint: if the operation does not have return value (i.e. has void return value) use some primitive type, e.g. <see cref="bool"/>.
    /// </typeparam>
    public class Retry<T>
    {
        readonly Func<int, T> _operation;
        readonly Func<T, Exception, int, bool> _isFailure;
        readonly Func<T, Exception, int, bool> _isSuccess;
        readonly Func<T, Exception, int, T> _epilogue;

        readonly Lazy<Random> _random = new Lazy<Random>(() => new Random(unchecked((int)DateTime.Now.Ticks)));

        Random Random => _random.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Retry{T}"/> class.
        /// </summary>
        /// <param name="operation">
        /// The operation to be tried one or more times.
        /// </param>
        /// <param name="isFailure">
        /// Caller supplied delegate which determines if the operation failed. 
        /// If <see langword="null"/> the object will invoke <see cref="RetryConstants.DefaultIsFailure"/>.
        /// Note that <paramref name="isFailure"/> is always called before <paramref name="isSuccess"/>.
        /// The operation will be retried if <paramref name="isFailure"/> and <paramref name="isSuccess"/> return <see langword="false"/>.
        /// </param>
        /// <param name="isSuccess">
        /// Caller supplied lambda which determines if the most recent operation succeeded.
        /// If <see langword="null"/> the default returns <see langword="true"/>, which means that the operation is considered succeeded the first time.
        /// Note that <paramref name="isFailure"/> is always called before <paramref name="isSuccess"/>.
        /// The operation will be retried if <paramref name="isFailure"/> and <paramref name="isSuccess"/> return <see langword="false"/>.
        /// </param>
        /// <param name="epilogue">
        /// Caller supplied lambda to be run after the operation was attempted unsuccessfully the maximum number of retries.
        /// </param>
        public Retry(
            Func<int, T> operation,
            Func<T, Exception, int, bool> isFailure = null,
            Func<T, Exception, int, bool> isSuccess = null,
            Func<T, Exception, int, T> epilogue = null)
        {
            Contract.Requires<ArgumentNullException>(operation != null, nameof(operation));

            _operation = operation;
            _isSuccess = isSuccess ?? RetryConstants.DefaultIsSuccess;
            _isFailure = isFailure ?? RetryConstants.DefaultIsFailure;
            _epilogue  = epilogue  ?? RetryConstants.DefaultEpilogue;
        }

        /// <summary>
        /// Starts retrying the operation.
        /// </summary>
        /// <param name="maxRetries">
        /// The maximum number of attempts to run the operation.
        /// </param>
        /// <param name="minDelay">
        /// The minimum delay before retrying the operation in milliseconds.
        /// </param>
        /// <param name="maxDelay">
        /// The maximum delay before retrying the operation in milliseconds.
        /// </param>
        /// <returns>The result of the last successful operation or the result from the epilogue lambda.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public T Start(
            int maxRetries = RetryConstants.DefaultMaxRetries,
            int minDelay = RetryConstants.DefaultMinDelay,
            int maxDelay = RetryConstants.DefaultMaxDelay)
        {
            Contract.Requires<ArgumentException>(maxRetries > 1, "The retries must be more than one.");
            Contract.Requires<ArgumentException>(minDelay >= 0, "The minimum delay before retrying must be a non-negative number.");
            Contract.Requires<ArgumentException>(maxDelay == 0  ||  maxDelay >= minDelay, "The maximum delay must be 0 or equal to or greater than the minimum delay.");

            if (maxDelay == 0)
                maxDelay = minDelay;

            Exception exception;
            var result  = default(T);
            var retries = 0;
            var first   = true;

            do
            {
                if (first)
                    first = false;
                else
                {
                    if (minDelay > 0  ||  maxDelay > 0)
                        Task.Delay(minDelay + Random.Next(maxDelay-minDelay)).Wait();
                }

                try
                {
                    exception = null;
                    result = _operation(retries);
                }
                catch (Exception x)
                {
                    exception = x;
                }

                if (_isFailure(result, exception, retries))
                    if (exception != null)
                        throw exception;
                    else
                        return result;

                if (_isSuccess(result, exception, retries))
                    return result;

                retries++;
            }
            while (retries < maxRetries);

            return _epilogue(result, exception, retries);
        }
    }
}
