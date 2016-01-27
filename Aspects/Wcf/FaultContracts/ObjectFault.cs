using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.Serialization;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class ObjectFault. Corresponds to <see cref="T:ObjectException"/>
    /// </summary>
    [DataContract(Namespace = "urn:vm.Aspects.Wcf")]
    [DebuggerDisplay("{GetType().Name, nq}:: {Message}: ObjectIdentifier: {ObjectIdentifier} (Type: {ObjectType})")]
    [MetadataType(typeof(ObjectFaultMetadata))]
    public abstract class ObjectFault : BusinessFault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFault"/> class.
        /// </summary>
        protected ObjectFault(HttpStatusCode httpStatusCode)
            : base(httpStatusCode)
        {
            Contract.Requires<ArgumentException>(httpStatusCode >= (HttpStatusCode)400, "The faults describe exceptional fault situations and should have status greater or equal to 400 (HTTP 400 Bad request.)");
        }

        /// <summary>
        /// Gets or sets the assembly fully qualified name of the type of the object which is at fault.
        /// </summary>
        /// <value>The type of the object.</value>
        [DataMember]
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the object identifier - the reference used by the client call.
        /// </summary>
        /// <value>The object identifier.</value>
        [DataMember]
        public string ObjectIdentifier { get; set; }
    }
}
