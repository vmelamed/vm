using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects.Model.Repository
{
    /// <summary>
    /// The interface of the object model's abstract store repository (DB, XML, in-memory, etc.)
    /// </summary>
    [ContractClass(typeof(IRepositoryContracts))]
    public interface IRepository : IDisposable, IIsDisposed
    {
        /// <summary>
        /// Initializes the repository.
        /// </summary>
        IRepository Initialize();

        /// <summary>
        /// Gets a value indicating whether the instance implementing the interface is initialized.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is initialized; otherwise, <see langword="false"/>.
        /// </value>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets a unique store id for the specified type of objects.
        /// </summary>
        /// <typeparam name="T">The type of object for which to get a unique ID.</typeparam>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <returns>Unique ID value.</returns>
        TId GetStoreId<T, TId>()
            where T : IHasStoreId<TId>
            where TId : IEquatable<TId>;

        /// <summary>
        /// Creates an <see cref="BaseDomainEntity"/>-derived object of type <typeparamref name="T"/>.
        /// Also extends the object with repository/ORM specific qualities like proxy, properties change tracking, etc.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be created.</typeparam>
        /// <returns>The created entity.</returns>
        T CreateEntity<T>() where T : BaseDomainEntity, new();

        /// <summary>
        /// Creates a <see cref="T:Value"/> derived object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to be created.
        /// </typeparam>
        /// <returns>
        /// The created entity.
        /// </returns>
        T CreateValue<T>() where T : BaseDomainValue, new();

        /// <summary>
        /// Adds a new instance of <typeparamref name="T"/> to the repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be added.</typeparam>
        /// <param name="entity">The instance to be added.</param>
        /// <returns><c>this</c></returns>
        IRepository Add<T>(T entity) where T : BaseDomainEntity;

        /// <summary>
        /// Attaches the specified instance to the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="entity">The instance to attach.</param>
        /// <returns><c>this</c></returns>
        IRepository Attach<T>(T entity) where T : BaseDomainEntity;

        /// <summary>
        /// Attaches the specified instance to the context of the repository and marks the entire instance or the specified properties as modified.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="entity">The instance to attach and mark as modified.</param>
        /// <param name="state">The repository related state of the object.</param>
        /// <param name="modifiedProperties">
        /// The names of the properties that actually changed their values.
        /// If the array is empty, the entire entity will be marked as modified and updated in the store 
        /// otherwise, only the modified properties will be updated in the store.
        /// </param>
        /// <returns><c>this</c></returns>
        IRepository Attach<T>(T entity, EntityState state, params string[] modifiedProperties) where T : BaseDomainEntity;

        /// <summary>
        /// Detaches the specified instance from the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="entity">The instance to attach.</param>
        /// <returns><c>this</c></returns>
        IRepository Detach<T>(T entity) where T : BaseDomainEntity;

        /// <summary>
        /// Gets an instance of type <typeparamref name="T"/> from the repository where the instance is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be fetched.</typeparam>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <param name="id">The repository's related unique identifier (primary key for a DB) of the instance to be fetched.</param>
        /// <returns>An instance with the specified <paramref name="id"/> or <see langword="null" />.</returns>
        T GetByStoreId<T, TId>(TId id)
            where T : BaseDomainEntity, IHasStoreId<TId>
            where TId : IEquatable<TId>;

        /// <summary>
        /// Gets an instance of type <typeparamref name="T"/> from the repository where the instance is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be fetched.</typeparam>
        /// <typeparam name="TKey">The type of the business key.</typeparam>
        /// <param name="key">The business key of the instance to be fetched.</param>
        /// <returns>An instance with the specified <paramref name="key"/> or <see langword="null" />.</returns>
        T GetByKey<T, TKey>(TKey key)
            where T : BaseDomainEntity, IHasBusinessKey<TKey>
            where TKey : IEquatable<TKey>;

        /// <summary>
        /// Deletes an instance from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be deleted.</typeparam>
        /// <param name="entity">The instance to be deleted.</param>
        /// <returns><c>this</c></returns>
        /// <remarks>
        /// Consider if <paramref name="entity"/> is <see langword="null"/> or not found in the repository, the method to silently succeed.
        /// </remarks>
        IRepository Delete<T>(T entity) where T : BaseDomainEntity;

        /// <summary>
        /// Deletes an instance from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be deleted.</typeparam>
        /// <param name="value">The instance to be deleted.</param>
        /// <returns><c>this</c></returns>
        /// <remarks>
        /// Consider if <paramref name="value"/> is <see langword="null"/> or not found in the repository, the method to silently succeed.
        /// </remarks>
        IRepository DeleteValue<T>(T value) where T : BaseDomainValue;

        /// <summary>
        /// Gets a collection of all entities of type <typeparamref name="T"/> from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entities to be returned.</typeparam>
        /// <returns><see cref="IQueryable{T}"/> sequence.</returns>
        /// <remarks>
        /// This returns the DDD's aggregate source for the given type. If you modify any of the returned entities 
        /// their state will most likely be saved in the underlying storage.
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </remarks>
        IQueryable<T> Entities<T>() where T : BaseDomainEntity;

        /// <summary>
        /// Gets a collection of all values of type <typeparamref name="T"/> from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the values to be returned.</typeparam>
        /// <returns><see cref="IQueryable{T}"/> sequence.</returns>
        /// <remarks>
        /// This returns the DDD's value objects represented by the given type. If you modify any of the returned values 
        /// their state will most likely be saved in the underlying storage.
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </remarks>
        IQueryable<T> Values<T>() where T : BaseDomainValue;

        /// <summary>
        /// Gets a collection of all entities of type <typeparamref name="T"/> from the repository and detaches them from the context/session.
        /// </summary>
        /// <typeparam name="T">The type of the instances to be returned.</typeparam>
        /// <returns><see cref="IQueryable{T}"/> sequence.</returns>
        /// <remarks>
        /// This method returns the DDD's aggregate source for the given type. The objects however are supposed to be &quot;detached&quot; 
        /// from the repository's context and any modifications on them will not be saved unless they are re-attached. 
        /// This method gives a chance for performance optimization for some ORM-s in fetching read-only objects from the repository. 
        /// The implementing class may however delegate this implementation to <see cref="Entities&lt;T&gt;"/>.
        /// <para>
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </para>
        /// </remarks>
        IQueryable<T> DetachedEntities<T>() where T : BaseDomainEntity;

        /// <summary>
        /// Gets a collection of all values of type <typeparamref name="T"/> from the repository and detaches them from the context/session.
        /// </summary>
        /// <typeparam name="T">The type of the values to be returned.</typeparam>
        /// <returns><see cref="IQueryable{T}"/> sequence.</returns>
        /// <remarks>
        /// This method returns the DDD's values represented by the given type. The objects however are supposed to be &quot;detached&quot; 
        /// from the repository's context and any modifications on them will not be saved unless they are re-attached. 
        /// This method gives a chance for performance optimization for some ORM-s in fetching read-only objects from the repository. 
        /// The implementing class may however delegate this implementation to <see cref="Entities{T}"/>.
        /// <para>
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </para>
        /// </remarks>
        IQueryable<T> DetachedValues<T>() where T : BaseDomainValue;

        /// <summary>
        /// Saves the changes buffered in the repository's context.
        /// </summary>
        /// <returns><c>this</c></returns>
        IRepository CommitChanges();
    }

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

        public IRepository DeleteValue<T>(T value) where T : BaseDomainValue
        {
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));
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

        #region IIsDisposed Members
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
