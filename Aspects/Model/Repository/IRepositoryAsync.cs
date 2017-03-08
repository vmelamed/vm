using System;
using System.Diagnostics.Contracts;
using System.Linq;
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
        Task<IRepositoryAsync> CommitChangesAsync();
    }

    [ContractClassFor(typeof(IRepositoryAsync))]
    abstract class IRepositoryAsyncContracts : IRepositoryAsync
    {
        #region IRepositoryAsync members
        public Task<IRepository> InitializeAsync()
        {
            Contract.Ensures(Contract.Result<Task<IRepository>>() != null);

            throw new NotImplementedException();
        }

        public Task<T> GetByStoreIdAsync<T, TId>(
            TId id)
            where T : BaseDomainEntity, IHasStoreId<TId>
            where TId : IEquatable<TId>
        {
            Contract.Ensures(Contract.Result<Task<T>>() != null);

            throw new NotImplementedException();
        }

        public Task<T> GetByKeyAsync<T, TKey>(
            TKey key)
            where T : BaseDomainEntity, IHasBusinessKey<TKey>
            where TKey : IEquatable<TKey>
        {
            Contract.Ensures(Contract.Result<Task<T>>() != null);

            throw new NotImplementedException();
        }

        public Task<IRepositoryAsync> CommitChangesAsync()
        {
            Contract.Ensures(Contract.Result<Task<IRepositoryAsync>>() != null);

            throw new NotImplementedException();
        }
        #endregion

        #region IRepository Members
        public IRepository Initialize()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized
        {
            get { throw new NotImplementedException(); }
        }

        public TId GetStoreId<T, TId>()
            where T : IHasStoreId<TId>
            where TId : IEquatable<TId>
        {
            throw new NotImplementedException();
        }

        public TId GetStoreId<TId>(
            Type objectsType)
            where TId : IEquatable<TId>
        {
            throw new NotImplementedException();
        }

        public T CreateEntity<T>() where T : BaseDomainEntity, new()
        {
            throw new NotImplementedException();
        }

        public BaseDomainEntity CreateEntity(Type entityType)
        {
            throw new NotImplementedException();
        }

        public T CreateValue<T>() where T : BaseDomainValue, new()
        {
            throw new NotImplementedException();
        }

        public BaseDomainValue CreateValue(Type valueType)
        {
            throw new NotImplementedException();
        }

        public IRepository Add<T>(
            T entity) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IRepository AttachEntity<T>(
            T entity) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IRepository AttachEntity<T>(
            T entity,
            EntityState state,
            params string[] modifiedProperties) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IRepository AttachValue<T>(
            T value) where T : BaseDomainValue
        {
            throw new NotImplementedException();
        }

        public IRepository AttachValue<T>(
            T value,
            EntityState state,
            params string[] modifiedProperties) where T : BaseDomainValue
        {
            throw new NotImplementedException();
        }

        public IRepository DetachEntity<T>(
            T entity) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IRepository DetachValue<T>(
            T value) where T : BaseDomainValue
        {
            throw new NotImplementedException();
        }

        public T GetByStoreId<T, TId>(
            TId id)
            where T : BaseDomainEntity, IHasStoreId<TId>
            where TId : IEquatable<TId>
        {
            throw new NotImplementedException();
        }

        public T GetByKey<T, TKey>(
            TKey key)
            where T : BaseDomainEntity, IHasBusinessKey<TKey>
            where TKey : IEquatable<TKey>
        {
            throw new NotImplementedException();
        }

        public IRepository DeleteEntity<T>(
            T entity) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IRepository DeleteValue<T>(
            T value) where T : BaseDomainValue
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Entities<T>() where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Values<T>() where T : BaseDomainValue
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> DetachedEntities<T>() where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> DetachedValues<T>() where T : BaseDomainValue
        {
            throw new NotImplementedException();
        }

        public IRepository CommitChanges()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region  IIsDisposed
        public bool IsDisposed
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
