using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors the <see cref="T:ObjectIdentifierNotUniqueException"/>.
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    public sealed class ObjectIdentifierNotUniqueFault : ObjectFault
    {
    }
}
