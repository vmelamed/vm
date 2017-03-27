using System.Runtime.Serialization;
using vm.Aspects.Exceptions;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class BusinessFault. Corresponds to <see cref="BusinessException"/>
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public class BusinessFault : Fault
    {
    }
}
