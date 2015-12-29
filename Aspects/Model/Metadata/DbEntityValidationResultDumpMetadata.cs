using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.Metadata
{
    /// <summary>
    /// Class DbEntityValidationResultDumpMetadata.
    /// </summary>
    public abstract class DbEntityValidationResultDumpMetadata
    {
        /// <summary>
        /// The entry
        /// </summary>
        [Dump(0)]
        public object Entry;

        /// <summary>
        /// The is valid
        /// </summary>
        [Dump(1)]
        public object IsValid;

        /// <summary>
        /// The validation errors
        /// </summary>
        [Dump(2)]
        public object ValidationErrors;
    }
}
