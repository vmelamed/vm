using System;

using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Class SqlStoreIdProvider does not generate sequences but relies instead on the database engine to generate unique sequences instead.
    /// </summary>
    public sealed class SqlStoreIdProvider : IStoreIdProvider,
        IStoreUniqueId<int>,
        IStoreUniqueId<long>,
        IStoreUniqueId<DateTime>,
        IStoreUniqueId<Guid>
    {
        #region IStoreIdProvider Members

        /// <summary>
        /// Gets a provider which generates ID sequence of type <typeparamref name="TId" />.
        /// </summary>
        /// <typeparam name="TId">The type of the generated ID-s.</typeparam>
        /// <returns>IStoreUniqueId&lt;TId&gt;.</returns>
        /// <exception cref="System.NotSupportedException">The store ID provider does not support generating ID-s of type +typeof(TId).FullName</exception>
        public IStoreUniqueId<TId> GetProvider<TId>() where TId : IEquatable<TId>
        {
            if (!(this is IStoreUniqueId<TId> uniqueId))
                throw new NotSupportedException("The store ID provider does not support generating ID-s of type "+typeof(TId).FullName);

            return uniqueId;
        }
        #endregion

        #region IStoreUniqueId<int> Members
        int IStoreUniqueId<int>.GetNewId<T>(
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            // the value should be ignored by SQL Server
            return 0;
        }

        int IStoreUniqueId<int>.GetNewId(
            Type objectsType,
            IRepository repository)
        {
            if (objectsType == null)
                throw new ArgumentNullException(nameof(objectsType));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            // the value should be ignored by SQL Server
            return 0;
        }
        #endregion

        #region IStoreUniqueId<long> Members
        long IStoreUniqueId<long>.GetNewId<T>(
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            // the value should be ignored by SQL Server
            return 0L;
        }

        long IStoreUniqueId<long>.GetNewId(
            Type objectsType,
            IRepository repository)
        {
            if (objectsType == null)
                throw new ArgumentNullException(nameof(objectsType));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            // the value should be ignored by SQL Server
            return 0L;
        }
        #endregion

        #region IStoreUniqueId<DateTime> Members
        readonly static DateTime _0date = new DateTime(1900, 1, 1);

        // the value should be ignored by SQL Server
        DateTime IStoreUniqueId<DateTime>.GetNewId<T>(IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return _0date;
        }

        // the value should be ignored by SQL Server
        DateTime IStoreUniqueId<DateTime>.GetNewId(
            Type objectsType,
            IRepository repository)
        {
            if (objectsType == null)
                throw new ArgumentNullException(nameof(objectsType));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return _0date;
        }
        #endregion

        #region IStoreUniqueId<Guid> Members
        Guid IStoreUniqueId<Guid>.GetNewId<T>(IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return new Guid("0123456789ABCDEF0123456789ABCDEF");
        }

        Guid IStoreUniqueId<Guid>.GetNewId(Type objectsType, IRepository repository)
        {
            if (objectsType == null)
                throw new ArgumentNullException(nameof(objectsType));
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            return new Guid("0123456789ABCDEF0123456789ABCDEF");
        }
        #endregion
    }
}
