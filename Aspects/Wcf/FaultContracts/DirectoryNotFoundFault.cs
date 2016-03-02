using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class DirectoryNotFoundFault. This class cannot be inherited.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public sealed class DirectoryNotFoundFault : IOFault
    {
    }
}
