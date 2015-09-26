using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.ServiceModel;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Exceptions;
using vm.Aspects.Facilities;
using vm.Aspects.Validation;

namespace vm.Aspects.Wcf.DataContracts
{
    /// <summary>
    /// Messages (contracts) base class. Standardizes on <see cref="T:vm.Aspects.Validation.IValidatable"/>.
    /// This is just another type of DTO. Prefer <see cref="T:DataTransferObject"/> where possible.
    /// </summary>
    [MessageContract(WrapperNamespace="urn:vm.Aspects.Wcf.DataContracts")]
    public abstract class Message : IValidatable
    {
        #region IValidatable Members
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <param name="results">The results.</param>
        /// <returns>A list of <see cref="ValidationResult" /> objects.</returns>
        public ValidationResults DoValidate(
            string ruleset = "",
            ValidationResults results = null)
        {
            var validator = Facility.ValidatorFactory.CreateValidator(GetType(), ruleset);

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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.DumpString();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <returns><see langword="true" /> if the specified ruleset is valid; otherwise, <see langword="false" />.</returns>
        public bool IsValid(
            string ruleset = "")
        {
            return DoValidate(ruleset).IsValid;
        }

        /// <summary>
        /// Performs the validation logic and if the object is not valid throws <see cref="T:ValidationException" />.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <returns>IValidatable.</returns>
        /// <exception cref="T:vm.Aspects.Exceptions.ValidationException">Thrown when []</exception>
        public IValidatable ConfirmValid(
            string ruleset = "")
        {
            var results = DoValidate(ruleset);

            if (!results.IsValid)
                throw new InvalidObjectException(results);

            return this;
        }

        /// <summary>
        /// Accepts the visitor which implements the <see cref="T:IDataTransferObjectVisitor"/> interface. See the G4 visitor pattern.
        /// Here it serves as a catch all - throws <see cref="System.NotImplementedException"/>.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public virtual void AcceptVisitor(
            object visitor)
        {
            Contract.Requires<ArgumentNullException>(visitor != null, nameof(visitor));

            throw new NotImplementedException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The message of type {0} do not accept visitors of type {1}.",
                            GetType().Name,
                            visitor.GetType().Name));
        }
    }
}
