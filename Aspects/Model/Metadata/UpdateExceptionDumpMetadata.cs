using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.Metadata
{
    /// <summary>
    /// Class UpdateExceptionDumpMetadata.
    /// </summary>
    public abstract class UpdateExceptionDumpMetadata
    {
        /// <summary>
        /// Gets or sets the state entries.
        /// </summary>
        [Dump(RecurseDump=ShouldDump.Skip)]
        public object StateEntries;
    }
}
