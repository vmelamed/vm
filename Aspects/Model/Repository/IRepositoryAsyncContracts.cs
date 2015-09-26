using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace vm.Aspects.Model.Repository
{
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

        public Task<IRepository> CommitChangesAsync()
        {
            Contract.Ensures(Contract.Result<Task<IRepository>>() != null);

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

        public T CreateEntity<T>() where T : BaseDomainEntity, new()
        {
            throw new NotImplementedException();
        }

        public T CreateValue<T>() where T : BaseDomainValue, new()
        {
            throw new NotImplementedException();
        }

        public IRepository Add<T>(
            T entity) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IRepository Attach<T>(
            T entity) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IRepository Attach<T>(
            T entity,
            EntityState state,
            params string[] modifiedProperties) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IRepository Detach<T>(
            T entity) where T : BaseDomainEntity
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

        public IRepository Delete<T>(
            T entity) where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Entities<T>() where T : BaseDomainEntity
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> DetachedEntities<T>() where T : BaseDomainEntity
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
    }
}
