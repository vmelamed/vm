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
    /// Class AggregateFault. Mirrors <see cref="AggregateException"/>.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    [DebuggerDisplay("{GetType().Name, nq}:: {Message} Parameter: {ParamName, nq}")]
    [MetadataType(typeof(AggregateFaultMetadata))]
    public sealed class AggregateFault : Fault
    {
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

                HttpStatusCode httpStatusCode = HttpStatusCode.OK;

                using (var w = new StringWriter(CultureInfo.InvariantCulture))
                {
                    foreach (var ie in value)
                    {
                        if (httpStatusCode == HttpStatusCode.OK  &&
                            ExceptionToHttpStatusCode.TryGetValue(ie.GetType(), out httpStatusCode))
                            HttpStatusCode = httpStatusCode;

                        w.WriteLine(ie.Message);
                    }

                    InnerExceptionsMessages = w.GetStringBuilder().ToString();
                }
            }
        }
    }
}
