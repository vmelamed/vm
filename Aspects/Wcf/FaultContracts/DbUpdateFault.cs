using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class DataFault. This class cannot be inherited.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class DbUpdateFault : DataFault
    {
    }
}
