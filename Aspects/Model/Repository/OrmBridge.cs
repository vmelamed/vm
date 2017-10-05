using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using vm.Aspects.Model.InMemory;

namespace vm.Aspects.Model.Repository
{
    /// <summary>
    /// Class OrmBridge implements a bridge to repository/ORM specific functions leveraging a class implementing <see cref="IOrmSpecifics"/>.
    /// The methods are implemented as extension methods of the related classes.
    /// </summary>
    public static class OrmBridge
    {
        static object _sync = new object();
        static IOrmSpecifics _ormSpecifics;

        /// <summary>
        /// Initializes the <c>OrmBridge</c> class with implementation of <see cref="IOrmSpecifics"/>.
        /// </summary>
        public static void SetOrmSpecifics(
            IOrmSpecifics ormSpecifics)
        {
            if (ormSpecifics == null)
                throw new ArgumentNullException(nameof(ormSpecifics));

            lock (_sync)
                if (_ormSpecifics == null)
                    _ormSpecifics = ormSpecifics;
        }

        /// <summary>
        /// Gets the object implementing <see cref="IOrmSpecifics"/>. If not found in the service locator, defaults to <see cref="ObjectsRepositorySpecifics"/>.
        /// </summary>
        static IOrmSpecifics OrmSpecifics
        {
            get
            {

                if (_ormSpecifics == null)
                    try
                    {
                        SetOrmSpecifics(ServiceLocator.Current.GetInstance<IOrmSpecifics>());
                    }
                    catch (ResolutionFailedException)
                    {
                        SetOrmSpecifics(new ObjectsRepositorySpecifics());
                    }
                    catch (ActivationException)
                    {
                        SetOrmSpecifics(new ObjectsRepositorySpecifics());
                    }

                return _ormSpecifics;
            }
        }

        /// <summary>
        /// Suggests eager fetching of related objects when querying the repository.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence">The source.</param>
        /// <param name="path">The path.</param>
        /// <returns>IQueryable&lt;T&gt;.</returns>
        public static IQueryable<T> FetchAlso<T>(
            this IQueryable<T> sequence,
            string path) where T : class
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(path));

            return OrmSpecifics.FetchAlso(sequence, path);
        }

        /// <summary>
        /// Suggests eager fetching of related objects when querying the repository.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty">The type of the t property.</typeparam>
        /// <param name="sequence">The source.</param>
        /// <param name="path">The path.</param>
        /// <returns>IQueryable&lt;T&gt;.</returns>
        public static IQueryable<T> FetchAlso<T, TProperty>(
            this IQueryable<T> sequence,
            Expression<Func<T, TProperty>> path) where T : class
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return OrmSpecifics.FetchAlso(sequence, path);
        }

        /// <summary>
        /// Enlists the repository's back store transaction manager in the ambient transaction.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>The repository.</returns>
        public static IRepository EnlistInAmbientTransaction(
            this IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return OrmSpecifics.EnlistInAmbientTransaction(repository);
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <param name="reference">The reference which POCO entity type is sought.</param>
        /// <returns>The POCO type of the reference.</returns>
        public static Type GetEntityType(
            object reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            return OrmSpecifics.GetEntityType(reference);
        }

        /// <summary>
        /// Determines whether the specified entity object is a reference to an ORM generated wrapper/proxy of the actual value.
        /// </summary>
        /// <param name="entity">The entity object to be tested.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified reference is a proxy; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method is not supposed to be called from operational code but rather from verification code which makes sure that
        /// the reference is tracking the changes to its properties. In order for that to happen all properties must be declared <c>virtual</c>.
        /// </remarks>
        public static bool IsProxy(
            this object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return OrmSpecifics.IsProxy(entity);
        }

        /// <summary>
        /// Determines whether an object or collection of objects which is associated to a principal object is already loaded in memory by the repository.
        /// </summary>
        /// <param name="associated">The associated object or collection that is tested.</param>
        /// <param name="principal">The principal object.</param>
        /// <param name="propertyName">The name of the <paramref name="principal" />'s property whose value is the <paramref name="associated" />.</param>
        /// <param name="repository">The repository.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified reference is loaded; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsLoaded(
            this object associated,
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

            return OrmSpecifics.IsLoaded(associated, principal, propertyName, repository);
        }

        /// <summary>
        /// Determines whether an object or collection of objects which is associated to a principal object is already loaded in memory by the repository.
        /// </summary>
        /// <typeparam name="TAssociated">The type of the associated object.</typeparam>
        /// <typeparam name="TPrincipal">The type of the principal object.</typeparam>
        /// <param name="associated">The associated object or collection that is tested.</param>
        /// <param name="principal">The principal object.</param>
        /// <param name="propertyNameLambda">
        /// Must be a simple lambda expression of the type <c>principal =&gt; principal.Associated</c> which will supply the name of the property.
        /// </param>
        /// <param name="repository">The repository where the objects are or will be stored to.</param>
        /// <returns><see langword="true" /> if the specified reference is loaded; otherwise, <see langword="false" />.</returns>
        public static bool IsLoaded<TAssociated, TPrincipal>(
            this TAssociated associated,
            TPrincipal principal,
            Expression<Func<TPrincipal, TAssociated>> propertyNameLambda,
            IRepository repository)
            where TAssociated : class
            where TPrincipal : class
        {
            if (associated == null)
                throw new ArgumentNullException(nameof(associated));
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (propertyNameLambda == null)
                throw new ArgumentNullException(nameof(propertyNameLambda));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return OrmSpecifics.IsLoaded(associated, principal, propertyNameLambda.GetMemberName(), repository);
        }

        /// <summary>
        /// Determines whether the specified entity object is a reference to an ORM generated wrapper/proxy of the actual value and that it
        /// is tracking automatically the changes to the properties.
        /// </summary>
        /// <param name="entity">The entity object to be tested.</param>
        /// <param name="repository">The repository.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified reference is change tracking proxy; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method is not supposed to be called from operational code but rather from verification code which makes sure that
        /// the reference is tracking the changes to its properties. In order for that to happen all properties must be declared <c>virtual</c>.
        /// </remarks>
        public static bool IsChangeTracking(
            this object entity,
            IRepository repository)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return OrmSpecifics.IsChangeTracking(entity, repository);
        }

        /// <summary>
        /// Determines whether the specified exception is a result of detected optimistic concurrency.
        /// </summary>
        /// <param name="exception">The exception to be tested.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is an optimistic concurrency problem; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsOptimisticConcurrency(
            this Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return OrmSpecifics.IsOptimisticConcurrency(exception);
        }

        /// <summary>
        /// Determines whether the specified exception is a result of a transaction deadlock problems in the store.
        /// </summary>
        /// <param name="exception">The exception to be tested.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is a connection problem; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsTransactionRelated(
            this Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return OrmSpecifics.IsTransactionRelated(exception);
        }

        /// <summary>
        /// Determines whether the specified exception is a result of connectivity problems to the store.
        /// </summary>
        /// <param name="exception">The exception to be tested.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is a connection problem; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsConnectionRelated(
            this Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return OrmSpecifics.IsConnectionRelated(exception);
        }

        /// <summary>
        /// Determines whether the specified exception allows for the operation to be repeated, e.g. optimistic concurrency, transaction killed, etc.
        /// </summary>
        /// <param name="exception">The exception to be tested.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is allows for the operation to be repeated; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsTransient(
            this Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return OrmSpecifics.IsTransient(exception);
        }
    }
}
