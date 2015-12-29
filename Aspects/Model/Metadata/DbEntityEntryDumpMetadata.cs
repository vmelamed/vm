using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.Metadata
{
    /// <summary>
    /// Class DbEntityEntryDumpMetadata.
    /// </summary>
    public abstract class DbEntityEntryDumpMetadata
    {
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [Dump(0)]
        public object State { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        [Dump(1)]
        public object Entity { get; set; }

        /// <summary>
        /// Gets or sets the current values.
        /// </summary>
        [Dump(false)]
        public object CurrentValues { get; set; }

        /// <summary>
        /// Gets or sets the internal entry.
        /// </summary>
        [Dump(false)]
        public object InternalEntry { get; set; }

        /// <summary>
        /// Gets or sets the original values.
        /// </summary>
        [Dump(false)]
        public object OriginalValues { get; set; }


    }
}
