using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class DirectoryNotFoundFault. This class cannot be inherited.
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    public sealed class DirectoryNotFoundFault : IOFault
    {
    }
}
