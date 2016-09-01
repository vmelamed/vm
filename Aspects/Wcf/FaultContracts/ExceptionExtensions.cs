using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects.Wcf.FaultContracts
{
    static class ExceptionExtensions
    {
        public static Exception PopulateData(
            this Exception ex,
            IDictionary<string, string> data)
        {
            Contract.Requires<ArgumentNullException>(ex != null, nameof(ex));
            Contract.Requires<ArgumentNullException>(data != null, nameof(data));

            if (data.Count == 0)
                data.Select(kv => ex.Data[kv.Key] = kv.Value).Count();

            return ex;
        }
    }
}
