using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using vm.Aspects.Facilities.Diagnostics;

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
        /// If <see langword="null"/> the object will invoke <see cref="RetryDefaults.IsFailure"/>.
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
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _isFailure = isFailure ?? RetryDefaults.IsFailure;
            _isSuccess = isSuccess ?? RetryDefaults.IsSuccess;
            _epilogue  = epilogue  ?? RetryDefaults.Epilogue;
        }

        /// <summary>
        /// Starts retrying the operation.
        /// </summary>
        /// <param name="maxRetries">
        /// The maximum number of attempts to re-run the operation.
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
            int maxRetries = RetryDefaults.DefaultMaxRetries,
            int minDelay = RetryDefaults.DefaultMinDelay,
            int maxDelay = RetryDefaults.DefaultMaxDelay)
        {
            if (maxRetries <= 1)
                throw new ArgumentException("The retries must be more than one.");
            if (minDelay < 0)
                throw new ArgumentException("The minimum delay before retrying must be a non-negative number.");
            if (maxDelay != 0  &&  maxDelay < minDelay)
                throw new ArgumentException("The maximum delay must be 0 or equal to or greater than the minimum delay.");

            if (maxDelay == 0)
                maxDelay = minDelay;

            Exception exception;
            var result  = default(T);
            var retries = 0;
            var first   = true;

            try
            {
                VmAspectsEventSource.Log.RetryStart(this);
                do
                {
                    if (first)
                        first = false;
                    else
                    {
                        int delay = 0;

                        if (minDelay > 0  ||  maxDelay > 0)
                        {
                            delay = minDelay + Random.Next(maxDelay-minDelay);
                            Task.Delay(delay).Wait();
                        }

                        VmAspectsEventSource.Log.Retrying(this, delay);
                    }

                    try
                    {
                        exception = null;
                        result = _operation(retries);
                    }
                    catch (Exception x)
                    {
                        VmAspectsEventSource.Log.Exception(x, EventLevel.Informational);
                        exception = x;
                    }

                    if (_isFailure(result, exception, retries))
                    {
                        VmAspectsEventSource.Log.RetryFailed(this, retries, maxRetries);
                        if (exception != null)
                            throw exception;
                        else
                            return result;
                    }

                    if (_isSuccess(result, exception, retries))
                        return result;

                    retries++;
                }
                while (retries < maxRetries);

                VmAspectsEventSource.Log.RetryFailed(this, retries, maxRetries);
                return _epilogue(result, exception, retries);
            }
            finally
            {
                VmAspectsEventSource.Log.RetryStop(this);
            }
        }
    }
}
