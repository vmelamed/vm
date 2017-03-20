namespace vm.Aspects.Model
{
    /// <summary>
    /// OptimisticConcurrencyStrategy defines the strategies for handling optimistic concurrency exceptions (<see cref="OptimisticConcurrencyException"/>)
    /// </summary>
    public enum OptimisticConcurrencyStrategy
    {
        /// <summary>
        /// If the store contains newer values for the object - do not allow changes and extend the exception to the client.
        /// </summary>
        StoreWins,

        /// <summary>
        /// Even if the store contains newer values for the object - the client values are considered with higher priority and the changes are allowed nevertheless.
        /// </summary>
        ClientWins,
    }
}
