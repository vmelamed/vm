using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vm.Aspects.Facilities;

namespace vm.Aspects.Threading
{
    /// <summary>
    /// Class TaskCombinators. Contains several methods combining <see cref="T:System.Threading.Tasks.Task"/> objects.
    /// The methods were copied from the famous article "The Task-based Asynchronous Pattern" by Stephen Toub.
    /// http://www.microsoft.com/en-us/download/details.aspx?id=19957
    /// </summary>
    public static class TaskCombinators
    {
        /// <summary>
        /// <c>RetryOnFault</c> executes the operation <paramref name="method"/> and if that fails it executes the <paramref name="shouldRetry"/> method. 
        /// If <paramref name="shouldRetry"/> returns <see langword="true"/>, <c>RetryOnFault</c> will retry <paramref name="method"/>, 
        /// up to <paramref name="maxRetries"/>
        /// </summary>
        /// <typeparam name="T">The type of the result value of <paramref name="method"/>.</typeparam>
        /// <param name="method">The method to be executed and possibly retried.</param>
        /// <param name="maxRetries">The maximum number of retries.</param>
        /// <param name="shouldRetry">A method which determines if the <paramref name="method"/> should be retried after a failure.</param>
        /// <returns>Task{T} object.</returns>
        /// <exception cref="AggregateException">
        /// Thrown if the <paramref name="method"/> has failed <paramref name="maxRetries"/> or <paramref name="shouldRetry"/> disallowed the retries.
        /// The property <see cref="P:System.AggregateException.InnerExceptions"/> contains a history of all exceptions.
        /// </exception>
        /// <remarks>
        /// Note that <paramref name="method"/> and <paramref name="shouldRetry"/> are executed with <c>ConfigureAwait(false)</c>
        /// </remarks>
        /// <example>
        /// Usage:
        /// <code>
        /// Exception exception = null;
        /// var page = await RetryOnFault(
        ///                     () => DownloadAsync(url),
        ///                     3,
        ///                     x => {
        ///                              if (IsFatal(x)) return false;
        ///                              await Task.Delay(1000);
        ///                              return true;
        ///                          });
        /// </code>
        /// </example>
        public static async Task<T> RetryOnFault<T>(
            Func<Task<T>> method,
            int maxRetries,
            Func<Exception, Task<bool>> shouldRetry)
        {
            Contract.Requires<ArgumentNullException>(method != null, nameof(method));
            Contract.Requires<ArgumentNullException>(shouldRetry != null, nameof(shouldRetry));
            Contract.Ensures(Contract.Result<Task<T>>() != null);

            var exceptions = new List<Exception>();

            for (var i = 0; i<maxRetries; i++)
            {
                try
                {
                    // execute the method and if all is kosher - return the completed task
                    return await method().ConfigureAwait(false);
                }
                catch (Exception x)
                {
                    exceptions.Add(x);

                    // if retried maxRetries times already - throw an aggregate exception with a history of all exceptions caught so far.
                    if (i == maxRetries-1)
                        throw new AggregateException(exceptions);
                }

                // figure out if we should retry and if not, throw an aggregate exception with a history of all exceptions caught so far.
                if (!await shouldRetry(exceptions.Last()).ConfigureAwait(false))
                    throw new AggregateException(exceptions);
            }

            return default(T);
        }

        /// <summary>
        /// <c>OnlyOne</c> executes a number of cancel-able methods contained in the sequence <paramref name="methods"/>.
        /// When one of the methods finishes first successfully, it will provide the result for the whole operation. 
        /// The rest of the methods are canceled.
        /// </summary>
        /// <typeparam name="T">The type of the result value of <paramref name="methods"/>.</typeparam>
        /// <param name="methods">The methods to be executed in parallel.</param>
        /// <returns>Task{T}.</returns>
        /// <example>
        /// Usage:
        /// <code>
        /// var price = await OnlyOne(
        ///                     ct => GetPriceFromSource1Async("abc", ct),
        ///                     ct => GetPriceFromSource2Async("abc", ct),
        ///                     ct => GetPriceFromSource3Async("abc", ct));
        /// </code>
        /// </example>
        public static async Task<T> OnlyOne<T>(
            IEnumerable<Func<CancellationToken, Task<T>>> methods)
        {
            Contract.Requires<ArgumentNullException>(methods != null, nameof(methods));
            Contract.Requires<ArgumentException>(methods.All(m => m!=null), "None of the methods in the array can be null.");
            Contract.Ensures(Contract.Result<Task<T>>() != null);

            var cts = new CancellationTokenSource();
            var tasks = methods.Select(f => f(cts.Token)).ToArray();
            var completed = await Task.WhenAny(tasks).ConfigureAwait(false);

            // cancel the rest of the methods
            cts.Cancel();

            // log the exceptions from the failed ones
            foreach (var task in tasks)
                await task.ContinueWith(
                        t => Facility.LogWriter.ExceptionError(t.Exception),
                        TaskContinuationOptions.OnlyOnFaulted);

            return completed.Result;
        }

        /// <summary>
        /// <c>OnlyOne</c> executes a number of cancel-able methods contained in the array <paramref name="methods"/>.
        /// When one of the methods finishes first successfully, it will provide the result for the whole operation. 
        /// The rest of the methods are canceled.
        /// </summary>
        /// <typeparam name="T">The type of the result value of <paramref name="methods"/>.</typeparam>
        /// <param name="methods">The methods to be executed in parallel.</param>
        /// <returns>Task{T}.</returns>
        /// <example>
        /// Usage:
        /// <code>
        /// var price = await OnlyOne(
        ///                     ct => GetPriceFromSource1Async("abc", ct),
        ///                     ct => GetPriceFromSource2Async("abc", ct),
        ///                     ct => GetPriceFromSource3Async("abc", ct));
        /// </code>
        /// </example>
        public static async Task<T> OnlyOne<T>(
            params Func<CancellationToken, Task<T>>[] methods)
        {
            Contract.Requires<ArgumentNullException>(methods != null, nameof(methods));
            Contract.Requires<ArgumentException>(methods.All(m => m!=null), "None of the methods in the array can be null.");
            Contract.Ensures(Contract.Result<Task<T>>() != null);

            var cts = new CancellationTokenSource();
            var tasks = methods.Select(f => f(cts.Token)).ToArray();
            var completed = await Task.WhenAny(tasks).ConfigureAwait(false);

            // cancel the rest of the methods
            cts.Cancel();

            // log the exceptions from the failed ones
            foreach (var task in tasks)
                await task.ContinueWith(
                        t => Facility.LogWriter.ExceptionError(t.Exception),
                        TaskContinuationOptions.OnlyOnFaulted);

            return completed.Result;
        }

        /// <summary>
        /// Allows for interleaving the specified parallel input tasks with some work on the current thread.
        /// </summary>
        /// <typeparam name="T">The type of the result value of <paramref name="inputTasks"/>.</typeparam>
        /// <returns>IEnumerable{Task{T}}.</returns>
        /// <example>
        /// <code>
        /// IEnumerable{Task{int}} tasks = ...
        /// 
        /// foreach (task in Interleaved(tasks))
        /// {
        ///     var result = await task;
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static IEnumerable<Task<T>> Interleaved<T>(
            IEnumerable<Task<T>> inputTasks)
        {
            Contract.Requires<ArgumentNullException>(inputTasks != null, nameof(inputTasks));
            Contract.Requires<ArgumentException>(inputTasks.All(t => t!=null), "None of the tasks in the sequence can be null.");
            Contract.Ensures(Contract.Result<IEnumerable<Task<T>>>() != null);
            Contract.Ensures(Contract.Result<IEnumerable<Task<T>>>().All(t => t!=null));

            var tasks = inputTasks.ToList();

            // create an array of TCS-s
            var sources = Enumerable.Range(0, tasks.Count)
                                    .Select(_ => new TaskCompletionSource<T>())
                                    .ToArray();
            var nextTaskIndex = -1;

            foreach (var task in tasks)
                task.ContinueWith(
                        completed =>
                        {
                            // get the first available TCS
                            var source = sources[Interlocked.Increment(ref nextTaskIndex)];

                            // set the TCS completion as appropriate
                            if (completed.IsFaulted)
                                source.TrySetException(completed.Exception.InnerExceptions);
                            else
                            if (completed.IsCanceled)
                                source.TrySetCanceled();
                            else
                                source.TrySetResult(completed.Result);
                        },
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default);

            // return the tasks from the TCS-s 
            return sources.Select(s => s.Task);
        }

        /// <summary>
        /// Allows for interleaving the specified parallel input tasks with some work on the current thread.
        /// </summary>
        /// <typeparam name="T">The type of the result value of <paramref name="inputTasks"/>.</typeparam>
        /// <returns>IEnumerable{Task{T}}.</returns>
        /// <example>
        /// <code>
        /// IEnumerable{Task{int}} tasks = ...
        /// 
        /// foreach (task in Interleaved(tasks))
        /// {
        ///     var result = await task;
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static IEnumerable<Task<T>> Interleaved<T>(
            params Task<T>[] inputTasks)
        {
            Contract.Requires<ArgumentNullException>(inputTasks != null, nameof(inputTasks));
            Contract.Requires<ArgumentException>(inputTasks.All(t => t!=null), "None of the tasks in the sequence can be null.");
            Contract.Ensures(Contract.Result<IEnumerable<Task<T>>>() != null);
            Contract.Ensures(Contract.Result<IEnumerable<Task<T>>>().All(t => t!=null));

            // create an array of TCS-s
            var sources = Enumerable.Range(0, inputTasks.Length)
                                    .Select(_ => new TaskCompletionSource<T>())
                                    .ToArray();
            var nextTaskIndex = -1;

            foreach (var task in inputTasks)
                task.ContinueWith(
                        completed =>
                        {
                            // get the first available TCS
                            var source = sources[Interlocked.Increment(ref nextTaskIndex)];

                            // set the TCS completion as appropriate
                            if (completed.IsFaulted)
                                source.TrySetException(completed.Exception.InnerExceptions);
                            else
                                if (completed.IsCanceled)
                                source.TrySetCanceled();
                            else
                                source.TrySetResult(completed.Result);
                        },
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default);

            // return the tasks from the TCS-s 
            return sources.Select(s => s.Task);
        }

        /// <summary>
        /// Waits for the completion of all tasks or until the first exception is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the result value of <paramref name="inputTasks"/>.</typeparam>
        /// <param name="inputTasks">The input tasks.</param>
        /// <returns>Task{T[]}.</returns>
        public static Task<T[]> WhenAllOrFirstException<T>(
            IEnumerable<Task<T>> inputTasks)
        {
            Contract.Requires<ArgumentNullException>(inputTasks != null, nameof(inputTasks));
            Contract.Requires<ArgumentException>(inputTasks.All(t => t!=null), "None of the tasks in the sequence can be null.");
            Contract.Ensures(Contract.Result<Task<T[]>>() != null);

            var tasks = inputTasks.ToList();
            var countdown = new CountdownEvent(tasks.Count);
            var tcs = new TaskCompletionSource<T[]>();

            foreach (var task in tasks)
                task.ContinueWith(
                    completed =>
                    {
                        if (completed.IsFaulted)
                            tcs.TrySetException(completed.Exception.InnerExceptions);
                        if (countdown.Signal() && !tcs.Task.IsCompleted)
                            tcs.TrySetResult(tasks.Select(t => t.Result).ToArray());
                    });

            return tcs.Task;
        }

        /// <summary>
        /// Waits for the completion of all tasks or until the first exception is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the result value of <paramref name="inputTasks"/>.</typeparam>
        /// <param name="inputTasks">The input tasks.</param>
        /// <returns>Task{T[]}.</returns>
        public static Task<T[]> WhenAllOrFirstException<T>(
            params Task<T>[] inputTasks)
        {
            Contract.Requires<ArgumentNullException>(inputTasks != null, nameof(inputTasks));
            Contract.Requires<ArgumentException>(inputTasks.All(t => t!=null), "None of the tasks can be null.");
            Contract.Ensures(Contract.Result<Task<T[]>>() != null);

            var countdown = new CountdownEvent(inputTasks.Length);
            var tcs = new TaskCompletionSource<T[]>();

            foreach (var task in inputTasks)
                task.ContinueWith(
                    completed =>
                    {
                        if (completed.IsFaulted)
                            tcs.TrySetException(completed.Exception.InnerExceptions);
                        if (countdown.Signal() && !tcs.Task.IsCompleted)
                            tcs.TrySetResult(inputTasks.Select(t => t.Result).ToArray());
                    });

            return tcs.Task;
        }
    }
}
