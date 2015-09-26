using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class IOFault. Mirrors <see cref="T:IOException"/>
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    public class IOFault : Fault
    {
    }
}
