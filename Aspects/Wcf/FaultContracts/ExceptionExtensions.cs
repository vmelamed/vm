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
        /// Populates the <see cref="Exception.Data"/> dictionary with data from a corresponding (e.g. from <see cref="Fault"/>) dictionary.
        /// </summary>
        /// <param name="ex">The exception that needs to be populated.</param>
        /// <param name="data">The data.</param>
        /// <returns>Exception.</returns>
        public static Exception PopulateData(
            this Exception ex,
            IDictionary<string, string> data)
        {
            Contract.Requires<ArgumentNullException>(ex != null, nameof(ex));
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));

            if (data.Count == 0)
                return ex;

            data.Select(kv => ex.Data[kv.Key] = kv.Value).Count();

            return ex;
        }

        /// <summary>
        /// Populates the <see cref="Fault.Data"/> dictionary with data from a corresponding (e.g. from <see cref="Exception"/>) dictionary.
        /// </summary>
        /// <param name="f">The fault that needs to be populated.</param>
        /// <param name="data">The data.</param>
        /// <returns>Fault.</returns>
        public static Fault PopulateData(
            this Fault f,
            IDictionary data)
        {
            Contract.Requires<ArgumentNullException>(f != null, nameof(f));
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));

            if (data.Count == 0)
                return f;

            foreach (DictionaryEntry kv in data)
                f.Data[kv.Key?.ToString()] = kv.Value?.ToString();

            return f;
        }
    }
}
