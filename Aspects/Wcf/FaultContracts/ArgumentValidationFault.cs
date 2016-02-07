using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors the <see cref="T:ArgumentValidationException"/> from the Enterprise Library.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    [MetadataType(typeof(ArgumentValidationFaultMetadata))]
    public sealed class ArgumentValidationFault : ArgumentFault
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentValidationFault"/> class.
        /// </summary>
        public ArgumentValidationFault()
        {
            ValidationElements = new List<ValidationFaultElement>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the nested validation results from a composite failed validation.
        /// </summary>
        [DataMember]
        public ICollection<ValidationFaultElement> ValidationElements { get; private set; }

        /// <summary>
        /// Allows for member-wise copying of <c>ArgumentValidationException.ValidationElements</c> by the
        /// exception shielding handlers.
        /// </summary>
        public ValidationResults ValidationResults
        {
            get
            {
                Contract.Ensures(Contract.Result<ValidationResults>() == null);

                return null;
            }

            set
            {
                if (ValidationElements == null)
                    ValidationElements = new List<ValidationFaultElement>();

                if (value == null)
                    return;

                value.CopyTo(ValidationElements);

                if (string.IsNullOrEmpty(Message))
                    return;

                // append the validation messages to the existing message:
                using (var textWriter = new StringWriter(new StringBuilder(base.Message), CultureInfo.InvariantCulture))
                {
                    foreach (var element in value)
                        element.DumpText(textWriter, 1);

                    base.Message = textWriter.GetStringBuilder().ToString();
                }
            }
        }

        /// <summary>
        /// Gets or sets the fault's message
        /// </summary>
        [DataMember]
        public override string Message
        {
            get { return base.Message; }

            set
            {
                if (ValidationElements == null  ||  ValidationElements.Count == 0)
                    base.Message = value;
                else
                    // append the validation messages to the passed in message:
                    using (var textWriter = new StringWriter(new StringBuilder(value), CultureInfo.InvariantCulture))
                    {
                        foreach (var element in ValidationElements)
                            element.DumpText(textWriter, 1);

                        base.Message = textWriter.GetStringBuilder().ToString();
                    }
            }
        }
        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        void ObjectInvariant()
        {
            Contract.Invariant(ValidationResults == null);
        }
    }
}
