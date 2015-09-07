using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Interface <c>IExceptionPolicyProvider</c> provides exception policy entries to various exception policies. By implementing this interface several types
    /// can contribute exception policy entries to the same policy. The facility will combine the entries from all registered implementations of 
    /// this interface in the <see cref="P:DIContainer.Root"/>.
    /// </summary>
    [ContractClass(typeof(IExceptionPolicyEntriesContract))]
    public interface IExceptionPolicyProvider
    {
        /// <summary>
        /// Gets a dictionary of exception policy names and respective lists of policy entries.
        /// </summary>
        /// <value>The entries for the respective exception policies.</value>
        IDictionary<string, IEnumerable<ExceptionPolicyEntry>> ExceptionPolicyEntries { get; }
    }

    [ContractClassFor(typeof(IExceptionPolicyProvider))]
    abstract class IExceptionPolicyEntriesContract : IExceptionPolicyProvider
    {
        #region IExceptionPolicyProvider Members

        public IDictionary<string, IEnumerable<ExceptionPolicyEntry>> ExceptionPolicyEntries
        {
            get
            {
                Contract.Ensures(Contract.Result<IDictionary<string, IEnumerable<ExceptionPolicyEntry>>>() != null);

                throw new System.NotImplementedException();
            }
        }

        #endregion
    }
}
