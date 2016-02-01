using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class DirectoryNotFoundFault. This class cannot be inherited.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class DirectoryNotFoundFault : IOFault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryNotFoundFault"/> class.
        /// </summary>
        public DirectoryNotFoundFault()
            : base(HttpStatusCode.NotFound)
        {
        }
    }
}
