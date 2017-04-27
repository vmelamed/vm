using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// Tries to execute asynchronously an operation one or more times with random delays between attempts until the operation succeeds, fails or runs out of tries.
    /// </summary>
    /// <typeparam name="T">The result of the operation.
    /// Hint: if the operation does not have a natural return value for type T (i.e. returns <c>Task</c>), use some primitive type instead, e.g. <see cref="bool"/> (<c>Task&lt;bool&gt;</c>).</typeparam>
    public class RetryTasks<T>
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
        /// The default delegate testing if the operation has failed. It returns <see langword="true"/> if the operation raised an exception, otherwise <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public readonly static Func<T, Exception, int, Task<bool>> DefaultIsFailure = (r,x,i) => Task.FromResult(x != null);]]>
        /// </code>
        /// </remarks>
        public readonly static Func<T, Exception, int, Task<bool>> DefaultIsFailure = (r,x,i) => Task.FromResult(x != null);
        /// <summary>
        /// The default delegate testing if the operation has succeeded. It returns <see langword="true"/> if the operation didn't raise an exception, otherwise <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public readonly static Func<T, Exception, int, Task<bool>> DefaultIsSuccess = (r,x,i) => Task.FromResult(x == null);]]>
        /// </code>
        /// </remarks>
        public readonly static Func<T, Exception, int, Task<bool>> DefaultIsSuccess = (r,x,i) => Task.FromResult(x == null);
        /// <summary>
        /// The default epilogue delegate throws the raised exception or returns the result of the operation.
        /// </summary>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public readonly static Func<T, Exception, int, Task<T>> DefaultEpilogue = (r,x,i) => { if (x!=null) throw x; return Task.FromResult(r); };]]>
        /// </code>
        /// </remarks>
        public readonly static Func<T, Exception, int, Task<T>> DefaultEpilogue = (r,x,i) => { if (x!=null) throw x; return Task.FromResult(r); };

        readonly Func<int, Task<T>> _operationAsync;
        readonly Func<T, Exception, int, Task<bool>> _isFailureAsync;
        readonly Func<T, Exception, int, Task<bool>> _isSuccessAsync;
        readonly Func<T, Exception, int, Task<T>> _epilogueAsync;

        readonly Lazy<Random> _random = new Lazy<Random>(() => new Random(unchecked((int)DateTime.Now.Ticks)));

        Random Random => _random.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryTasks{T}"/> class.
        /// </summary>
        /// <param name="operationAsync">
        /// The asynchronous operation to be tried one or more times.
        /// </param>
        /// <param name="isFailureAsync">
        /// Caller supplied delegate which determines if the operation failed. 
        /// If <see langword="null"/> the object will invoke <see cref="DefaultIsFailure"/>.
        /// Note that <paramref name="isFailureAsync"/> is always called before <paramref name="isSuccessAsync"/>.
        /// The operation will be retried if <paramref name="isFailureAsync"/> and <paramref name="isSuccessAsync"/> return <see langword="false"/>.
        /// </param>
        /// <param name="isSuccessAsync">
        /// Caller supplied lambda which determines if the most recent operation succeeded.
        /// If <see langword="null"/> the default returns <see langword="true"/>, which means that the operation is considered succeeded the first time.
        /// Note that <paramref name="isFailureAsync"/> is always called before <paramref name="isSuccessAsync"/>.
        /// The operation will be retried if <paramref name="isFailureAsync"/> and <paramref name="isSuccessAsync"/> return <see langword="false"/>.
        /// </param>
        /// <param name="epilogueAsync">
        /// Caller supplied lambda to be run after the operation was attempted unsuccessfully the maximum number of retries.
        /// </param>
        public RetryTasks(
            Func<int, Task<T>> operationAsync,
            Func<T, Exception, int, Task<bool>> isFailureAsync = null,
            Func<T, Exception, int, Task<bool>> isSuccessAsync = null,
            Func<T, Exception, int, Task<T>> epilogueAsync = null)
        {
            Contract.Requires<ArgumentNullException>(operationAsync != null, nameof(operationAsync));

            _operationAsync = operationAsync;
            _isSuccessAsync = isSuccessAsync ?? DefaultIsSuccess;
            _isFailureAsync = isFailureAsync ?? DefaultIsFailure;
            _epilogueAsync  = epilogueAsync  ?? DefaultEpilogue;
        }

        /// <summary>
        /// Starts retrying the operation.
        /// </summary>
        /// <param name="maxRetries">
        /// The maximum number of retries..
        /// </param>
        /// <param name="minDelay">
        /// The minimum delay between retries in milliseconds.
        /// </param>
        /// <param name="maxDelay">
        /// The maximum delay between retries in milliseconds. If maxDelay is 0 or equal to <paramref name="minDelay"/> the delay between retries will be exactly <paramref name="minDelay"/> milliseconds.
        /// If it is greater than <paramref name="minDelay"/>, each retry will happen after a random period of time between <paramref name="minDelay"/> and <paramref name="maxDelay"/> milliseconds.
        /// </param>
        /// <returns>The result of the last successful operation or the result from the epilogue lambda.</returns>
        public async Task<T> StartAsync(
            int maxRetries = DefaultMaxRetries,
            int minDelay = DefaultMinDelay,
            int maxDelay = 0)
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
                        await Task.Delay(minDelay + Random.Next(maxDelay-minDelay));
                }

                try
                {
                    exception = null;
                    result = await _operationAsync(retries);
                }
                catch (Exception x)
                {
                    exception = x;
                }

                if (await _isFailureAsync(result, exception, retries))
                    if (exception != null)
                        throw exception;
                    else
                        return result;

                if (await _isSuccessAsync(result, exception, retries))
                    return result;

                retries++;
            }
            while (retries < maxRetries);

            return await _epilogueAsync(result, exception, retries);
        }
    }
}
