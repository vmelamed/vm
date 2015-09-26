using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Dump(RecurseDump=ShouldDump.Skip)]
        public object Entry;

        /// <summary>
        /// The is valid
        /// </summary>
        [Dump(0)]
        public object IsValid;

        /// <summary>
        /// The validation errors
        /// </summary>
        [Dump(1)]
        public object ValidationErrors;        
    }
}
