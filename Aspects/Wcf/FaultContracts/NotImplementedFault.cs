using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class NotImplementedFault. Mirrors <see cref="T:NotImplementedException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class NotImplementedFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotImplementedFault"/> class.
        /// </summary>
        public NotImplementedFault()
            : base(HttpStatusCode.NotImplemented)
        {
        }
    }
}
