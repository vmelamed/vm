using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using vm.Aspects.Diagnostics;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// This class is used to return information to a WCF
    /// client when validation fails on a service parameter.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    [MetadataType(typeof(ValidationFaultMetadata))]
    public sealed class InvalidObjectFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidObjectFault"/> class.
        /// </summary>
        public InvalidObjectFault()
        {
            InternalDetails = new List<ValidationFaultElement>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidObjectFault"/> class.
        /// </summary>
        /// <param name="details">The details.</param>
        public InvalidObjectFault(
            IEnumerable<ValidationFaultElement> details)
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));

            InternalDetails = new List<ValidationFaultElement>(details);
        }

        /// <summary>
        /// Adds the specified detail.
        /// </summary>
        /// <param name="detail">The detail.</param>
        public void Add(
            ValidationFaultElement detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            InternalDetails.Add(detail);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsValid => InternalDetails.Count == 0;

        [DataMember(Name = "internalDetails")]
        [Dump(false)]
        IList<ValidationFaultElement> InternalDetails { get; set; }

        /// <summary>
        /// Gets the validation details.
        /// </summary>
        public IReadOnlyList<ValidationFaultElement> Details
        {
            get
            {
                
                return new ReadOnlyCollection<ValidationFaultElement>(InternalDetails);
            }

            internal set
            {
                InternalDetails = value.ToList();
            }
        }
    }
}
