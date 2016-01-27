using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class PathTooLongFault. This class cannot be inherited. Mirrors <see cref="T:PathTooLongException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:vm.Aspects.Wcf")]
    public sealed class PathTooLongFault : IOFault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathTooLongFault"/> class.
        /// </summary>
        public PathTooLongFault()
            : base(HttpStatusCode.BadRequest)
        {
        }
    }
}
