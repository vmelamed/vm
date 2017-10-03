using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Interface <c>IExceptionPolicyProvider</c> provides exception policy entries to various exception policies. By implementing this interface several types
    /// can contribute exception policy entries to the same policy. The facility will combine the entries from all registered implementations of 
    /// this interface in the <see cref="P:DIContainer.Root"/>.
    /// </summary>
    public interface IExceptionPolicyProvider
    {
        /// <summary>
        /// Gets a dictionary of exception policy names and respective lists of policy entries.
        /// </summary>
        /// <value>The entries for the respective exception policies.</value>
        IDictionary<string, IEnumerable<ExceptionPolicyEntry>> ExceptionPolicyEntries { get; }
    }
}
