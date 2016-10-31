using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository.HiLoIdentity
{
    /// <summary>
    /// Class HiLoStoreIdProvider. Implements <see cref="T:IStoreUniqueId"/> for <see cref="EFRepositoryBase"/> 
    /// using Hi-Lo generators <see cref="T:HiLoIdentityGenerator"/> - one for each data set.
    /// </summary>
    public sealed class HiLoStoreIdProvider : IStoreIdProvider,
        IStoreUniqueId<long>,
        IStoreUniqueId<int>,
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
            Contract.Ensures(Contract.Result<IStoreUniqueId<TId>>() != null);
            Contract.Ensures(Contract.Result<IStoreUniqueId<TId>>() != null);

            var provider = this as IStoreUniqueId<TId>;

            if (provider == null)
                throw new NotSupportedException("The store ID provider does not support generating ID-s of type "+typeof(TId).FullName);

            return provider;
        }
        #endregion

        long IStoreUniqueId<long>.GetNewId<T>(
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            return DoGetNew<T>(efRepository);
        }

        long IStoreUniqueId<long>.GetNewId(
            Type objectsType,
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            return DoGetNew(objectsType, efRepository);
        }

        int IStoreUniqueId<int>.GetNewId<T>(
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            var id = DoGetNew<T>(efRepository);

            if (id > 0x7FFFFFFF0)
                throw new InvalidOperationException(
                    $"FATAL ERROR: the ID generator for type {typeof(T).FullName} has reached the maximum value.");

            return unchecked((int)id);
        }

        int IStoreUniqueId<int>.GetNewId(
            Type objectsType,
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            var id = DoGetNew(objectsType, efRepository);

            if (id > 0x7FFFFFFF0)
                throw new InvalidOperationException(
                    $"FATAL ERROR: the ID generator for type {objectsType.FullName} has reached the maximum value.");

            return unchecked((int)id);
        }

        Guid IStoreUniqueId<Guid>.GetNewId<T>(
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (!(repository is EFRepositoryBase))
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            return Facility.GuidGenerator.NewGuid();
        }

        Guid IStoreUniqueId<Guid>.GetNewId(
            Type objectsType,
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (!(repository is EFRepositoryBase))
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            return Facility.GuidGenerator.NewGuid();
        }

        /// <summary>
        /// The default resolve name of the repository supplying the ID-s. Must have transient lifetime.
        /// </summary>
        /// <remarks>
        /// Note that the code in <see cref="M:GetNew"/> requires that the repository instance must have transient lifetime.
        /// </remarks>
        public const string HiLoGeneratorsRepositoryResolveName = "HiLoRepository";

        /// <summary>
        /// The default entity set name - "_".
        /// </summary>
        public const string DefaultEntitySetName = "_";

        readonly Func<IRepository> _generatorsRepositoryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiLoStoreIdProvider"/> class.
        /// </summary>
        /// <param name="generatorsRepositoryFactory">
        /// The generators' repository factory method. If the factory is <see langword="null"/> 
        /// the constructor will try to resolve it from the service locator with resolve name <see cref="F:HiLoGeneratorsRepositoryResolveName"/>
        /// </param>
        public HiLoStoreIdProvider(
            [Dependency(HiLoGeneratorsRepositoryResolveName)]
            Func<IRepository> generatorsRepositoryFactory)
        {
            _generatorsRepositoryFactory = generatorsRepositoryFactory ??
                                                (() => ServiceLocator.Current.GetInstance<IRepository>(HiLoGeneratorsRepositoryResolveName));
        }

        #region Transaction scope defaults:
        /// <summary>
        /// The default transaction scope option: RequiresNew
        /// </summary>
        public const TransactionScopeOption DefaultTransactionScopeOption = TransactionScopeOption.RequiresNew;

        /// <summary>
        /// The default transaction timeout in number of seconds: 60sec / 30min in DEBUG mode
        /// </summary>
        public const int DefaultTransactionTimeoutSeconds
#if DEBUG
                    		= 30 * 60;
#else
                            = 60;
#endif
        /// <summary>
        /// The default transaction timeout. 
        /// </summary>
        static readonly TimeSpan DefaultTransactionTimeout = new TimeSpan(0, 0, DefaultTransactionTimeoutSeconds);

        /// <summary>
        /// Specifies the default isolation level of the transactions: Serializable
        /// </summary>
        public const IsolationLevel DefaultIsolationLevel = IsolationLevel.Serializable;
        #endregion

        #region The HiLoGenerator objects and the related properties:
        /// <summary>
        /// The per entity set generator objects.
        /// </summary>
        readonly IDictionary<string, HiLoIdentityGenerator> _generators = new Dictionary<string, HiLoIdentityGenerator>();
        /// <summary>
        /// Synchronizes the access to the generators.
        /// </summary>
        readonly object _sync = new object();
        #endregion

        HiLoIdentityGenerator CreateOrGetFreshGenerator(
            string entitySetName)
        {
            Contract.Requires<ArgumentNullException>(entitySetName!=null, nameof(entitySetName));
            Contract.Requires<ArgumentNullException>(entitySetName!=null, nameof(entitySetName));
            Contract.Requires<ArgumentException>(entitySetName.Length > 0, "The argument "+nameof(entitySetName)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(entitySetName.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(entitySetName)+" cannot be empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<HiLoIdentityGenerator>() != null);

            HiLoIdentityGenerator generator;

            // Start a new, independent serializable transaction and use a repository with a *transient* lifetime manager.
            // We don't want a failed external transaction to invalidate our in-memory generators.
            using (var transactionScope = new TransactionScope(
                                                DefaultTransactionScopeOption,
                                                new TransactionOptions
                                                {
                                                    IsolationLevel = DefaultIsolationLevel,
                                                    Timeout        = DefaultTransactionTimeout,
                                                }))
            {
                using (var localRepository = _generatorsRepositoryFactory())
                {
                    if (localRepository == null)
                        throw new ConfigurationErrorsException("Could not resolve a repository for the Hi-Lo generators with resolve name " + HiLoGeneratorsRepositoryResolveName);

                    // get a fresh generator object from the DB
                    generator = localRepository
                                    .Entities<HiLoIdentityGenerator>()
                                    .FirstOrDefault(g => g.EntitySetName == entitySetName)
                                    ;

                    if (generator == null)
                    {
                        // create a new generator
                        generator = new HiLoIdentityGenerator(entitySetName);
                        localRepository.Add(generator);
                    }

                    generator.IncrementHighValue();
                    localRepository.CommitChanges();
                }

                // the newly loaded generator owns the range from (HighValue-1)*MaxLowValue to HighValue*(MaxLowValue-1).
                transactionScope.Complete();
            }

            // replace the old generator in the hash table with the fresh one
            _generators[entitySetName] = generator;
            return generator;
        }

        long DoGetNew<T>(EFRepositoryBase efRepository)
        {
            Contract.Requires<ArgumentNullException>(efRepository != null, nameof(efRepository));

            long id = -1L;
            HiLoIdentityGenerator generator;
            var entitySetName = efRepository.ObjectContext.GetEntitySetName<T>() ?? DefaultEntitySetName;

            // make sure that the _generators hash table is accessible from this thread only
            lock (_sync)
            {
                if (_generators.TryGetValue(entitySetName, out generator))
                    id = generator.GetId();
                // if GetId returns -1, we'll need a fresh generator from the database with its own new HighValue.

                if (id == -1L)
                {
                    generator = CreateOrGetFreshGenerator(entitySetName);
                    id = generator.GetId();
                }
            }

            return id;
        }

        long DoGetNew(
            Type objectsType,
            EFRepositoryBase efRepository)
        {
            Contract.Requires<ArgumentNullException>(efRepository != null, nameof(efRepository));

            long id = -1L;
            HiLoIdentityGenerator generator;
            var entitySetName = efRepository.ObjectContext.GetEntitySetName(objectsType) ?? DefaultEntitySetName;

            // make sure that the _generators hash table is accessible from this thread only
            lock (_sync)
            {
                if (_generators.TryGetValue(entitySetName, out generator))
                    id = generator.GetId();
                // if GetId returns -1, we'll need a fresh generator from the database with its own new HighValue.

                if (id == -1L)
                {
                    generator = CreateOrGetFreshGenerator(entitySetName);
                    id = generator.GetId();
                }
            }

            return id;
        }
    }
}
