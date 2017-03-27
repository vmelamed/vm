using System;

namespace vm.Aspects.Model
{
    /// <summary>
    /// <para>
    /// Interface IHasBusinessKey requires implementing a property <see cref="Key"/> in the implementing types 
    /// which represent domain entities and usually derive from <see cref="BaseDomainEntity"/>.
    /// Unlike <see cref="P:IHasStoreId.Id"/> the property <see cref="Key"/> can be used as a primary key in databases. 
    /// However if the key can be long this may violate the database best practices.
    /// </para><para>
    /// When the interface is implemented by a type inheriting from <see cref="BaseDomainEntity"/>, make sure that 
    /// the property <see cref="P:BaseDomainEntity.HasKey"/> is computed based on the value of the property 
    /// <see cref="Key"/> and also the .NET rules of equality are also implemented based on the value of 
    /// <see cref="Key"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TKey">The type of the business key.</typeparam>
    public interface IHasBusinessKey<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets the business key.
        /// </summary>
        TKey Key { get; }
    }
}
