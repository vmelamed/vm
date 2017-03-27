using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Class DomainEntity.
    /// </summary>
    /// <typeparam name="TId">The type of the store identifier.</typeparam>
    /// <typeparam name="TKey">The type of the business key.</typeparam>
    [DebuggerDisplay("{GetType().Name, nq}[{Id,nq}]: {Key,nq}")]
    [MetadataType(typeof(DomainEntityMetadata))]
    public abstract partial class DomainEntity<TId, TKey> : BaseDomainEntity,
        IHasStoreId<TId>,
        IHasBusinessKey<TKey>,
        IEquatable<DomainEntity<TId, TKey>>
        where TId : IEquatable<TId>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets a value indicating whether this instance has identity.
        /// </summary>
        /// <remarks>
        /// The implementation assumes that the entity has identity when the <see cref="Key"/> is not equal to the default value of its type.
        /// E.g. if the type of the key is <see cref="string"/> the entity has identity if <c>Key!=null</c>.
        /// </remarks>
        public override bool HasIdentity => !ReferenceEquals(Key, null)  &&  !Key.Equals(default(TKey));

        #region IHasStoreId<TId> Members
        /// <summary>
        /// Provides the inheritors with an access to the backing field of the property <see cref="Id"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "The inheritors may want to override the default behavior without duplicating the field.")]
        [CLSCompliant(false)]
        protected TId _id;

        /// <summary>
        /// Gets or sets the store identifier.
        /// The property is applied the attributes <see cref="KeyAttribute"/> and <see cref="ColumnAttribute"/> with parameter <c>Order = 0</c>.
        /// </summary>
        [CacheKey(IsPrimary = true)]
        public virtual TId Id
        {
            get { return _id; }
            set
            {
                if (!_id.Equals(default(TId)))
                    throw new InvalidOperationException("Once the value of the property is set it cannot be changed.");

                _id = value;
            }
        }
        #endregion

        #region IHasBusinessKey<TKey> Members
        /// <summary>
        /// Gets the entity business key.
        /// </summary>
        public abstract TKey Key { get; }
        #endregion

        #region Identity rules implementation.
        #region IEquatable<DomainEntity<TId, TKey>> Members
        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="DomainEntity{TId, TKey}"/> to compare with this object.</param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="other"/> is equal to <see langword="null"/>, otherwise
        /// <see langword="true"/> if <paramref name="other"/> refers to <c>this</c> object, otherwise
        /// <see langword="false"/> if <paramref name="other"/> is not the same type as the current object, otherwise
        /// <see langword="true"/> if <i>the business identities</i> of the current object and the <paramref name="other"/> are equal by value.
        /// </returns>
        /// <remarks>
        /// The <see cref="M:Equals(DomainEntity{TId, TKey})"/> methods and the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
        /// </remarks>
        public virtual bool Equals(
            DomainEntity<TId, TKey> other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (OrmBridge.GetEntityType(this) != OrmBridge.GetEntityType(other))
                return false;

            return Key.Equals(other.Key);
        }
        #endregion

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
        /// The <see cref="M:Equals(BaseDomainEntity)"/> methods and the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
        /// </remarks>
        public override bool Equals(BaseDomainEntity other) => Equals(other as DomainEntity<TId, TKey>);

        /// <summary>
        /// Determines whether this <see cref="DomainEntity{TId, TKey}"/> instance is equal to the specified <see cref="System.Object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> reference to compare with this <see cref="DomainEntity{TId, TKey}"/> object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="obj"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true"/> if <paramref name="obj"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="true"/> if <paramref name="obj"/> <i>is an instance of</i> <see cref="DomainEntity{TId, TKey}"/> and 
        ///           the business identities of the current object and the <paramref name="obj"/> are equal by value; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(DomainEntity{TId, TKey})"/> methods and the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
        /// </remarks>
        public override bool Equals(
            object obj) => Equals(obj as DomainEntity<TId, TKey>);

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="DomainEntity{TId, TKey}"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DomainEntity{TId, TKey}"/> instance.</returns>
        public override int GetHashCode() => HasIdentity ? Key.GetHashCode() : 0;

        /// <summary>
        /// Compares two <see cref="DomainEntity{TId, TKey}"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are considered to be equal (<see cref="Equals(DomainEntity{TId, TKey})"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(
            DomainEntity<TId, TKey> left,
            DomainEntity<TId, TKey> right) => ReferenceEquals(left, null)
                                                ? ReferenceEquals(right, null)
                                                : left.Equals(right);

        /// <summary>
        /// Compares two <see cref="DomainEntity{TId, TKey}"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(DomainEntity{TId, TKey})"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(
            DomainEntity<TId, TKey> left,
            DomainEntity<TId, TKey> right) => !(left == right);
        #endregion
    }
}
