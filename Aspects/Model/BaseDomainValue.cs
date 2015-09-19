using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Exceptions;
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
    public abstract partial class BaseDomainValue : IValidatable, IVisited<BaseDomainValue>
    {
        #region IValidatable Members
        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <param name="ruleset">
        /// The ruleset to test validity against.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the specified ruleset is valid; otherwise, <see langword="false"/>.
        /// </returns>
        /// <value><see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.</value>
        public virtual bool IsValid(
            string ruleset = "")
        {
            return DoValidate(ruleset).IsValid;
        }

        /// <summary>
        /// Performs the validation logic and if the object is not valid throws <see cref="InvalidObjectException"/>.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <returns></returns>
        /// <exception cref="InvalidObjectException">
        /// Thrown if the object is not valid. Should contain all reasons why the state is not consistent.
        ///   </exception>
        public virtual IValidatable ConfirmValid(
            string ruleset = "")
        {
            Contract.Ensures(Contract.Result<IValidatable>() != null);

            var results = DoValidate(ruleset);

            if (!results.IsValid)
                throw new InvalidObjectException(results);

            return this;
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <param name="results">An existing results collection to which the current validation results should be appended to.</param>
        /// <returns>A list of <see cref="ValidationResult" /> objects.</returns>
        public virtual ValidationResults DoValidate(
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
                Debug.WriteLine(
                        "{0}\n{1}",
                        ToString(),
                        results.DumpString());
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
                throw new ArgumentNullException("visitor");

            throw new NotImplementedException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Either values of type {0} do not accept visitors of type {1} or the visitors do not have a concrete overload for {0}.",
                            GetType().Name,
                            visitor.GetType().Name));
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
