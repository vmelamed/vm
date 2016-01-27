using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Class AggregateFault. Mirrors <see cref="T:AggregateException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:vm.Aspects.Wcf")]
    [DebuggerDisplay("{GetType().Name, nq}:: {Message} Parameter: {ParamName, nq}")]
    [MetadataType(typeof(AggregateFaultMetadata))]
    public sealed class AggregateFault : Fault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateFault"/> class.
        /// </summary>
        public AggregateFault()
            : base(HttpStatusCode.InternalServerError)
        {
        }

        /// <summary>
        /// Gets or sets the inner exceptions.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "N/A")]
        public ReadOnlyCollection<Exception> InnerExceptions
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<Exception>>() == null);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                using (var w = new StringWriter(CultureInfo.InvariantCulture))
                {
                    foreach (var ie in value)
                        w.WriteLine(ie.Message);

                    InnerExceptionsMessages = w.GetStringBuilder().ToString();
                }
            }
        }

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        void ObjectInvariant()
        {
            Contract.Invariant(InnerExceptions == null);
        }

    }
}
