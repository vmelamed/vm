using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Corresponds to the <see cref="T:ObjectNotFoundException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class ObjectNotFoundFault : ObjectFault
    {
    }
}
