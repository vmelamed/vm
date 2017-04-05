using System.Data.Entity.Infrastructure;
using vm.Aspects.Exceptions;

namespace vm.Aspects.Model
{
    /// <summary>
    /// OptimisticConcurrencyStrategy defines the strategies for handling optimistic concurrency exceptions (<see cref="DbUpdateConcurrencyException"/>)
    /// </summary>
    public enum OptimisticConcurrencyStrategy
    {
        /// <summary>
        /// No concurrency strategy is applied, i.e. all concurrency related exceptions are rethrown without any preprocessing.
        /// </summary>
        None,

        /// <summary>
        /// If the store contains newer values for the object - do not allow the changes to propagate to the store but 
        /// extend the concurrency exceptions to the client wrapped in <see cref="RepeatableOperationException"/>.
        /// The rest of the transient exceptions are also similarly wrapped.
        /// </summary>
        StoreWins,

        /// <summary>
        /// Even if the store contains newer values for the object - the client values are considered with higher priority and the changes are allowed nevertheless by
        /// swallowing the <see cref="DbUpdateConcurrencyException"/>-s and allowing for retrying the commit operation,
        /// the rest of the transient DB related exceptions are wrapped in <see cref="RepeatableOperationException"/>.
        /// </summary>
        ClientWins,
    }
}
