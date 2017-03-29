using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// Tries to execute an operation one or more times with random delays between attempts until the operation succeeds, fails or runs out of tries.
    /// </summary>
    /// <typeparam name="T">The result of the operation.
    /// Hint: if the operation does not have return value (i.e. has void return value) use some primitive type, e.g. <see cref="bool"/>.</typeparam>
    public class Retry<T>
    {
        readonly Func<int, T> _operation;
        readonly Func<T, int, bool> _isFailure;
        readonly Func<T, int, bool> _isSuccess;
        readonly Func<T, int, T> _epilogue;

        readonly Lazy<Random> _random = new Lazy<Random>(() => new Random(unchecked((int)DateTime.Now.Ticks)));

        Random Random => _random.Value;

        readonly static Func<T, int, bool> _defaultFailure  = (_,__) => false;
        readonly static Func<T, int, bool> _defaultSuccess  = (_,__) => true;
        readonly static Func<T, int, T>    _defaultEpilogue = (_,__) => default(T);

        /// <summary>
        /// Initializes a new instance of the <see cref="Retry{T}"/> class.
        /// </summary>
        /// <param name="operation">
        /// The operation to be tried one or more times.
        /// </param>
        /// <param name="isFailure">
        /// Caller supplied lambda which determines if the most recent operation failed.
        /// If <see langword="null"/> the default returns <see langword="false"/> - the operation is considered not failure (not success either).
        /// Note that <paramref name="isFailure"/> is always called before <paramref name="isSuccess"/>.
        /// Hint: based on the result from the last operation, the lambda may throw exception.
        /// </param>
        /// <param name="isSuccess">
        /// Caller supplied lambda which determines if the most recent operation succeeded.
        /// If <see langword="null"/> the default returns <see langword="true"/> - the operation is considered successful the first time.
        /// Note that <paramref name="isFailure"/> is always called before <paramref name="isSuccess"/>.
        /// </param>
        /// <param name="epilogue">
        /// Caller supplied lambda to be run after the operation was attempted unsuccessfully the maximum number of retries.
        /// Hint: based on the result from the last operation, the lambda may throw exception.
        /// </param>
        public Retry(
            Func<int, T> operation,
            Func<T, int, bool> isFailure = null,
            Func<T, int, bool> isSuccess = null,
            Func<T, int, T> epilogue = null)
        {
            Contract.Requires<ArgumentNullException>(operation != null, nameof(operation));

            _operation  = operation;
            _isSuccess  = isSuccess ?? _defaultSuccess;
            _isFailure  = isFailure ?? _defaultFailure;
            _epilogue   = epilogue  ?? _defaultEpilogue;
        }

        /// <summary>
        /// Starts retrying the operation.
        /// </summary>
        /// <param name="maxRetries">
        /// The maximum number to retry.
        /// </param>
        /// <param name="minDelay">
        /// The minimum delay before retry in milliseconds.
        /// </param>
        /// <param name="maxDelay">
        /// The maximum delay before retry in milliseconds.
        /// </param>
        /// <returns>The result of the last successful operation or the result from the epilogue lambda.</returns>
        public T Start(
            int maxRetries,
            int minDelay,
            int maxDelay)
        {
            Contract.Requires<ArgumentException>(maxRetries > 1, "Please specify more than one retries.");
            Contract.Requires<ArgumentException>(minDelay >= 0, "Please specify non-negative minimum delay before retrying.");
            Contract.Requires<ArgumentException>(maxDelay >= 0, "Please specify non-negative maximum delay before retrying.");
            Contract.Requires<ArgumentException>(maxDelay >= minDelay, "The maximum delay cannot be less than the minimum delay.");

            var result  = default(T);
            var retries = 0;
            var first   = true;

            do
            {
                if (first)
                    first = false;
                else
                if (minDelay > 0  ||  maxDelay > 0)
                    Task.Delay(minDelay + Random.Next(maxDelay-minDelay)).Wait();

                result = _operation(retries);

                if (_isFailure(result, retries))
                    return default(T);

                if (_isSuccess(result, retries))
                    return result;

                retries++;
            }
            while (retries < maxRetries);

            return _epilogue(result, retries);
        }
    }
}
