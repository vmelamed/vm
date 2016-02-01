using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors the <see cref="T:ObjectIdentifierNotUniqueException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class ObjectIdentifierNotUniqueFault : ObjectFault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifierNotUniqueFault"/> class.
        /// </summary>
        public ObjectIdentifierNotUniqueFault()
            : base(HttpStatusCode.BadRequest)
        {
        }
    }
}
