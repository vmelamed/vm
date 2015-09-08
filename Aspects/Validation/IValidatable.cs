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
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified ruleset is valid; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsValid(string ruleset = "");

        /// <summary>
        /// Performs the validation logic and if the object is not valid throws <see cref="T:vm.Aspects.Exceptions.ValidationException"/>.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <exception cref="T:vm.Aspects.Exceptions.ValidationException">
        /// Thrown if the object is not valid. Should contain all reasons why the state is not consistent.
        /// </exception>
        IValidatable ConfirmValid(string ruleset = "");

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <param name="results">An existing results collection to which the current validation results should be appended to.</param>
        /// <returns>A list of <see cref="ValidationResult" /> objects.</returns>
        ValidationResults DoValidate(string ruleset = "", ValidationResults results = null);
    }

    [ContractClassFor(typeof(IValidatable))]
    abstract class IValidatableContract : IValidatable
    {
        #region IValidatable Members

        public bool IsValid(
            string ruleset = "")
        {
            Contract.Requires<ArgumentNullException>(ruleset != null, nameof(ruleset));

            throw new NotImplementedException();
        }

        public IValidatable ConfirmValid(
            string ruleset = "")
        {
            Contract.Requires<ArgumentNullException>(ruleset != null, nameof(ruleset));
            Contract.Ensures(Contract.Result<IValidatable>() != null);

            throw new NotImplementedException();
        }

        public ValidationResults DoValidate(
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
