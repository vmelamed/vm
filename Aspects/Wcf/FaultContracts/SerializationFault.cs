using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors <see cref="SerializationException"/>
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class SerializationFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationFault"/> class.
        /// </summary>
        public SerializationFault()
            : base(HttpStatusCode.BadRequest)
        {
        }
    }
}
