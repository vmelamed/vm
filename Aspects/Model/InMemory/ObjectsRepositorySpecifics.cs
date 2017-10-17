using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.InMemory
{
    /// <summary>
    /// Class ObjectRepositorySpecifics. This class cannot be inherited.
    /// </summary>
    public sealed class ObjectsRepositorySpecifics : IOrmSpecifics
    {
        #region IOrmSpecifics Members

        /// <summary>
        /// Suggests eager fetching of related objects when querying the repository.
        /// Here does nothing.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the queried sequence.</typeparam>
        /// <param name="sequence">The queryable sequence.</param>
        /// <param name="path">Specifies the navigation method/property to the property that should be eagerly loaded.</param>
        /// <returns>The queryable sequence.</returns>
        public IQueryable<T> FetchAlso<T>(
            IQueryable<T> sequence,
            string path) where T : class
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(path));

            return sequence;
        }

        /// <summary>
        /// Suggests eager fetching of related objects when querying the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the queried sequence.</typeparam>
        /// <typeparam name="TProperty">The type of the property to be eagerly loaded.</typeparam>
        /// <param name="sequence">The queryable sequence.</param>
        /// <param name="path">Specifies the navigation method/property to the property(s) that should be eagerly loaded as a lambda expression.</param>
        /// <returns>The queryable sequence.</returns>
        public IQueryable<T> FetchAlso<T, TProperty>(
            IQueryable<T> sequence,
            Expression<Func<T, TProperty>> path) where T : class
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return sequence;
        }

        /// <summary>
        /// Enlists the repository's back store transaction manager in the ambient transaction.
        /// Here does nothing.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>this</returns>
        public IRepository EnlistInAmbientTransaction(
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return repository;
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <param name="reference">The reference which POCO entity type is sought.</param>
        /// <returns>The POCO type of the reference.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Type GetEntityType(
            object reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            return reference.GetType();
        }

        /// <summary>
        /// Gets the name of the entity set associated with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="repository">The repository.</param>
        /// <returns>System.String.</returns>
        public string GetEntitySetName(
            Type type,
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (repository is ListObjectsRepository)
                return null;

            return MapObjectsRepository.GetEntitySetRootType(type).FullName;
        }

        /// <summary>
        /// Determines whether the specified reference is a reference to an ORM generated wrapper/proxy of the actual object instead of the actual object itself.
        /// Here always returns <see langword="false"/>.
        /// </summary>
        /// <param name="reference">The reference to be tested.</param>
        /// <returns><see langword="false" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsProxy(
            object reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            return false;
        }

        /// <summary>
        /// Determines whether the specified reference is a reference to an ORM generated wrapper/proxy of the actual object and that it
        /// is tracking automatically the changes to the properties.
        /// Here it always returns <see langword="true"/>.
        /// </summary>
        /// <param name="reference">The reference to be tested.</param>
        /// <param name="repository">The repository.</param>
        /// <returns><see langword="true" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>This method is not supposed to be called from operational code but rather from verification code which makes sure that
        /// the reference is tracking the changes to its properties. In order for that to happen all properties must be declared <c>virtual</c>.</remarks>
        public bool IsChangeTracking(
            object reference,
            IRepository repository)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return true;
        }

        /// <summary>
        /// Determines whether a reference to an object or collection which is associated to a principal object is already loaded in memory.
        /// Here returns <see langword="true" />.
        /// </summary>
        /// <param name="associated">The reference object that needs testing.</param>
        /// <param name="principal">The owner object of the associated.</param>
        /// <param name="propertyName">The name of the <paramref name="principal" />'s property whose value is <paramref name="associated" />.</param>
        /// <param name="repository">The repository.</param>
        /// <returns><see langword="true" />.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// associated
        /// or
        /// principal
        /// or
        /// repository
        /// </exception>
        /// <exception cref="System.ArgumentException">The argument cannot be null, empty string or consist of whitespace characters only. - propertyName</exception>
        public bool IsLoaded(
            object associated,
            object principal,
            string propertyName,
            IRepository repository)
        {
            if (associated == null)
                throw new ArgumentNullException(nameof(associated));
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (propertyName.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(propertyName));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return true;
        }

        /// <summary>
        /// Loads an object or collection of objects which is associated to a principal object.
        /// </summary>
        /// <param name="associated">The associated object or collection that is tested.</param>
        /// <param name="principal">The principal object.</param>
        /// <param name="propertyName">The name of the <paramref name="principal" />'s property whose value is the <paramref name="associated" />.</param>
        /// <param name="repository">The repository.</param>
        /// <returns><see langword="true" /> if the specified reference is loaded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// associated
        /// or
        /// principal
        /// or
        /// repository
        /// </exception>
        /// <exception cref="System.ArgumentException">The argument cannot be null, empty string or consist of whitespace characters only. - propertyName</exception>
        public bool Load(
            object associated,
            object principal,
            string propertyName,
            IRepository repository)
        {
            if (associated == null)
                throw new ArgumentNullException(nameof(associated));
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (propertyName.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(propertyName));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return true;
        }

        /// <summary>
        /// Asynchronously loads an object or collection of objects which is associated to a principal object.
        /// </summary>
        /// <param name="associated">The associated object or collection that is tested.</param>
        /// <param name="principal">The principal object.</param>
        /// <param name="propertyName">The name of the <paramref name="principal" />'s property whose value is the <paramref name="associated" />.</param>
        /// <param name="repository">The repository.</param>
        /// <returns><see langword="true" /> if the specified reference was loaded from the DB; otherwise, <see langword="false" /> - the reference was already loaded.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// associated
        /// or
        /// principal
        /// or
        /// repository
        /// </exception>
        /// <exception cref="System.ArgumentException">The argument cannot be null, empty string or consist of whitespace characters only. - propertyName</exception>
        public Task<bool> LoadAsync(
            object associated,
            object principal,
            string propertyName,
            IRepository repository)
        {
            if (associated == null)
                throw new ArgumentNullException(nameof(associated));
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (propertyName.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(propertyName));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return Task.FromResult(true);
        }

        /// <summary>
        /// Determines whether the specified exception is a result of detected optimistic concurrency.
        /// Here it always returns <see langword="false"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns><see langword="false" />.</returns>
        public bool IsOptimisticConcurrency(
            Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return false;
        }

        /// <summary>
        /// Determines whether the specified exception is a result of problems connecting to the store.
        /// Here it always returns <see langword="false"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns><see langword="false" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsConnectionRelated(
            Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return false;
        }

        /// <summary>
        /// Determines whether the specified exception is a result of problems related to transactions isolation.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is a transactions isolation problem; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsTransactionRelated(
            Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return false;
        }

        /// <summary>
        /// Determines whether the specified exception allows for the operation to be repeated, e.g. optimistic concurrency, transaction killed, etc..
        /// Here it always returns <see langword="false"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns><see langword="false" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsTransient(
            Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return false;
        }

        #endregion

        /// <summary>
        /// Creates a <see cref="BaseDomainValue" /> derived object of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the object to be created.</typeparam>
        /// <returns>The created entity.</returns>
        public static T CreateEntity<T>() where T : BaseDomainEntity, new()
        {
            if (!typeof(DomainEntity<long, string>).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException("The repository does not support this type.");

            return CreateCollections(new T());
        }

        /// <summary>
        /// Creates an entity with the specified type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>System.Object.</returns>
        public static object CreateEntity(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            if (!typeof(DomainEntity<long, string>).IsAssignableFrom(entityType))
                throw new InvalidOperationException("The repository does not support this type.");

            return CreateCollections(Activator.CreateInstance(entityType));
        }

        /// <summary>
        /// Creates a <see cref="BaseDomainValue" /> derived object of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the object to be created.</typeparam>
        /// <returns>The created entity.</returns>
        public static T CreateValue<T>() where T : BaseDomainValue, new()
        {

            return CreateCollections(new T());
        }

        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>System.Object.</returns>
        public static object CreateValue(Type valueType)
        {
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));

            return CreateCollections(Activator.CreateInstance(valueType));
        }

        static object CreateCollections(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            foreach (var pi in instance
                                    .GetType()
                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(pi => pi.PropertyType.IsGenericType &&
                                                 pi.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                var collectionType = typeof(List<>).MakeGenericType(pi.PropertyType.GetGenericArguments()[0]);

                pi.SetValue(instance, collectionType.GetConstructor(Type.EmptyTypes).Invoke(null));
            }

            return instance;
        }

        static T CreateCollections<T>(T instance)
        {
            foreach (var pi in typeof(T)
                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(pi => pi.PropertyType.IsGenericType &&
                                                 pi.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                var collectionType = typeof(List<>).MakeGenericType(pi.PropertyType.GetGenericArguments()[0]);

                pi.SetValue(instance, collectionType.GetConstructor(Type.EmptyTypes).Invoke(null));
            }

            return instance;
        }
    }
}
