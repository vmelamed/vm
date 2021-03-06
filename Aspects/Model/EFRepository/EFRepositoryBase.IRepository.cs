﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository
{
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Security;
    using System.Threading;

    using CommonServiceLocator;

    using Exceptions;

    using Facilities.Diagnostics;

    using Threading;

    using Unity.Exceptions;

    using EFEntityState = System.Data.Entity.EntityState;
    using EntityState = Repository.EntityState;

    public partial class EFRepositoryBase : IRepository
    {
        readonly static ReaderWriterLockSlim _latchesSync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly static IDictionary<Type, Latch> _latches = new Dictionary<Type, Latch>();

        readonly static ReaderWriterLockSlim _eventsSync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly static IDictionary<Type, ManualResetEvent> _events = new Dictionary<Type, ManualResetEvent>();

        Latch GetLatch()
        {
            Latch latch = null;
            Type myType = GetType();

            using (_latchesSync.UpgradableReaderLock())
            {
                if (_latches.TryGetValue(myType, out latch))
                    return latch;

                using (_latchesSync.WriterLock())
                {
                    if (_latches.TryGetValue(myType, out latch))
                        return latch;
                    return _latches[myType] = new Latch();
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is the intent")]
        ManualResetEvent GetEvent()
        {
            ManualResetEvent @event = null;
            Type myType = GetType();

            using (_eventsSync.UpgradableReaderLock())
            {
                if (_events.TryGetValue(myType, out @event))
                    return @event;

                using (_eventsSync.WriterLock())
                {
                    if (_events.TryGetValue(myType, out @event))
                        return @event;
                    return _events[myType] = new ManualResetEvent(false);
                }
            }
        }

        #region IRepository
        /// <summary>
        /// Initializes the repository.
        /// </summary>
        /// <returns>this</returns>
        public virtual IRepository Initialize(Action query = null)
        {
            if (!GetLatch().Latched())
                return this;

            DoInitialize(query);
            GetEvent().Set();
            return this;
        }

        /// <summary>
        /// Performs the actual initialization (always called from a synchronized context, i.e. from within a lock).
        /// Always call the base class's implementation first.
        /// </summary>
        protected virtual void DoInitialize(Action query)
        {
            try
            {
                VmAspectsEventSource.Log.EFRepositoryInitializationStart(this);
                SetDatabaseInitializer();
                Database.Initialize(false);

                query?.Invoke();
                VmAspectsEventSource.Log.EFRepositoryInitializationStop(this);
            }
            catch (Exception x)
            {
                VmAspectsEventSource.Log.EFRepositoryInitializationFailed(this, x);
                throw;
            }
        }

        /// <summary>
        /// Initializes the database with an initializer resolved from the service locator, if not found the method
        /// will initialize it with <see cref="T:DropCreateDatabaseIfModelChanges{T}"/>. The derived repositories can override
        /// the method if they seek a different behavior as in the example below.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public override void SetDatabaseInitializer()
        /// {
        ///     Database.SetInitializer(new MigrateDatabaseToLatestVersion<MyRepository, Configuration>());
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void SetDatabaseInitializer()
        {
            object initializer = null;

            try
            {
                // get the initializer from the DI container
                initializer = ServiceLocator
                                        .Current
                                        .GetService(
                                            typeof(IDatabaseInitializer<>).MakeGenericType(GetType()));
            }
            catch (ActivationException) { }
            catch (ResolutionFailedException) { }

            if (initializer == null)
                try
                {
                    initializer = typeof(DropCreateDatabaseIfModelChanges<>)
                                        .MakeGenericType(GetType())
                                        .GetConstructor(Type.EmptyTypes)
                                        .Invoke(new object[0]);
                }
                catch (ArgumentException) { }
                catch (InvalidOperationException) { }
                catch (NotSupportedException) { }
                catch (MemberAccessException) { }
                catch (SecurityException) { }
                catch (TargetInvocationException) { }

            // if we've got an initializer set it in the DB.
            if (initializer != null)
                // Database.SetInitializer<MyRepository>(initializer);
                typeof(Database)
                        .GetMethod(nameof(Database.SetInitializer))
                        .MakeGenericMethod(GetType())
                        .Invoke(null, new object[] { initializer });
        }

        /// <summary>
        /// Gets a value indicating whether the instance implementing the interface is initialized.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is initialized; otherwise, <see langword="false"/>.
        /// </value>
        public virtual bool IsInitialized => GetLatch().IsLatched;

        /// <summary>
        /// Waits until the repository is initialized.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IRepository.</returns>
        public IRepository WaitIsInitialized(Action query = null)
        {
            Initialize();
            GetEvent().WaitOne();
            return this;
        }

        /// <summary>
        /// Gets or sets the optimistic concurrency strategy - caller wins vs. store wins (the default).
        /// See also <seealso cref="CommitChanges"/> and <seealso cref="CommitChangesAsync"/>.
        /// </summary>
        public OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        /// <summary>
        /// Gets a unique store id for the specified type of objects.
        /// </summary>
        /// <typeparam name="T">The type of object for which to get a unique ID.</typeparam>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <returns>Unique ID value.</returns>
        public TId GetStoreId<T, TId>()
            where T : IHasStoreId<TId>
            where TId : IEquatable<TId> => StoreIdProvider
                                                .GetProvider<TId>()
                                                .GetNewId<T>(this);

        /// <summary>
        /// Gets a unique store id for the specified type of objects.
        /// </summary>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <param name="objectsType">Type of the objects.</param>
        /// <returns>Unique ID value.</returns>
        public TId GetStoreId<TId>(
            Type objectsType)
            where TId : IEquatable<TId>
        {
            if (objectsType == null)
                throw new ArgumentNullException(nameof(objectsType));

            return StoreIdProvider
                        .GetProvider<TId>()
                        .GetNewId(objectsType, this);
        }

        /// <summary>
        /// Creates an entity of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to be created.</typeparam>
        /// <returns>The created object.</returns>
        public T CreateEntity<T>() where T : BaseDomainEntity, new()
            => Set<T>().Create();

        /// <summary>
        /// Creates a <see cref="BaseDomainEntity" />-derived object of type <paramref name="entityType" />
        /// Also extends the object with repository/ORM specific qualities like proxy, properties change tracking, etc.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>The created entity.</returns>
        public BaseDomainEntity CreateEntity(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return Set(entityType).Create() as BaseDomainEntity;
        }

        /// <summary>
        /// Creates a value object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to be created.</typeparam>
        /// <returns>The created object.</returns>
        public T CreateValue<T>() where T : BaseDomainValue, new()
            => Set<T>().Create();

        /// <summary>
        /// Creates a <see cref="BaseDomainValue" /> derived object of type <paramref name="valueType" />.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>The created value.</returns>
        public BaseDomainValue CreateValue(Type valueType)
        {
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));

            return Set(valueType).Create() as BaseDomainValue;
        }

        /// <summary>
        /// Adds a new entity of <typeparamref name="T" /> to the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be added.</typeparam>
        /// <param name="entity">The entity to be added.</param>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        public IRepository Add<T>(T entity) where T : BaseDomainEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (!entity.HasIdentity)
                throw new InvalidOperationException("The entity must have identity before it is added to the repository.");

            Set<T>().Add(entity);
            return this;
        }

        /// <summary>
        /// Attaches the specified entity to the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to attach.</param>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public IRepository AttachEntity<T>(T entity) where T : BaseDomainEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (!entity.HasIdentity)
                throw new InvalidOperationException("The entity must have identity before it is added to the repository.");

            Set<T>().Attach(entity);
            return this;
        }

        /// <summary>
        /// Attaches the specified entity to the context of the repository and marks the entire entity or the specified properties as modified.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to attach and mark as modified.</param>
        /// <param name="state">The repository related state of the object.</param>
        /// <param name="modifiedProperties">
        /// The names of the properties that actually changed their values.
        /// If the array is empty, the entire entity will be marked as modified and updated in the store 
        /// otherwise, only the modified properties will be updated in the store.
        /// </param>
        /// <returns><c>this</c></returns>
        public IRepository AttachEntity<T>(
            T entity,
            EntityState state,
            params string[] modifiedProperties) where T : BaseDomainEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (!entity.HasIdentity)
                throw new InvalidOperationException("The entity must have identity before it is added to the repository.");
            if (modifiedProperties == null)
                throw new ArgumentNullException(nameof(modifiedProperties));

            Set<T>().Add(entity);

            var entry = ChangeTracker.Entries<T>()
                                     .FirstOrDefault(e => ReferenceEquals(e.Entity, entity));

            if (state == EntityState.Modified && modifiedProperties.Any())
            {
                entry.State = EFEntityState.Unchanged;
                foreach (var property in modifiedProperties)
                    entry.Property(property).IsModified = true;
            }
            else
                entry.State = EFSpecifics.ConvertState(state);

            return this;
        }

        /// <summary>
        /// Attaches the specified value to the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to attach.</param>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public IRepository AttachValue<T>(T value) where T : BaseDomainValue
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Set<T>().Attach(value);
            return this;
        }

        /// <summary>
        /// Attaches the specified value to the context of the repository and marks the entire value or the specified properties as modified.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to attach and mark as modified.</param>
        /// <param name="state">The repository related state of the object.</param>
        /// <param name="modifiedProperties">
        /// The names of the properties that actually changed their values.
        /// If the array is empty, the entire value will be marked as modified and updated in the store 
        /// otherwise, only the modified properties will be updated in the store.
        /// </param>
        /// <returns><c>this</c></returns>
        public IRepository AttachValue<T>(
            T value,
            EntityState state,
            params string[] modifiedProperties) where T : BaseDomainValue
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (modifiedProperties == null)
                throw new ArgumentNullException(nameof(modifiedProperties));

            Set<T>().Add(value);

            var entry = ChangeTracker.Entries<T>()
                                     .FirstOrDefault(e => ReferenceEquals(e.Entity, value));

            if (state == EntityState.Modified && modifiedProperties.Length > 0)
            {
                entry.State = EFEntityState.Unchanged;
                foreach (var property in modifiedProperties)
                    entry.Property(property).IsModified = true;
            }
            else
                entry.State = EFSpecifics.ConvertState(state);

            return this;
        }

        /// <summary>
        /// Detaches the specified entity to the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to attach.</param>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public IRepository DetachEntity<T>(T entity) where T : BaseDomainEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ObjectContext.Detach(entity);
            return this;
        }

        /// <summary>
        /// Detaches the specified value to the context of the repository.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to attach.</param>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public IRepository DetachValue<T>(T value) where T : BaseDomainValue
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            ObjectContext.Detach(value);
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
            where TId : IEquatable<TId> => Set<T>().Find(id);

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
            where TKey : IEquatable<TKey> => Set<T>().FirstOrDefault(e => e.Key.Equals(key));

        /// <summary>
        /// Deletes an instance from a repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be deleted.</typeparam>
        /// <param name="entity">The instance to be deleted.</param>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        /// <exception cref="ArgumentNullException">instance</exception>
        /// <remarks>
        /// Consider if <paramref name="entity" /> is <see langword="null"/> or not found in the repository, the method to silently succeed.
        /// </remarks>
        public IRepository DeleteEntity<T>(
            T entity) where T : BaseDomainEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                Set<T>().Remove(entity);
            }
            catch (InvalidOperationException)
            {
                // ignore the exception when the object is not in the repository - silently succeed  
            }

            return this;
        }

        /// <summary>
        /// Deletes an instance from a repository.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be deleted.</typeparam>
        /// <param name="value">The instance to be deleted.</param>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        /// <exception cref="ArgumentNullException">instance</exception>
        /// <remarks>
        /// Consider if <paramref name="value" /> is <see langword="null"/> or not found in the repository, the method to silently succeed.
        /// </remarks>
        public IRepository DeleteValue<T>(
            T value) where T : BaseDomainValue
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                Set<T>().Remove(value);
            }
            catch (InvalidOperationException)
            {
                // ignore the exception when the object is not in the repository - silently succeed  
            }

            return this;
        }

        /// <summary>
        /// Gets a collection of all entities of type <typeparamref name="T" /> from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entities to be returned.</typeparam>
        /// <returns>
        ///   <see cref="IEnumerable{E}" /> sequence.
        /// </returns>
        /// <remarks>
        /// This returns the DDD's aggregate source for the given type. If you modify any of the returned entities
        /// their state will most likely be saved in the underlying storage.
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </remarks>
        public IQueryable<T> Entities<T>() where T : BaseDomainEntity => Set<T>();

        /// <summary>
        /// Gets a collection of all values of type <typeparamref name="T" /> from the repository.
        /// </summary>
        /// <typeparam name="T">The type of the values to be returned.</typeparam>
        /// <returns>
        ///   <see cref="IEnumerable{E}" /> sequence.
        /// </returns>
        /// <remarks>
        /// This returns the DDD's aggregate source for the given type. If you modify any of the returned values
        /// their state will most likely be saved in the underlying storage.
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </remarks>
        public IQueryable<T> Values<T>() where T : BaseDomainValue => Set<T>();

        /// <summary>
        /// Gets a collection of all entities of type <typeparamref name="T" /> from the repository and detaches them from the context/session.
        /// </summary>
        /// <typeparam name="T">The type of the entities to be returned.</typeparam>
        /// <returns>
        ///   <see cref="IEnumerable{T}" /> sequence.
        /// </returns>
        /// <remarks>
        /// This method returns the DDD's aggregate source for the given type. The objects however are supposed to be "detached" from the repository's
        /// context and any modifications on them will not be saved unless they are re-attached. This method gives a chance for performance optimization for some ORM-s in
        /// fetching read-only objects from the repository.
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </remarks>
        public IQueryable<T> DetachedEntities<T>() where T : BaseDomainEntity => Set<T>().AsNoTracking();

        /// <summary>
        /// Gets a collection of all values of type <typeparamref name="T" /> from the repository and detaches them from the context/session.
        /// </summary>
        /// <typeparam name="T">The type of the entities to be returned.</typeparam>
        /// <returns>
        ///   <see cref="IEnumerable{T}" /> sequence.
        /// </returns>
        /// <remarks>
        /// This method returns the DDD's aggregate source for the given type. The objects however are supposed to be "detached" from the repository's
        /// context and any modifications on them will not be saved unless they are re-attached. This method gives a chance for performance optimization for some ORM-s in
        /// fetching read-only objects from the repository.
        /// Hint: The result of this method can participate in the <c>from</c> clause of a LINQ expression.
        /// </remarks>
        public IQueryable<T> DetachedValues<T>() where T : BaseDomainValue => Set<T>().AsNoTracking();

        /// <summary>
        /// Initializes the repository asynchronously.
        /// </summary>
        /// <returns>this</returns>
        public virtual async Task<IRepository> InitializeAsync(Func<Task> query = null)
            => await Task.Run(
                            async () =>
                            {
                                if (!GetLatch().Latched())
                                    return this;

                                await DoInitializeAsync(query);
                                GetEvent().Set();
                                return this;
                            });

        /// <summary>
        /// Performs the actual initialization (always called from a synchronized context, i.e. from within a lock).
        /// Always call the base class's implementation first.
        /// </summary>
        protected virtual async Task DoInitializeAsync(Func<Task> query)
        {
            try
            {
                VmAspectsEventSource.Log.EFRepositoryInitializationStart(this);
                SetDatabaseInitializer();
                Database.Initialize(false);

                if (query != null)
                    await query.Invoke();
                VmAspectsEventSource.Log.EFRepositoryInitializationStop(this);
            }
            catch (Exception x)
            {
                VmAspectsEventSource.Log.EFRepositoryInitializationFailed(this, x);
                throw;
            }
        }

        /// <summary>
        /// Waits until the repository is initialized.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IRepository.</returns>
        public async Task<IRepository> WaitIsInitializedAsync(Func<Task> query = null)
        {
            await InitializeAsync(query);
            GetEvent().WaitOne();
            return this;
        }

        /// <summary>
        /// Gets asynchronously an entity of type <typeparamref name="T" /> from the repository where the entity is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be fetched.</typeparam>
        /// <typeparam name="TId">The type of the store identifier.</typeparam>
        /// <param name="id">The repository's related unique identifier (primary key for a DB) of the entity to be fetched.</param>
        /// <returns>A <see cref="Task{T}" /> object which represents the asynchronous process of fetching the object.</returns>
        public Task<T> GetByStoreIdAsync<T, TId>(TId id)
            where T : BaseDomainEntity, IHasStoreId<TId>
            where TId : IEquatable<TId> => Set<T>().FindAsync(id);

        /// <summary>
        /// Gets an entity of type <typeparamref name="T" /> from the repository where the entity is referred to by repository ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be fetched.</typeparam>
        /// <typeparam name="TKey">The type of the business key.</typeparam>
        /// <param name="key">The business key of the entity to be fetched.</param>
        /// <returns>A <see cref="Task{T}" /> object which represents the asynchronous process of fetching the object.</returns>
        public Task<T> GetByKeyAsync<T, TKey>(TKey key)
            where T : BaseDomainEntity, IHasBusinessKey<TKey>
            where TKey : IEquatable<TKey> => Set<T>().FirstOrDefaultAsync(e => e.Key.Equals(key));

        /// <summary>
        /// Saves the changes buffered in the repository's context.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public IRepository CommitChanges()
        {
            new Retry<bool>(
                i =>
                {
                    try
                    {
                        VmAspectsEventSource.Log.EFRepositoryCommitChanges(this);
                        SaveChanges();
                        return true;
                    }
                    catch (DbUpdateConcurrencyException cx)
                    {
                        var entry = cx.Entries.SingleOrDefault();

                        if (entry == null)
                            throw;

                        if (OptimisticConcurrencyStrategy != OptimisticConcurrencyStrategy.ClientWins)
                        {
                            if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.StoreWins)
                                // load the current values into the entity to communicate them to the client
                                entry.CurrentValues.SetValues(entry.GetDatabaseValues());

                            throw;
                        }

                        if (i >= MaxNumberOfRetries-1)
                            // we should not be retrying anymore
                            throw new RepeatableOperationException("IRepository.CommitChangesAsync failed too many times.", cx);

                        // load the current DB values as original - as if we knew there was an update and we are overwriting the updated values with what the client told us,
                        entry.OriginalValues.SetValues(entry.GetDatabaseValues());

                        // try again
                        return false;
                    }
                },
                (r, x, i) => x != null,
                (r, x, i) => x == null  &&  r)
                .Start(
                    MaxNumberOfRetries,
                    MinWaitBeforeRetry,
                    MaxWaitBeforeRetry);    // by default retry immediately but not more than 5 times.

            return this;
        }

        /// <summary>
        /// Saves the changes buffered in the repository's context asynchronously.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public async Task<IRepository> CommitChangesAsync()
        {
            await new RetryTasks<bool>(
                async i =>
                {
                    try
                    {
                        VmAspectsEventSource.Log.EFRepositoryCommitChanges(this);
                        await SaveChangesAsync();
                        return true;
                    }
                    catch (DbUpdateConcurrencyException cx)
                    {
                        var entry = cx.Entries.SingleOrDefault();

                        if (entry == null)
                            throw;

                        if (OptimisticConcurrencyStrategy != OptimisticConcurrencyStrategy.ClientWins)
                        {
                            if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.StoreWins)
                                // load the current values into the entity to communicate them to the client
                                entry.CurrentValues.SetValues(await entry.GetDatabaseValuesAsync());

                            throw;
                        }

                        if (i >= MaxNumberOfRetries-1)
                            // we should not be retrying anymore
                            throw new RepeatableOperationException("IRepository.CommitChangesAsync failed too many times.", cx);

                        // load the current DB values as original - as if we knew there was a update and we are overwriting the updated values with what the client told us,
                        entry.OriginalValues.SetValues(await entry.GetDatabaseValuesAsync());

                        // try again
                        return false;
                    }
                    catch (AggregateException ax)
                    {

                        if (!(UnwrapAggregateExceptionHandler.Unwrap(ax) is DbUpdateConcurrencyException cx))
                            throw;

                        // catch (DbUpdateConcurrencyException cx)
                        var entry = cx.Entries.SingleOrDefault();

                        if (entry == null)
                            throw;

                        if (OptimisticConcurrencyStrategy != OptimisticConcurrencyStrategy.ClientWins)
                        {
                            if (OptimisticConcurrencyStrategy == OptimisticConcurrencyStrategy.StoreWins)
                                // load the current values into the entity to communicate them to the client
                                entry.CurrentValues.SetValues(await entry.GetDatabaseValuesAsync());

                            throw;
                        }

                        if (i >= MaxNumberOfRetries-1)
                            // we should not be retrying anymore
                            throw new RepeatableOperationException("IRepository.CommitChangesAsync failed too many times.", ax);

                        // load the current DB values as original - as if we knew there was a update and we are overwriting the updated values with what the client told us,
                        entry.OriginalValues.SetValues(await entry.GetDatabaseValuesAsync());

                        // try again
                        return false;
                    };
                },
                (r, x, i) => Task.FromResult(x != null),
                (r, x, i) => Task.FromResult(x == null  &&  r))
                .StartAsync(
                    MaxNumberOfRetries,
                    MinWaitBeforeRetry,
                    MaxWaitBeforeRetry);    // by default retry immediately but not more than 5 times.

            return this;
        }
        #endregion

        /// <summary>
        /// Gets or sets the maximum number of <see cref="CommitChanges"/> retries on optimistic concurrency exception.
        /// </summary>
        public int MaxNumberOfRetries { get; set; } = 5;
        /// <summary>
        /// Gets or sets the minimum wait before <see cref="CommitChanges"/> retry after optimistic concurrency exception.
        /// </summary>
        public int MinWaitBeforeRetry { get; set; } = 0;
        /// <summary>
        /// Gets or sets the maximum wait before <see cref="CommitChanges"/> retry after optimistic concurrency exception.
        /// </summary>
        public int MaxWaitBeforeRetry { get; set; } = 0;
    }
}
