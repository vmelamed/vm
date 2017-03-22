using System;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using vm.Aspects.Facilities;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Class ConcurrencyExceptionHandler handles <see cref="DbUpdateConcurrencyException"/>.
    /// </summary>
    public class OptimisticConcurrencyExceptionHandler
    {
        /// <summary>
        /// The default optimistic concurrency strategy.
        /// </summary>
        public const OptimisticConcurrencyStrategy DefaultOptimisticConcurrencyStrategy = OptimisticConcurrencyStrategy.ClientWins;

        /// <summary>
        /// The default maximum optimistic concurrency retries.
        /// </summary>
        public const int DefaultMaxOptimisticConcurrencyRetries = 10;

        /// <summary>
        /// The default minimum delay before the retry after an optimistic concurrency exception.
        /// </summary>
        public const int DefaultMinDelayBeforeRetry = 20;

        /// <summary>
        /// The default maximum delay before the retry after an optimistic concurrency exception.
        /// </summary>
        public const int DefaultMaxDelayBeforeRetry = 200;

        /// <summary>
        /// Gets or sets the optimistic concurrency strategy.
        /// </summary>
        public OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        /// <summary>
        /// Gets or sets the maximum optimistic concurrency retries.
        /// </summary>
        public int MaxOptimisticConcurrencyRetries { get; set; }

        /// <summary>
        /// The minimum delay before the retry after an optimistic concurrency exception.
        /// </summary>
        public int MinDelayBeforeRetry { get; set; }

        /// <summary>
        /// The maximum delay before the retry after an optimistic concurrency exception.
        /// </summary>
        public int MaxDelayBeforeRetry { get; set; }

        /// <summary>
        /// Gets the attempts count.
        /// </summary>
        public int Attempts { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log a warning for the exception.
        /// </summary>
        public bool LogExceptionWarnings { get; set; }

        Random _random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Initializes a new instance of the <see cref="OptimisticConcurrencyExceptionHandler" /> class.
        /// </summary>
        /// <param name="optimisticConcurrencyStrategy">The optimistic concurrency strategy.</param>
        /// <param name="maxOptimisticConcurrencyRetries">The maximum optimistic concurrency retries.</param>
        /// <param name="minDelayBeforeRetry">The minimum delay before retry.</param>
        /// <param name="maxDelayBeforeRetry">The maximum delay before retry.</param>
        /// <param name="logExceptionWarnings">if set to <see langword="true" /> the handler will log a warning for the exception.</param>
        public OptimisticConcurrencyExceptionHandler(
            OptimisticConcurrencyStrategy optimisticConcurrencyStrategy = DefaultOptimisticConcurrencyStrategy,
            int maxOptimisticConcurrencyRetries = DefaultMaxOptimisticConcurrencyRetries,
            int minDelayBeforeRetry = DefaultMinDelayBeforeRetry,
            int maxDelayBeforeRetry = DefaultMaxDelayBeforeRetry,
            bool logExceptionWarnings = true)
        {
            Contract.Requires<ArgumentException>(maxOptimisticConcurrencyRetries >= 0, nameof(maxOptimisticConcurrencyRetries)+" cannot be negative");
            Contract.Requires<ArgumentException>(minDelayBeforeRetry             >= 0, nameof(minDelayBeforeRetry)+" cannot be negative");
            Contract.Requires<ArgumentException>(maxDelayBeforeRetry             >= 0, nameof(maxDelayBeforeRetry)+" cannot be negative");

            OptimisticConcurrencyStrategy   = optimisticConcurrencyStrategy;
            MaxOptimisticConcurrencyRetries = maxOptimisticConcurrencyRetries;
            MinDelayBeforeRetry             = MinDelayBeforeRetry;
            MaxDelayBeforeRetry             = maxDelayBeforeRetry;
            LogExceptionWarnings            = logExceptionWarnings;
        }

        /// <summary>
        /// Handles the database update concurrency exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void HandleDbUpdateConcurrencyException(
            DbUpdateConcurrencyException exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            // see https://msdn.microsoft.com/en-us/data/jj592904.aspx
            if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.StoreWins  ||
                Attempts == MaxOptimisticConcurrencyRetries-1)
                throw exception;

            Attempts++;

            if (LogExceptionWarnings)
                Facility.LogWriter.ExceptionWarning(exception);

            var entry = exception.Entries.Single();

            WaitBeforeRetry();
            entry.OriginalValues.SetValues(entry.GetDatabaseValues());
        }

        /// <summary>
        /// Handles the database update concurrency exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>Task.</returns>
        public async Task HandleDbUpdateConcurrencyExceptionAsync(
            DbUpdateConcurrencyException exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            // see https://msdn.microsoft.com/en-us/data/jj592904.aspx
            if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.StoreWins  ||
                Attempts == MaxOptimisticConcurrencyRetries-1)
                throw exception;

            Attempts++;

            if (LogExceptionWarnings)
                Facility.LogWriter.ExceptionWarning(exception);

            var entry = exception.Entries.Single();

            await WaitBeforeRetryAsync();
            entry.OriginalValues.SetValues(await entry.GetDatabaseValuesAsync());
        }

        void WaitBeforeRetry()
        {
            WaitBeforeRetryAsync().Wait();
        }

        async Task WaitBeforeRetryAsync()
        {
            await Task.Delay(MinDelayBeforeRetry + _random.Next(MaxDelayBeforeRetry));
        }
    }
}
