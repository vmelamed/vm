using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class DataFault. This class cannot be inherited.
    /// </summary>
    [DataContract(Namespace = "urn:vm.Aspects.Wcf")]
    public sealed class DataFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataFault"/> class.
        /// </summary>
        public DataFault()
            : base(HttpStatusCode.InternalServerError)
        {
        }
    }
}
