using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model.Metadata
{
    /// <summary>
    /// Class ObjectStateEntryDumpMetadata.
    /// </summary>
    [Dump(RecurseDump=ShouldDump.Skip)]
    public abstract class ObjectStateEntryDumpMetadata
    {
    }
}
