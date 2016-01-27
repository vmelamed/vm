using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class RepeatableOperationFault.
    /// </summary>
    [DataContract(Namespace = "urn:vm.Aspects.Wcf")]
    public class RepeatableOperationFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatableOperationFault"/> class.
        /// </summary>
        public RepeatableOperationFault()
            : base(HttpStatusCode.GatewayTimeout)
        {
        }
    }
}
