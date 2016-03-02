using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class PathTooLongFault. This class cannot be inherited. Mirrors <see cref="T:PathTooLongException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class PathTooLongFault : IOFault
    {
    }
}
