using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class FormatFault. Mirrors <see cref="T:FormatException"/>.
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    public sealed class FormatFault : Fault
    {
    }
}
