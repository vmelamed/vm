using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Fault and Exception Extensions which populate their Data collections
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Populates the <see cref="Exception.Data" /> dictionary with data from a corresponding (e.g. from <see cref="Fault" />) dictionary.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exception">The exception that needs to be populated.</param>
        /// <param name="data">The data.</param>
        /// <returns>Exception.</returns>
        public static TException PopulateData<TException>(
            this TException exception,
            IDictionary<string, string> data)
            where TException : Exception
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
            Contract.Requires<ArgumentNullException>(data      != null, nameof(data));
            Contract.Ensures(Contract.Result<Exception>() != null);

            if (data.Count == 0)
                return exception;

            data.Select(kv => exception.Data[kv.Key] = kv.Value).Count();

            return exception;
        }

        /// <summary>
        /// Populates the <see cref="Fault.Data"/> dictionary with data from a corresponding (e.g. from <see cref="Exception"/>) dictionary.
        /// </summary>
        /// <param name="fault">The fault that needs to be populated.</param>
        /// <param name="data">The data.</param>
        /// <returns>Fault.</returns>
        public static TFault PopulateFaultData<TFault>(
            this TFault fault,
            IDictionary data)
            where TFault : Fault
        {
            Contract.Requires<ArgumentNullException>(fault != null, nameof(fault));
            Contract.Requires<ArgumentNullException>(data  != null, nameof(data));
            Contract.Ensures(Contract.Result<Fault>() != null);

            if (data.Count == 0)
                return fault;

            foreach (DictionaryEntry kv in data)
                fault.Data[kv.Key?.ToString()] = kv.Value?.ToString();

            return fault;
        }
    }
}
