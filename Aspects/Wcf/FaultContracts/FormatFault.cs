using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class FormatFault. Mirrors <see cref="T:FormatException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:vm.Aspects.Wcf")]
    public sealed class FormatFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatFault"/> class.
        /// </summary>
        public FormatFault()
            : base(HttpStatusCode.InternalServerError)
        {
        }
    }
}
