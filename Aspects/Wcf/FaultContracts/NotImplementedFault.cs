using System;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class NotImplementedFault. Mirrors <see cref="NotImplementedException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class NotImplementedFault : Fault
    {
    }
}
