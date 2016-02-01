using System;
using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors <see cref="UnauthorizedAccessException"/>
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class UnauthorizedAccessFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedAccessFault"/> class.
        /// </summary>
        public UnauthorizedAccessFault()
            : base(HttpStatusCode.Unauthorized)
        {
        }
    }
}
