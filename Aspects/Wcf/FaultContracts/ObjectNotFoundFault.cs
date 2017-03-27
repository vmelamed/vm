using System.Runtime.Serialization;
using vm.Aspects.Exceptions;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Corresponds to the <see cref="ObjectNotFoundException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class ObjectNotFoundFault : ObjectFault
    {
    }
}
