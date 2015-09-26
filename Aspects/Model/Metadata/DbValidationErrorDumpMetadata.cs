using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.Metadata
{
    /// <summary>
    /// Class DbValidationErrorDumpMetadata.
    /// </summary>
    public abstract class DbValidationErrorDumpMetadata
    {
        /// <summary>
        /// The property name
        /// </summary>
        [Dump(0)]
        public object PropertyName;

        /// <summary>
        /// The error message
        /// </summary>
        [Dump(1)]
        public object ErrorMessage;
    }
}
