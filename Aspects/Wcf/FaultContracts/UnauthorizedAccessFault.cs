using System;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors <see cref="UnauthorizedAccessException"/>
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    public sealed class UnauthorizedAccessFault : Fault
    {
    }
}
