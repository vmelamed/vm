using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.Serialization;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// This class is used to return information to a WCF
    /// client when validation fails on a service parameter.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    [MetadataType(typeof(ValidationFaultMetadata))]
    public sealed class ValidationFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFault"/> class.
        /// </summary>
        public ValidationFault()
            : base(HttpStatusCode.BadRequest)
        {
            InternalDetails = new List<ValidationFaultElement>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFault"/> class.
        /// </summary>
        /// <param name="details">The details.</param>
        public ValidationFault(
            IEnumerable<ValidationFaultElement> details)
            : base(HttpStatusCode.BadRequest)
        {
            Contract.Requires<ArgumentNullException>(details != null, nameof(details));

            InternalDetails = new List<ValidationFaultElement>(details);
        }

        /// <summary>
        /// Adds the specified detail.
        /// </summary>
        /// <param name="detail">The detail.</param>
        public void Add(
            ValidationFaultElement detail)
        {
            Contract.Requires<ArgumentNullException>(detail != null, nameof(detail));

            InternalDetails.Add(detail);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsValid => InternalDetails.Count == 0;

        [DataMember(Name = "internalDetails")]
        IList<ValidationFaultElement> InternalDetails { get; }

        /// <summary>
        /// Gets the validation details.
        /// </summary>
        public IReadOnlyList<ValidationFaultElement> Details
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<ValidationFaultElement>>() != null);

                return new ReadOnlyCollection<ValidationFaultElement>(InternalDetails);
            }
        }
    }
}
