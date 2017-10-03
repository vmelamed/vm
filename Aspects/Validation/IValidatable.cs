using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Contract implemented by entities and values which can validate the consistency of their own state.
    /// While it is not necessary it is assumed that the implementation of the interface would be based on 
    /// Enterprise Library's Validation Application Block.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <param name="results">An existing results collection to which the current validation results should be appended to.</param>
        /// <returns>A list of <see cref="ValidationResult" /> objects.</returns>
        ValidationResults Validate(string ruleset = "", ValidationResults results = null);
    }
}
