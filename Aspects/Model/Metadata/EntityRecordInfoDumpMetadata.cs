using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.Metadata
{
    /// <summary>
    /// Class EntityRecordInfoDumpMetadata.
    /// </summary>
    public abstract class EntityRecordInfoDumpMetadata
    {
        /// <summary>
        /// The entity key
        /// </summary>
        [Dump(false)]
        public object EntityKey { get; set; }

        /// <summary>
        /// The field metadata
        /// </summary>
        [Dump(false)]
        public object FieldMetadata { get; set; }

        /// <summary>
        /// The record type
        /// </summary>
        [Dump(false)]
        public object RecordType { get; set; }
    }
}
