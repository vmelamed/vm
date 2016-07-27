using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Properties;
using vm.Aspects.Validation;
using vm.Aspects.Visitor;

namespace vm.Aspects.Model
{
    /// <summary>
    /// <para>
    /// Class BaseDomainEntity represents the very basic tenets of a domain entity as they are defined in DDD.
    /// </para><para>
    /// The domain entity has an immutable identity. This is reflected by the fact that <see cref="BaseDomainEntity"/> inherits  <see cref="T:IEquatable{BaseDomainEntity}"/>
    /// and the property <see cref="P:HasIdentity"/>. The latter should reflect the fact that when domain entity instances are created 
    /// they may not have identity until some business logic assigns them one (e.g. assigns value/s to the entity's property/s which define its identity).
    /// Once the identity of the entity is assigned it should never change or at least may change very rarely preferably in controlled conditions. 
    /// The identity should remain immutable for the entire lifespan of the entity including the time when it is not loaded in memory and exists 
    /// only as a persisted image (e.g. serialized in a binary file, XML document, database row(s) and columns). If the property <see cref="P:HasIdentity"/> returns
    /// <see langword="false"/> then the method <see cref="M:Equals"/> should always return <see langword="false"/> regardless of the value of 
    /// the comparand. Following the rules of .NET's for equality, the return value of the method <see cref="M:GetHashCode"/> should also be immutable.
    /// </para><para>
    /// Another tenet of the domain entity is that once ready to be used in business logic or ready to be persisted, the entity must have a <bold>valid</bold>
    /// state, which is verified by the methods from the inherited interface <see cref="IValidatable"/>.
    /// </para><para>
    /// A feature of the domain entities, which I find to be very useful, is to be able to participate in the visitor pattern (see G4 patterns) as the
    /// visited element. It can be used in many situations like: assigning values to the entity's properties from various factories: database primary keys, 
    /// properties constituting the entity's identity, constructing entire new entities from DTO-s or vice-versa generating DTO-s out of entities.
    /// I tend to think of this pattern as an elegant way to extend the interface of the entity without modifying it or with a minimum modification.
    /// </para>
    /// </summary>
    [DebuggerDisplay("{GetType().Name, nq}")]
    [HasSelfValidation]
    public abstract class BaseDomainEntity : IEquatable<BaseDomainEntity>, IValidatable, IVisited<BaseDomainEntity>
    {
        /// <summary>
        /// Gets a value indicating whether this instance has identity.
        /// </summary>
        /// <remarks>
        /// When the domain entity instances are created at first they may not have identity yet and the property must return 
        /// <see langword="false"/>. After assigning identity value/s to the entity (e.g. assigning value/s to the entity's identity property/s),
        /// the property should return <see langword="true"/>.
        /// </remarks>
        public abstract bool HasIdentity { get; }

        #region IEquatable<BaseDomainEntity>
        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="BaseDomainEntity"/> to compare with this object.</param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="other"/> is equal to <see langword="null"/>, otherwise
        /// <see langword="true"/> if <paramref name="other"/> refers to <c>this</c> object, otherwise
        /// <see langword="true"/> if <i>the business identities</i> of the current object and the <paramref name="other"/> are equal by value,
        /// e.g. <c>BusinessKeyProperty == other.BusinessKeyProperty</c>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals"/> methods and the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
        /// </remarks>
        public abstract bool Equals(BaseDomainEntity other);
        #endregion

        #region IValidatable Members
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <param name="results">An existing results collection to which the current validation results should be appended to.</param>
        /// <returns>A list of <see cref="ValidationResult" /> objects.</returns>
        [Pure]
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

        /// <summary>
        /// Validates the current instance. Here checks if the instance has identity in if it does not, 
        /// adds a new <see cref="T:ValidationResult"/> to the <see cref="T:ValidationResults"/> parameter.
        /// </summary>
        /// <param name="results">The full set of validation results.</param>
        [SelfValidation]
        protected virtual void Validate(
            ValidationResults results = null)
        {
            if (!HasIdentity)
                results.AddResult(
                    new ValidationResult(
                        Resources.ExNoIdentity,
                        this,
                        "HasIdentity",
                        string.Empty,
                        null));
        }
        #endregion

        #region IVisited<BaseDomainEntity> Members
        /// <summary>
        /// Throws <see cref="T:NotImplementedException"/> exception.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="visitor"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.NotImplementedException">Always thrown.</exception>
        public virtual BaseDomainEntity Accept(
            IVisitor<BaseDomainEntity> visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));

            throw new NotImplementedException(
                        $"Entities of type {GetType().Name} do not accept visitors of type {visitor.GetType().Name}.");
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
