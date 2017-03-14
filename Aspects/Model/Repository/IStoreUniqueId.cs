using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Model.Repository
{
    /// <summary>
    /// Defines a contract that represents the obtaining of a new unused value for data store unique identifier for objects of a specified type.
    /// E.g. usually represents obtaining a DB primary key for the specified type.
    /// </summary>
    /// <typeparam name="TId">The type of the store unique identifier.</typeparam>
    [ContractClass(typeof(IStoreUniqueIdContract<>))]
    public interface IStoreUniqueId<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// Gets a new store ID which must be guaranteed to be unique.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the store sequence for which this method should return a new unused value.</typeparam>
        /// <param name="repository">The repository representing the store.</param>
        /// <returns>A new unused value for unique store identifier in the store sequence.</returns>
        TId GetNewId<T>(IRepository repository) where T : IHasStoreId<TId>;

        /// <summary>
        /// Gets a new store ID which must be guaranteed to be unique.
        /// </summary>
        /// <param name="objectsType">Type of the objects.</param>
        /// <param name="repository">The repository representing the store.</param>
        /// <returns>A new unused value for unique store identifier in the store sequence.</returns>
        TId GetNewId(Type objectsType, IRepository repository);
    }

    #region IStoreUniqueId<TId> contract binding
    [ContractClassFor(typeof(IStoreUniqueId<>))]
    abstract class IStoreUniqueIdContract<TId> : IStoreUniqueId<TId> where TId : IEquatable<TId>
    {
        public TId GetNewId<T>(IRepository repository) where T : IHasStoreId<TId>
        {
            Contract.Requires<ArgumentNullException>(repository != null, nameof(repository));

            throw new NotImplementedException();
        }

        public TId GetNewId(Type objectsType, IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(repository  != null, nameof(repository));
            Contract.Requires<ArgumentNullException>(objectsType != null, nameof(objectsType));

            throw new NotImplementedException();
        }
    }
    #endregion

}
