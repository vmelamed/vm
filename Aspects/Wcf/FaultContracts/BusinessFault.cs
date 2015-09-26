using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class BusinessFault. Corresponds to <see cref="T:BusinessException"/>
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    public class BusinessFault : Fault
    {
    }
}
