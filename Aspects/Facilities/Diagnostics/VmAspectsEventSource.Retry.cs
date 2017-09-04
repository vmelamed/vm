using System.Diagnostics.Tracing;
using vm.Aspects.Threading;

namespace vm.Aspects.Facilities.Diagnostics
{
    public sealed partial class VmAspectsEventSource
    {
        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}"/> starts the retry cycle of an operation.
        /// </summary>
        /// <param name="retry">The retry-er.</param>
        [NonEvent]
        public void RetryStart<T>(
            Retry<T> retry)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.Retry))
                RetryStart(retry.GetType().FullName);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="RetryTasks{T}"/> starts the retry cycle of an operation.
        /// </summary>
        /// <param name="retry">The retry-er.</param>
        [NonEvent]
        public void RetryStart<T>(
            RetryTasks<T> retry)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.Retry))
                RetryStart(retry.GetType().FullName);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}"/> stops the retry cycle of an operation.
        /// </summary>
        /// <param name="retry">The retry-er.</param>
        [NonEvent]
        public void RetryStop<T>(
            Retry<T> retry)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.Retry))
                RetryStop(retry.GetType().FullName);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="RetryTasks{T}"/> stops the retry cycle of an operation.
        /// </summary>
        /// <param name="retry">The retry-er.</param>
        [NonEvent]
        public void RetryStop<T>(
            RetryTasks<T> retry)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.Retry))
                RetryStop(retry.GetType().FullName);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}"/> retries an operation.
        /// </summary>
        /// <param name="retry">The retry-er.</param>
        /// <param name="delay">The delay.</param>
        [NonEvent]
        public void Retrying<T>(
            Retry<T> retry,
            int delay)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.Retry))
                Retrying(retry.GetType().FullName, delay);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="RetryTasks{T}"/> retries an operation.
        /// </summary>
        /// <param name="retry">The retry-er.</param>
        /// <param name="delay">The delay.</param>
        [NonEvent]
        public void Retrying<T>(
            RetryTasks<T> retry,
            int delay)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.Retry))
                Retrying(retry.GetType().FullName, delay);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}" /> retries an operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="retry">The retry-er.</param>
        /// <param name="retries">Number of retries.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        [NonEvent]
        public void RetryFailed<T>(
            Retry<T> retry,
            int retries,
            int maxRetries)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.Retry))
                RetryFailed(retry.GetType().FullName, retries, maxRetries);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="RetryTasks{T}" /> retries an operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="retry">The retry-er.</param>
        /// <param name="retries">The delay.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        [NonEvent]
        public void RetryFailed<T>(
            RetryTasks<T> retry,
            int retries,
            int maxRetries)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.Retry))
                RetryFailed(retry.GetType().FullName, retries, maxRetries);
        }

        // ===========================================================================

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}"/> retries an operation.
        /// </summary>
        /// <param name="retryType">Type of the retry.</param>
        [Event(RetryStartId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Retry, Message = "{0} starts.")]
        void RetryStart(
            string retryType)
        {
            if (IsEnabled())
                WriteEvent(RetryStartId, retryType);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}"/> retries an operation.
        /// </summary>
        /// <param name="retryType">Type of the retry.</param>
        [Event(RetryStopId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Retry, Message = "{0} stops.")]
        void RetryStop(
            string retryType)
        {
            if (IsEnabled())
                WriteEvent(RetryStopId, retryType);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}"/> retries an operation.
        /// </summary>
        /// <param name="retryType">Type of the retry.</param>
        /// <param name="delay">The delay.</param>
        [Event(RetryingId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Retry, Message = "{0} again after {1}msec delay.")]
        void Retrying(
            string retryType,
            int delay)
        {
            if (IsEnabled())
                WriteEvent(RetryingId, retryType, delay);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}" /> retries an operation.
        /// </summary>
        /// <param name="retryType">Type of the retry.</param>
        /// <param name="retries">Number of retries.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        [Event(RetryFailedId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Retry, Message = "{0} failed after {1}/{2} retries.")]
        void RetryFailed(
            string retryType,
            int retries,
            int maxRetries)
        {
            if (IsEnabled())
                WriteEvent(RetryFailedId, retryType, retries, maxRetries);
        }
    }
}
