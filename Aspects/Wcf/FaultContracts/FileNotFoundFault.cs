using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class FileNotFoundFault. This class cannot be inherited. Mirrors <see cref="T:FileNotFoundException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    [DebuggerDisplay("{GetType().Name, nq}:: {Message}: ObjectIdentifier: {ObjectIdentifier} (Type: {ObjectType})")]
    [MetadataType(typeof(FileNotFoundFaultMetadata))]
    public sealed class FileNotFoundFault : IOFault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileNotFoundFault"/> class.
        /// </summary>
        public FileNotFoundFault()
            : base(HttpStatusCode.NotFound)
        {
        }

        /// <summary>
        /// Gets or sets the name of the file that cannot be found.
        /// </summary>
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the log file that describes why loading of an assembly failed.
        /// </summary>
        [DataMember(Name = "fusionLog")]
        public string FusionLog { get; set; }
    }
}
