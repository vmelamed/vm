using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Facilities;
using vm.Aspects.Validation;
using vm.Aspects.Visitor;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Base class for value classes from the the domain model.
    /// </summary>
    /// <remarks>
    /// The value objects do not have identity and make sense only in the context of their relationship with their owning domain entities.
    /// However the value objects should be validated and also can participate in the visitor pattern.
    /// </remarks>
    [DebuggerDisplay("{GetType().Name, nq}")]
    public abstract partial class BaseDomainValue : IValidatable, IVisited<BaseDomainValue>, IVisitedTasks<BaseDomainValue>
    {
        #region IValidatable Members
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <param name="results">An existing results collection to which the current validation results should be appended to.</param>
        /// <returns>A list of <see cref="ValidationResult" /> objects.</returns>
        public virtual ValidationResults Validate(
            string ruleset = "",
            ValidationResults results = null)
        {
            Contract.Ensures(Contract.Result<ValidationResults>() != null);

            var validator = Facility.ValidatorFactory
                                    .CreateValidator(GetType(), ruleset);

            if (results == null)
                results = validator.Validate(this);
            else
                validator.Validate(this, results);

#if DEBUG
            if (!results.IsValid)
                Debug.WriteLine($"{ToString()}\n{results.DumpString()}");
#endif

            return results;
        }
        #endregion

        #region IVisited<BaseDomainValue> Members
        /// <summary>
        /// Throws <see cref="T:NotImplementedException"/> exception.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="visitor"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.NotImplementedException">Always thrown.</exception>
        public BaseDomainValue Accept(
            IVisitor<BaseDomainValue> visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));

            throw new NotImplementedException($"Either values of type {GetType().Name} do not accept visitors of type {1} or the visitors do not have a concrete overload for {visitor.GetType().Name}.");
        }
        #endregion

        #region IVisitedTasks<BaseDomainValue> Members
        /// <summary>
        /// Throws <see cref="T:NotImplementedException"/> exception.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="visitor"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.NotImplementedException">Always thrown.</exception>
        public Task<BaseDomainValue> AcceptAsync(
            IVisitorTasks<BaseDomainValue> visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));

            throw new NotImplementedException($"Either values of type {GetType().Name} do not accept visitors of type {1} or the visitors do not have a concrete overload for {visitor.GetType().Name}.");
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="indentLevel">The indent level.</param>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public string ToString(
            int indentLevel)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return this.DumpString(indentLevel);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return this.ToString(0);
        }
    }
}
