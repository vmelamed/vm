using System;

namespace vm.Aspects.Model
{
    /// <summary>
    /// <para>
    /// Interface IHasStoreId adds a property <see cref="P:Id"/> to the implementing types, e.g. domain entities or domain values.
    /// The property represents the unique identifier in some specific persistent object store e.g. database.
    /// In RDBMS it may map directly to the primary key of the database table(s) or at least can participate in it.
    /// </para><para>
    /// This interface is a convenience and is definitely a leak of persistence concerns into the domain model. 
    /// However it makes our live easier especially when working with O/RM-s.
    /// Technically the <see cref="P:Id"/> can also serve as the entity unique key but this is rarely a good idea because:
    /// <list type="number">
    /// <item>
    /// It has no business meaning - it is not from the ubiquitous language. 
    /// It is always better to use as a key an identifier used in the business domain, e.g. &quot;OrderNumber&quot;.
    /// </item><item>
    /// When stored in other stores, it may not necessarily preserve its value.
    /// </item><item>
    /// In cases when the value of the Id is generated from the store as an identity value, it may be perceived as a security risk.
    /// When the Id is transmitted to and from the client as an entity reference, a simple increment of the value may give an attacker access to other entities.
    /// </item>
    /// </list>
    /// </para><para>
    /// The property <see cref="P:Id"/> exposes both a getter and a setter. However the implementing types should make sure that 
    /// once the property is assigned a value it becomes immutable.
    /// </para>
    /// </summary>
    /// <typeparam name="TId">The type of the store identifier.</typeparam>
    public interface IHasStoreId<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// Gets or sets the store identifier. The implementing type may want to constraint the property setter.
        /// </summary>
        TId Id { get; set; }
    }
}
