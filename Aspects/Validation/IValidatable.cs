using System;
using System.Diagnostics.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Contract implemented by entities and values which can validate the consistency of their own state.
    /// While it is not necessary it is assumed that the implementation of the interface would be based on 
    /// Enterprise Library's Validation Application Block.
    /// </summary>
    [ContractClass(typeof(IValidatableContract))]
    public interface IValidatable
    {
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <param name="results">An existing results collection to which the current validation results should be appended to.</param>
        /// <returns>A list of <see cref="ValidationResult" /> objects.</returns>
        [Pure]
        ValidationResults Validate(string ruleset = "", ValidationResults results = null);
    }

    [ContractClassFor(typeof(IValidatable))]
    abstract class IValidatableContract : IValidatable
    {
        #region IValidatable Members
        [Pure]
        public ValidationResults Validate(
            string ruleset = "",
            ValidationResults results = null)
        {
            Contract.Requires<ArgumentNullException>(ruleset != null, nameof(ruleset));
            Contract.Ensures(Contract.Result<ValidationResults>() != null);

            throw new NotImplementedException();
        }

        #endregion
    }
}
