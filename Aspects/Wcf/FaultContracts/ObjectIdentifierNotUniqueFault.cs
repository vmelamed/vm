using System.Runtime.Serialization;
using vm.Aspects.Exceptions;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors the <see cref="ObjectIdentifierNotUniqueException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class ObjectIdentifierNotUniqueFault : ObjectFault
    {
    }
}
