using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.Metadata
{
    /// <summary>
    /// Class DbUpdateExceptionDumpMetadata.
    /// </summary>
    public abstract class DbUpdateExceptionDumpMetadata
    {
        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        //[Dump(RecurseDump=ShouldDump.Skip)]
        public object Entries;
    }
}
