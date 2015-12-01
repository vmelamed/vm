using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace vm.Aspects.Model.Repository
{
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
