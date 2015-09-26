using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace vm.Aspects.Model.Repository
{
    /// <summary>
    /// Interface IRepositoryAsync extends <see cref="IRepository"/> with asynchronous versions of some of the methods.
    /// </summary>
    [ContractClass(typeof(IRepositoryAsyncContracts))]
    public interface IRepositoryAsync : IRepository
    {
        /// <summary>
        /// Initializes the repository asynchronously.
        /// </summary>
        /// <returns>this</returns>
        Task<IRepository> InitializeAsync();

        /// <summary>
        /// Gets asynchronously an entity of type <typeparamref name="T"/> from the repository where the entity is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be fetched.</typeparam>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <param name="id">The repository's related unique identifier (primary key for a DB) of the entity to be fetched.</param>
        /// <returns>A <see cref="Task{T}"/> object which represents the asynchronous process of fetching the object.</returns>
        Task<T> GetByStoreIdAsync<T, TId>(
            TId id)
            where T : BaseDomainEntity, IHasStoreId<TId>
            where TId : IEquatable<TId>;

        /// <summary>
        /// Gets an entity of type <typeparamref name="T"/> from the repository where the entity is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be fetched.</typeparam>
        /// <typeparam name="TKey">The type of the business key.</typeparam>
        /// <param name="key">The business key of the entity to be fetched.</param>
        /// <returns>A <see cref="Task{T}"/> object which represents the asynchronous process of fetching the object.</returns>
        Task<T> GetByKeyAsync<T, TKey>(
            TKey key)
            where T : BaseDomainEntity, IHasBusinessKey<TKey>
            where TKey : IEquatable<TKey>;

        /// <summary>
        /// Asynchronously saves the changes buffered in the repository's context.
        /// </summary>
        /// <returns><c>this</c></returns>
        Task<IRepository> CommitChangesAsync();
    }
}
