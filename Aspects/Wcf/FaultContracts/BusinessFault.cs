using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.Serialization;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class BusinessFault. Corresponds to <see cref="T:BusinessException"/>
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    public class BusinessFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessFault"/> class.
        /// </summary>
        public BusinessFault()
            : base(HttpStatusCode.Forbidden)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessFault" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        protected BusinessFault(
            HttpStatusCode httpStatusCode)
            : base(httpStatusCode)
        {
            Contract.Requires<ArgumentException>(httpStatusCode >= (HttpStatusCode)400, "The faults describe exceptional fault situations and should have status greater or equal to 400 (HTTP 400 Bad request.)");
        }
    }
}
