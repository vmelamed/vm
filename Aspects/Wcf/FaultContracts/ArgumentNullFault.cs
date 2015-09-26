using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors ArgumentNullException.
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    public sealed class ArgumentNullFault : ArgumentFault
    {
    }
}
