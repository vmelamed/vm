using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects.Model.Repository
{
    [ContractClassFor(typeof(IRepository))]
    abstract class IRepositoryContracts : IRepository
    {
        #region IRepository Members
        public IRepository Initialize()
        {
            Contract.Ensures(IsInitialized, "The repository was not initialized successfully.");

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
            Contract.Ensures(!Contract.Result<TId>().Equals(default(TId)), "The store ID provider returned default(T)");

            throw new NotImplementedException();
        }

        public T CreateEntity<T>() where T : BaseDomainEntity, new()
        {
            Contract.Ensures(Contract.Result<T>() != null, "Could not create an entity object.");

            throw new NotImplementedException();
        }

        public T CreateValue<T>() where T : BaseDomainValue, new()
        {
            Contract.Ensures(Contract.Result<T>() != null, "Could not create a value object.");

            throw new NotImplementedException();
        }

        public IRepository Add<T>(
            T entity) where T : BaseDomainEntity
        {
            Contract.Requires<ArgumentNullException>(entity != null, nameof(entity));
            Contract.Requires<InvalidOperationException>(entity.HasIdentity, "The entity must have identity before it is added to the repository.");
            Contract.Ensures(Contract.Result<IRepository>() != null);

            throw new NotImplementedException();
        }

        public IRepository Attach<T>(
            T entity) where T : BaseDomainEntity
        {
            Contract.Requires<ArgumentNullException>(entity != null, nameof(entity));
            Contract.Requires<InvalidOperationException>(entity.HasIdentity, "The entity must have identity before it is attached to the repository.");
            Contract.Ensures(Contract.Result<IRepository>() != null);

            throw new NotImplementedException();
        }

        public IRepository Attach<T>(
            T entity,
            EntityState state,
            params string[] modifiedProperties) where T : BaseDomainEntity
        {
            Contract.Requires<ArgumentNullException>(entity != null, nameof(entity));
            Contract.Requires<InvalidOperationException>(entity.HasIdentity, "The entity must have identity before it is attached to the repository.");
            Contract.Requires<ArgumentNullException>(modifiedProperties != null, nameof(modifiedProperties));
            Contract.Ensures(Contract.Result<IRepository>() != null);

            throw new NotImplementedException();
        }

        public IRepository Detach<T>(
            T entity) where T : BaseDomainEntity
        {
            Contract.Requires<ArgumentNullException>(entity != null, nameof(entity));

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
            Contract.Requires<ArgumentNullException>(entity != null, nameof(entity));
            Contract.Ensures(Contract.Result<IRepository>() != null);

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
    }
}
