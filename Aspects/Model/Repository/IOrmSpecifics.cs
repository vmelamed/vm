using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace vm.Aspects.Model.Repository
{
    /// <summary>
    /// Defines a set of bridged operation(s) (see G4 patterns) which are done and named differently in the various ORM-s 
    /// but have the same semantics for the client.
    /// </summary>
    [ContractClass(typeof(IOrmSpecificsContract))]
    public interface IOrmSpecifics
    {
        /// <summary>
        /// Suggests eager fetching of related objects when querying the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the queried sequence.</typeparam>
        /// <param name="sequence">The queryable sequence.</param>
        /// <param name="path">Specifies the navigation method/property to the property that should be eagerly loaded as a string.</param>
        /// <returns>The queryable sequence.</returns>
        IQueryable<T> Fetch<T>(IQueryable<T> sequence, string path) where T : BaseDomainEntity;

        /// <summary>
        /// Suggests eager fetching of related objects when querying the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the queried sequence.</typeparam>
        /// <typeparam name="TProperty">The type of the property to be eagerly loaded.</typeparam>
        /// <param name="sequence">The queryable sequence.</param>
        /// <param name="path">Specifies the navigation method/property to the property(s) that should be eagerly loaded as a lambda expression.</param>
        /// <returns>The queryable sequence.</returns>
        IQueryable<T> Fetch<T, TProperty>(IQueryable<T> sequence, Expression<Func<T, TProperty>> path) where T : BaseDomainEntity;

        /// <summary>
        /// Enlists the repository's back store transaction manager in the ambient transaction.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>The repository.</returns>
        IRepository EnlistInAmbientTransaction(IRepository repository);

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <param name="reference">The reference which POCO entity type is sought.</param>
        /// <returns>The POCO type of the reference.</returns>
        Type GetEntityType(object reference);

        /// <summary>
        /// Gets the name of the entity set associated with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="repository">The repository.</param>
        /// <returns>System.String.</returns>
        string GetEntitySetName(Type type, IRepository repository);

        /// <summary>
        /// Determines whether the specified reference is a reference to an ORM generated wrapper/proxy of the actual object instead of the actual object itself.
        /// </summary>
        /// <param name="reference">The reference to be tested.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified reference is proxy; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsProxy(object reference);

        /// <summary>
        /// Determines whether the specified reference is a reference to an ORM generated wrapper/proxy of the actual object and that it
        /// is tracking automatically the changes to the properties.
        /// </summary>
        /// <param name="reference">The reference to be tested.</param>
        /// <param name="repository">The repository.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified reference is proxy; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method is not supposed to be called from operational code but rather from verification code which makes sure that
        /// the reference is tracking the changes to its properties. In order for that to happen all properties must be declared <c>virtual</c>.
        /// </remarks>
        bool IsChangeTracking(object reference, IRepository repository);

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
        bool IsLoaded(object associated, object principal, string propertyName, IRepository repository);

        /// <summary>
        /// Determines whether the specified exception is a result of detected optimistic concurrency.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is an optimistic concurrency problem; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsOptimisticConcurrency(Exception exception);

        /// <summary>
        /// Determines whether the specified exception is a result of problems connecting to the store.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is a connection problem; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsConnectionRelated(Exception exception);

        /// <summary>
        /// Determines whether the specified exception is a result of problems related to transactions isolation.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is a transactions isolation problem; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsTransactionRelated(Exception exception);

        /// <summary>
        /// Determines whether the specified exception allows for the operation to be repeated, e.g. optimistic concurrency, transaction killed, etc..
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is allows for the operation to be repeated; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsTransient(Exception exception);
    }

    [ContractClassFor(typeof(IOrmSpecifics))]
    abstract class IOrmSpecificsContract : IOrmSpecifics
    {
        #region IOrmSpecifics Members

        public IQueryable<T> Fetch<T>(
            IQueryable<T> sequence,
            string path) where T : BaseDomainEntity
        {
            Contract.Requires<ArgumentNullException>(sequence != null, nameof(sequence));
            Contract.Requires<ArgumentNullException>(path != null, nameof(path));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(path), "path");
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            throw new NotImplementedException();
        }

        public IQueryable<T> Fetch<T, TProperty>(
            IQueryable<T> sequence,
            Expression<Func<T, TProperty>> path) where T : BaseDomainEntity
        {
            Contract.Requires<ArgumentNullException>(sequence != null, nameof(sequence));
            Contract.Requires<ArgumentNullException>(path != null, nameof(path));
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            throw new NotImplementedException();
        }


        public IRepository EnlistInAmbientTransaction(
            IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(repository != null, nameof(repository));
            Contract.Ensures(Contract.Result<IRepository>() != null);

            throw new NotImplementedException();
        }

        public Type GetEntityType(
            object reference)
        {
            Contract.Requires<ArgumentNullException>(reference != null, nameof(reference));

            throw new NotImplementedException();
        }

        public string GetEntitySetName(
            Type type,
            IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(repository != null, nameof(repository));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));

            throw new NotImplementedException();
        }

        public bool IsProxy(
            object reference)
        {
            Contract.Requires<ArgumentNullException>(reference != null, nameof(reference));

            throw new NotImplementedException();
        }

        public bool IsChangeTracking(
            object reference,
            IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(reference != null, nameof(reference));
            Contract.Requires<ArgumentNullException>(repository != null, nameof(repository));

            throw new NotImplementedException();
        }

        public bool IsLoaded(
            object associated,
            object principal,
            string propertyName,
            IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(associated != null, nameof(associated));
            Contract.Requires<ArgumentNullException>(principal != null, nameof(principal));
            Contract.Requires<ArgumentNullException>(propertyName != null, nameof(propertyName));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(propertyName), "The argument \"propertyName\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(repository != null, nameof(repository));

            throw new NotImplementedException();
        }

        public bool IsOptimisticConcurrency(
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            throw new NotImplementedException();
        }

        public bool IsConnectionRelated(Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            throw new NotImplementedException();
        }

        public bool IsTransactionRelated(Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            throw new NotImplementedException();
        }

        public bool IsTransient(Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            throw new NotImplementedException();
        }
        #endregion
    }
}
