using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vm.Aspects.Exceptions;
using vm.Aspects.Model.Repository;
using vm.Aspects.Threading;

namespace vm.Aspects.Model.InMemory
{
    /// <summary>
    /// Class Repository is in-memory objects repository. Can be used in some test scenarios.
    /// Not advisable for use in production environment even as a cache.
    /// </summary>
    public sealed partial class ListObjectsRepository : IRepository
    {
        /// <summary>
        /// Synchronizes multi-threaded access to the underlying objects container
        /// </summary>
        static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        /// <summary>
        /// The latch controls the initialization of the repository.
        /// </summary>
        static Latch _latch = new Latch();
        /// <summary>
        /// The objects container.
        /// </summary>
        static List<DomainEntity<long, string>> _objects;
        /// <summary>
        /// The long store ID generator.
        /// </summary>
        static long _longId;

        /// <summary>
        /// The repository unique instance identifier
        /// </summary>
        readonly Guid _instanceId = Guid.NewGuid();

        /// <summary>
        /// Gets the instance identifier.
        /// </summary>
        public Guid InstanceId => _instanceId;

        static void UnsafeReset()
        {
            _objects       = new List<DomainEntity<long, string>>();
            _longId        = 0L;
        }

        /// <summary>
        /// Resets the underlying global structures
        /// </summary>
        public static void Reset()
        {
            using (_sync.WriterLock())
                UnsafeReset();
        }

        #region IRepository Members
        /// <summary>
        /// Gets a value indicating whether the instance implementing the interface is initialized.
        /// </summary>
        public bool IsInitialized => _latch.IsLatched;

        /// <summary>
        /// Gets or sets the optimistic concurrency strategy - caller wins vs. store wins (the default).
        /// Here it really doesn't matter as all in memory operations are synchronized and concurrency conflicts will not happen.
        /// </summary>
        public OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        /// <summary>
        /// Initializes the repository.
        /// </summary>
        /// <returns>IRepository.</returns>
        public IRepository Initialize()
        {
            if (_latch.Latched())
                Reset();

            return this;
        }

        /// <summary>
        /// Gets a unique store id for the specified type of objects.
        /// </summary>
        /// <typeparam name="T">The type of object for which to get a unique ID.</typeparam>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <returns>Unique ID value.</returns>
        /// <exception cref="System.NotImplementedException">The repository does not support entities with ID of type +typeof(TId)</exception>
        public TId GetStoreId<T, TId>()
            where T : IHasStoreId<TId>
            where TId : IEquatable<TId>
        {
            if (typeof(TId) != typeof(long))
                throw new NotImplementedException("The repository does not support entities with ID of type "+typeof(TId));

            object id = Interlocked.Increment(ref _longId);

            return (TId)id;
        }

        /// <summary>
        /// Gets a unique store id for the specified type of objects.
        /// </summary>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <param name="objectsType">Type of the objects.</param>
        /// <returns>Unique ID value.</returns>
        /// <exception cref="System.NotImplementedException">The repository does not support entities with ID of type +typeof(TId)</exception>
        public TId GetStoreId<TId>(
            Type objectsType)
            where TId : IEquatable<TId>
        {
            if (typeof(TId) != typeof(long))
                throw new NotImplementedException("The repository does not support entities with ID of type "+typeof(TId));

            object id = Interlocked.Increment(ref _longId);

            return (TId)id;
        }

        /// <summary>
        /// Creates an <see cref="BaseDomainEntity" />-derived object of type <typeparamref name="T" />.
        /// Also extends the object with repository/ORM specific qualities like proxy, properties change tracking, etc.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be created.</typeparam>
        /// <returns>The created entity.</returns>
        /// <exception cref="System.InvalidOperationException">The repository does not support type +typeof(T).FullName</exception>
        public T CreateEntity<T>()
            where T : BaseDomainEntity, new()
            => ObjectsRepositorySpecifics.CreateEntity<T>();

        /// <summary>
        /// Creates a <see cref="BaseDomainEntity" />-derived object of type <paramref name="entityType" />
        /// Also extends the object with repository/ORM specific qualities like proxy, properties change tracking, etc.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>The created entity.</returns>
        public BaseDomainEntity CreateEntity(
            Type entityType)
            => (BaseDomainEntity)ObjectsRepositorySpecifics.CreateEntity(entityType);

        /// <summary>
        /// Creates a <see cref="BaseDomainValue" /> derived object of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the object to be created.</typeparam>
        /// <returns>The created entity.</returns>
        public T CreateValue<T>() where T : BaseDomainValue, new()
            => ObjectsRepositorySpecifics.CreateValue<T>();

        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>BaseDomainEntity.</returns>
        public BaseDomainValue CreateValue(Type valueType)
            => (BaseDomainValue)ObjectsRepositorySpecifics.CreateEntity(valueType);

        /// <summary>
        /// Adds a new entity of <typeparamref name="T" /> to the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be added.</typeparam>
        /// <param name="entity">The entity to be added.</param>
        /// <returns><c>this</c></returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public IRepository Add<T>(
            T entity) where T : BaseDomainEntity
        {
            var entityWith = entity as DomainEntity<long, string>;

            if (entityWith == null)
                throw new InvalidOperationException("The repository does not support type "+typeof(T).FullName);

            using (_sync.UpgradableReaderLock())
            {
                // ensure the uniqueness of the entity
                if (_objects.OfType<T>()
                            .Any(e => e.Equals(entityWith)))
                    throw new ObjectIdentifierNotUniqueException("An object with this identity already exists in the store.");

                if (entityWith.Id == 0L)
                    entityWith.Id = Interlocked.Increment(ref _longId);

                if (_objects.OfType<T>().Any(o => (o as DomainEntity<long, string>).Id == entityWith.Id))
                    throw new ObjectIdentifierNotUniqueException("An object with this store unique identifier already exists in the store.");

                using (_sync.WriterLock())
                    _objects.Add(entityWith);
            }

            return this;
        }

        /// <summary>
        /// Gets an entity of type <typeparamref name="T" /> from the repository where the entity is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be fetched.</typeparam>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <param name="id">The repository's related unique identifier (primary key for a DB) of the entity to be fetched.</param>
        /// <returns>An entity with the specified <paramref name="id" /> or <see langword="null" />.</returns>
        public T GetByStoreId<T, TId>(
            TId id)
            where T : BaseDomainEntity, IHasStoreId<TId>
            where TId : IEquatable<TId>
        {
            using (_sync.ReaderLock())
                return _objects.OfType<T>()
                               .FirstOrDefault(e => e.Id.Equals(id));
        }

        /// <summary>
        /// Gets an entity of type <typeparamref name="T" /> from the repository where the entity is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be fetched.</typeparam>
        /// <typeparam name="TKey">The type of the business key.</typeparam>
        /// <param name="key">The business key of the entity to be fetched.</param>
        /// <returns>An entity with the specified <paramref name="key" /> or <see langword="null" />.</returns>
        public T GetByKey<T, TKey>(
            TKey key)
            where T : BaseDomainEntity, IHasBusinessKey<TKey>
            where TKey : IEquatable<TKey>
        {
            using (_sync.ReaderLock())
                return _objects.OfType<T>()
                               .FirstOrDefault(e => e.Key.Equals(key));
        }

        /// <summary>
        /// Attaches the specified entity to the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to attach.</param>
        /// <returns><c>this</c></returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public IRepository AttachEntity<T>(
            T entity) where T : BaseDomainEntity
        {
            var entityWith = entity as DomainEntity<long, string>;

            if (entityWith == null)
                throw new InvalidOperationException("The repository does not support type "+typeof(T).FullName);

            using (_sync.UpgradableReaderLock())
            {
                var existing = _objects.OfType<T>()
                                       .FirstOrDefault(o => o.Equals(entityWith));

                if (existing == null)
                    throw new InvalidOperationException(
                                "Only entities that exist in the underlying store can be attached in EntityState.Modified state. "+
                                "Note that repositories based on Entity Framework or NHibernate would not be able to catch this situation early "+
                                "and will possibly throw an exception at commit time.");

                if (ReferenceEquals(existing, entity))
                    return this;

                using (_sync.WriterLock())
                {
                    _objects.Remove(existing as DomainEntity<long, string>);
                    _objects.Add(entityWith);
                }
                return this;
            }
        }

        /// <summary>
        /// Attaches the specified entity to the context of the repository and marks the entire entity or the specified properties as modified.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to attach and mark as modified.</param>
        /// <param name="state">The repository related state of the object.</param>
        /// <param name="modifiedProperties">The names of the properties that actually changed their values.
        /// If the array is empty, the entire entity will be marked as modified and updated in the store
        /// otherwise, only the modified properties will be updated in the store.</param>
        /// <returns><c>this</c></returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#"), SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "It is cast only once.")]
        public IRepository AttachEntity<T>(
            T entity,
            EntityState state,
            params string[] modifiedProperties) where T : BaseDomainEntity
        {
            var entityWith = entity as DomainEntity<long, string>;

            if (entityWith == null)
                throw new InvalidOperationException("The repository does not support type "+typeof(T).FullName);

            using (_sync.UpgradableReaderLock())
            {
                var existing = _objects.OfType<T>()
                                       .FirstOrDefault(o => o.Equals(entity));

                switch (state)
                {
                case EntityState.Added:
                    if (existing != null)
                        throw new InvalidOperationException(
                                "Only entities that do not exist in the underlying store can be attached in EntityState.Added state. "+
                                "Note that repositories based on Entity Framework or NHibernate would not be able to catch this situation early "+
                                "and will possibly throw an exception at commit time.");

                    using (_sync.WriterLock())
                        _objects.Add(entityWith);

                    break;

                case EntityState.Modified:
                    if (existing == null)
                        throw new InvalidOperationException(
                                "Only entities that exist in the underlying store can be attached in EntityState.Modified state. "+
                                "Note that repositories based on Entity Framework or NHibernate would not be able to catch this situation early "+
                                "and will possibly throw an exception at commit time.");

                    if (!ReferenceEquals(existing, entity))
                        using (_sync.WriterLock())
                        {
                            _objects.Remove(existing as DomainEntity<long, string>);
                            _objects.Add(entityWith);
                        }
                    break;

                case EntityState.Deleted:
                    if (existing != null)
                        using (_sync.WriterLock())
                            _objects.Remove(existing as DomainEntity<long, string>);
                    break;
                }
            }

            return this;
        }

        /// <summary>
        /// Attaches the specified value to the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to attach.</param>
        /// <returns><c>this</c></returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public IRepository AttachValue<T>(
            T value) where T : BaseDomainValue => this;

        /// <summary>
        /// Attaches the specified value to the context of the repository and marks the entire entity or the specified properties as modified.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to attach and mark as modified.</param>
        /// <param name="state">The repository related state of the object.</param>
        /// <param name="modifiedProperties">The names of the properties that actually changed their values.
        /// If the array is empty, the entire entity will be marked as modified and updated in the store
        /// otherwise, only the modified properties will be updated in the store.</param>
        /// <returns><c>this</c></returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#"), SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "It is cast only once.")]
        public IRepository AttachValue<T>(
            T value,
            EntityState state,
            params string[] modifiedProperties) where T : BaseDomainValue => this;

        /// <summary>
        /// Detaches the specified entity from the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to attach.</param>
        /// <returns><c>this</c></returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public IRepository DetachEntity<T>(
            T entity) where T : BaseDomainEntity
        {
            throw new NotImplementedException("The ListObjectsRepository does not implement the method Detach.");
        }

        /// <summary>
        /// Detaches the specified value from the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to attach.</param>
        /// <returns><c>this</c></returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public IRepository DetachValue<T>(
            T value) where T : BaseDomainValue
        {
            throw new NotImplementedException("The ListObjectsRepository does not implement the method Detach.");
        }

        /// <summary>
        /// Deletes an instance from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be deleted.</typeparam>
        /// <param name="instance">The instance to be deleted.</param>
        /// <returns><c>this</c></returns>
        /// <remarks>Consider if <paramref name="instance" /> is <see langword="null" /> or not found in the repository, the method to silently succeed.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public IRepository DeleteEntity<T>(
            T instance) where T : BaseDomainEntity
        {
            var entityWith = instance as DomainEntity<long, string>;

            if (entityWith == null)
                throw new InvalidOperationException("The repository does not support type "+typeof(T).FullName);

            using (_sync.WriterLock())
                _objects.Remove(entityWith);

            return this;
        }

        /// <summary>
        /// Deletes an instance from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be deleted.</typeparam>
        /// <param name="instance">The instance to be deleted.</param>
        /// <returns><c>this</c></returns>
        /// <remarks>Consider if <paramref name="instance" /> is <see langword="null" /> or not found in the repository, the method to silently succeed.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public IRepository DeleteValue<T>(
            T instance) where T : BaseDomainValue => this;

        /// <summary>
        /// Gets a collection of all entities of type <typeparamref name="T" /> from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entities to be returned.</typeparam>
        /// <returns><see cref="IQueryable{T}" /> sequence.</returns>
        /// <remarks>This returns the DDD's aggregate source for the given type. If you modify any of the returned entities
        /// their state will most likely be saved in the underlying storage.
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.</remarks>
        public IQueryable<T> Entities<T>() where T : BaseDomainEntity
        {
            using (_sync.ReaderLock())
                return _objects.OfType<T>()
                               .ToArray()
                               .AsQueryable<T>();
        }

        /// <summary>
        /// Gets a collection of all values of type <typeparamref name="T" /> from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the values to be returned.</typeparam>
        /// <returns><see cref="IQueryable{T}" /> sequence.</returns>
        /// <remarks>This returns the DDD's values represented by the given type. If you modify any of the returned values
        /// their state will most likely be saved in the underlying storage.
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.</remarks>
        public IQueryable<T> Values<T>() where T : BaseDomainValue
        {
            using (_sync.ReaderLock())
                return _objects.OfType<T>()
                               .ToArray()
                               .AsQueryable<T>();
        }

        /// <summary>
        /// Gets from the repository a collection of detached from the current context/session entities of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the entities to be returned.</typeparam>
        /// <returns><see cref="IQueryable{T}" /> sequence.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>This method returns the DDD's aggregate source for the given type. The objects however are supposed to be "detached"
        /// from the repository's context and any modifications on them will not be saved unless they are re-attached.
        /// This method gives a chance for performance optimization for some ORM-s in fetching read-only objects from the repository.
        /// The implementing class may however delegate this implementation to <see cref="Entities&lt;T&gt;" />.
        /// <para>
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </para></remarks>
        public IQueryable<T> DetachedEntities<T>() where T : BaseDomainEntity => Entities<T>();

        /// <summary>
        /// Gets from the repository a collection of detached from the current context/session values of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the values to be returned.</typeparam>
        /// <returns><see cref="IQueryable{T}" /> sequence.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>This method returns the DDD's aggregate source for the given type. The objects however are supposed to be "detached"
        /// from the repository's context and any modifications on them will not be saved unless they are re-attached.
        /// This method gives a chance for performance optimization for some ORM-s in fetching read-only objects from the repository.
        /// The implementing class may however delegate this implementation to <see cref="Entities&lt;T&gt;" />.
        /// <para>
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </para></remarks>
        public IQueryable<T> DetachedValues<T>() where T : BaseDomainValue => Values<T>();

        /// <summary>
        /// Saves the changes buffered in the repository's context.
        /// </summary>
        /// <returns><c>this</c></returns>
        public IRepository CommitChanges() => this;

        /// <summary>
        /// Initializes the repository asynchronously.
        /// </summary>
        /// <returns>this</returns>
        public Task<IRepository> InitializeAsync() => Task.FromResult(Initialize());

        /// <summary>
        /// Gets asynchronously an entity of type <typeparamref name="T" /> from the repository where the entity is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be fetched.</typeparam>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <param name="id">The repository's related unique identifier (primary key for a DB) of the entity to be fetched.</param>
        /// <returns>A <see cref="Task{T}" /> object which represents the asynchronous process of fetching the object.</returns>
        public Task<T> GetByStoreIdAsync<T, TId>(
            TId id)
            where T : BaseDomainEntity, IHasStoreId<TId>
            where TId : IEquatable<TId> => Task.FromResult(GetByStoreId<T, TId>(id));

        /// <summary>
        /// Gets asynchronously an entity of type <typeparamref name="T" /> from the repository where the entity is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be fetched.</typeparam>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A <see cref="Task{T}" /> object which represents the asynchronous process of fetching the object.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<T> GetByKeyAsync<T, TKey>(
            TKey key)
            where T : BaseDomainEntity, IHasBusinessKey<TKey>
            where TKey : IEquatable<TKey> => Task.FromResult(GetByKey<T, TKey>(key));

        /// <summary>
        /// Asynchronously saves the changes buffered in the repository's context/session.
        /// </summary>
        /// <returns><c>this</c></returns>
        public Task<IRepository> CommitChangesAsync() => Task.FromResult(CommitChanges());
        #endregion

        #region IDisposable Members
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <see langword="true"/> if the object has already been disposed, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
