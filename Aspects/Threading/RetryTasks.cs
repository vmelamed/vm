using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// Asynchronously tries to execute an operation one or more times with random delays between attempts until the operation succeeds, fails or runs out of tries.
    /// </summary>
    /// <typeparam name="T">The result of the operation.
    /// Hint: if the operation does not have return value (i.e. has void return value) use some primitive type, e.g. <see cref="bool"/>.</typeparam>
    public class RetryTasks<T>
    {
        readonly int _maxRetries;
        readonly int _minDelay;
        readonly int _maxDelay;

        readonly Func<int, Task<T>> _operationAsync;
        readonly Func<T, int, Task<bool>> _isFailureAsync;
        readonly Func<T, int, Task<bool>> _isSuccessAsync;
        readonly Func<T, int, Task<T>> _epilogueAsync;

        readonly Lazy<Random> _random = new Lazy<Random>(() => new Random(unchecked((int)DateTime.Now.Ticks)));

        Random Random => _random.Value;

        readonly static Func<T, int, Task<bool>> _defaultFailure  = (_,__) => Task.FromResult(false);
        readonly static Func<T, int, Task<bool>> _defaultSuccess  = (_,__) => Task.FromResult(true);
        readonly static Func<T, int, Task<T>>    _defaultEpilogue = (_,__) => Task.FromResult(default(T));

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryTasks{T}"/> class.
        /// </summary>
        /// <param name="operationAsync">
        /// The asynchronous operation to be tried one or more times.
        /// </param>
        /// <param name="maxRetries">
        /// The maximum number to retry.
        /// </param>
        /// <param name="minDelay">
        /// The minimum delay before retry in milliseconds.
        /// </param>
        /// <param name="maxDelay">
        /// The maximum delay before retry in milliseconds.
        /// </param>
        /// <param name="isFailureAsync">
        /// Caller supplied asynchronous lambda which determines if the most recent operation failed.
        /// If <see langword="null"/> the default returns <see langword="false"/> - the operation is considered not failure (not success either).
        /// Note that <paramref name="isFailureAsync"/> is always called before <paramref name="isSuccessAsync"/>.
        /// Hint: based on the result from the last operation, the lambda may throw exception.
        /// </param>
        /// <param name="isSuccessAsync">
        /// Caller supplied asynchronous lambda which determines if the most recent operation succeeded.
        /// If <see langword="null"/> the default returns <see langword="true"/> - the operation is considered successful the first time.
        /// Note that <paramref name="isFailureAsync"/> is always called before <paramref name="isSuccessAsync"/>.
        /// </param>
        /// <param name="epilogueAsync">
        /// Caller supplied asynchronous lambda to be run after the operation was attempted unsuccessfully <paramref name="maxRetries"/> times.
        /// Hint: based on the result from the last operation, the lambda may throw exception.
        /// </param>
        public RetryTasks(
            Func<int, Task<T>> operationAsync,
            int maxRetries,
            int minDelay,
            int maxDelay,
            Func<T, int, Task<bool>> isFailureAsync = null,
            Func<T, int, Task<bool>> isSuccessAsync = null,
            Func<T, int, Task<T>> epilogueAsync = null)
        {
            Contract.Requires<ArgumentNullException>(operationAsync != null, nameof(operationAsync));
            Contract.Requires<ArgumentException>(maxRetries > 1, "Please specify more than one retries.");
            Contract.Requires<ArgumentException>(minDelay >= 0, "Please specify non-negative minimum delay before retrying.");
            Contract.Requires<ArgumentException>(maxDelay >= 0, "Please specify non-negative maximum delay before retrying.");
            Contract.Requires<ArgumentException>(maxDelay >= minDelay, "The maximum delay cannot be less than the minimum delay.");

            _operationAsync = operationAsync;
            _maxRetries     = maxRetries;
            _minDelay       = minDelay;
            _maxDelay       = maxDelay;
            _isSuccessAsync = isSuccessAsync ?? _defaultSuccess;
            _isFailureAsync = isFailureAsync ?? _defaultFailure;
            _epilogueAsync  = epilogueAsync  ?? _defaultEpilogue;
        }

        /// <summary>
        /// Starts retrying the operation.
        /// </summary>
        /// <returns>The result of the last successful operation or the result from the epilogue lambda.</returns>
        public async Task<T> StartAsync()
        {
            var result  = default(T);
            var retries = 0;
            var first   = true;

            do
            {
                if (first)
                    first = false;
                else
                if (_minDelay > 0  ||  _maxDelay > 0)
                    await Task.Delay(_minDelay + Random.Next(_maxDelay-_minDelay));

                result = await _operationAsync(retries);

                if (await _isFailureAsync(result, retries))
                    return default(T);

                if (await _isSuccessAsync(result, retries))
                    return result;

                retries++;
            }
            while (retries < _maxRetries);

            return await _epilogueAsync(result, retries);
        }
    }
}
