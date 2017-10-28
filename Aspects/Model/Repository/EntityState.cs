namespace vm.Aspects.Model.Repository
{
    /// <summary>
    /// Enum EntityState. Characterizes the state of an object with regards to its persistence in <see cref="IRepository"/>.
    /// </summary>
    public enum EntityState
    {
        /// <summary>
        /// The object was not modified.
        /// </summary>
        Unchanged,
        /// <summary>
        /// The object was added to the repository.
        /// </summary>
        Added,
        /// <summary>
        /// The object was modified.
        /// </summary>
        Modified,
        /// <summary>
        /// The object was deleted the repository.
        /// </summary>
        Deleted,
    }
}