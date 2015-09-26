using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors ArgumentException.
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    [DebuggerDisplay("{GetType().Name, nq}:: {Message} Parameter: {ParamName, nq}")]
    [MetadataType(typeof(ArgumentFaultMetadata))]
    public class ArgumentFault : Fault
    {
        #region Properties
        /// <summary>
        /// Gets or sets the name of the parameter that causes this exception.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Param", Justification="From ArgumentException")]
        public string ParamName { get; set; }
        #endregion
    }
}
