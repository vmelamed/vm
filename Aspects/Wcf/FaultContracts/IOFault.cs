using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class IOFault. Mirrors <see cref="T:IOException"/>
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public class IOFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IOFault"/> class.
        /// </summary>
        public IOFault()
            : base(HttpStatusCode.InternalServerError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IOFault" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        protected IOFault(
            HttpStatusCode httpStatusCode)
            : base(httpStatusCode)
        {
            Contract.Requires<ArgumentException>(httpStatusCode >= (HttpStatusCode)400, "The faults describe exceptional fault situations and should have status greater or equal to 400 (HTTP 400 Bad request.)");
        }
    }
}
