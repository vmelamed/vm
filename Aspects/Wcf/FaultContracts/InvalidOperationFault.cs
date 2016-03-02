using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class InvalidOperationFault. This class cannot be inherited. Mirrors <see cref="T:InvalidOperationException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class InvalidOperationFault : Fault
    {
    }
}
