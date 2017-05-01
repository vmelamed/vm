using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Transactions;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository.HiLoIdentity
{
    /// <summary>
    /// Class HiLoStoreIdProvider. Implements <see cref="IStoreUniqueId{T}"/> for <see cref="EFRepositoryBase"/> 
    /// using Hi-Lo generators <see cref="HiLoIdentityGenerator"/> - one for each data set.
    /// </summary>
    public sealed class HiLoStoreIdProvider : IStoreIdProvider,
        IStoreUniqueId<long>,
        IStoreUniqueId<int>,
        IStoreUniqueId<Guid>
    {
        #region Constants
        /// <summary>
        /// The default resolve name of the repository supplying the ID-s. Must have transient lifetime.
        /// </summary>
        /// <remarks>
        /// Note that the code in <see cref="GetOrCreateFreshGenerator"/> requires that the resolved repository instance must have transient lifetime, so that it is not reused by any other.
        /// </remarks>
        public const string HiLoGeneratorsRepositoryResolveName = "HiLoRepository";

        /// <summary>
        /// The default entity set name - "_".
        /// </summary>
        public const string DefaultEntitySetName = "_";

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
        /// Synchronizes the access to the generators dictionary.
        /// </summary>
        readonly object _sync = new object();
        /// <summary>
        /// The per entity set generator objects.
        /// </summary>
        readonly IDictionary<string, HiLoIdentityGenerator> _generators = new Dictionary<string, HiLoIdentityGenerator>();
        /// <summary>
        /// The generators repository factory
        /// </summary>
        readonly Func<IRepository> _generatorsRepositoryFactory;
        /// <summary>
        /// If set to <see langword="true" /> getting an ID will be done from within a new transaction scope.
        /// Especially needed when the owning repository is enlisted in external transaction: we do not want
        /// the outcomes of the two transactions to interfere each other.
        /// </summary>
        readonly bool _useTransactionScope;
        #endregion

        /// <summary>
        /// The default generators repository factory delegate.
        /// </summary>
        public static Func<IRepository> DefaultGeneratorsRepositoryFactory { get; } = () => DIContainer.Root.Resolve<IRepository>(HiLoGeneratorsRepositoryResolveName);

        /// <summary>
        /// Initializes a new instance of the <see cref="HiLoStoreIdProvider" /> class.
        /// </summary>
        /// <param name="generatorsRepositoryFactory">The generators repository factory.</param>
        [InjectionConstructor]
        public HiLoStoreIdProvider(
            [Dependency(HiLoGeneratorsRepositoryResolveName)] Func<IRepository> generatorsRepositoryFactory) : this(generatorsRepositoryFactory, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiLoStoreIdProvider" /> class.
        /// </summary>
        /// <param name="generatorsRepositoryFactory">The generators repository factory.</param>
        /// <param name="useTransactionScope">If set to <see langword="true" /> getting an ID will be done from within a new transaction scope.</param>
        public HiLoStoreIdProvider(
            Func<IRepository> generatorsRepositoryFactory = null,
            bool useTransactionScope = false)
        {
            _generatorsRepositoryFactory = generatorsRepositoryFactory ?? DefaultGeneratorsRepositoryFactory;
            _useTransactionScope         = useTransactionScope;
        }

        #region IStoreIdProvider
        /// <summary>
        /// Gets a provider which generates ID sequence of type <typeparamref name="TId" />.
        /// </summary>
        /// <typeparam name="TId">The type of the generated ID-s.</typeparam>
        /// <returns>IStoreUniqueId&lt;TId&gt;.</returns>
        /// <exception cref="System.NotSupportedException">The store ID provider does not support generating ID-s of type +typeof(TId).FullName</exception>
        public IStoreUniqueId<TId> GetProvider<TId>() where TId : IEquatable<TId>
        {
            Contract.Ensures(Contract.Result<IStoreUniqueId<TId>>() != null);

            var provider = this as IStoreUniqueId<TId>;

            if (provider == null)
                throw new NotSupportedException("The store ID provider does not support generating ID-s of type "+typeof(TId).FullName);

            return provider;
        }
        #endregion

        #region IStoreUniqueId<long>
        long IStoreUniqueId<long>.GetNewId<T>(
            IRepository repository)
        {
            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            return DoGetNew<T>(efRepository);
        }

        long IStoreUniqueId<long>.GetNewId(
            Type objectsType,
            IRepository repository)
        {
            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            return DoGetNew(objectsType, efRepository);
        }
        #endregion

        #region IStoreUniqueId<int>
        int IStoreUniqueId<int>.GetNewId<T>(
            IRepository repository)
        {
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
            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            var id = DoGetNew(objectsType, efRepository);

            if (id > 0x7FFFFFFF0)
                throw new InvalidOperationException(
                    $"FATAL ERROR: the ID generator for type {objectsType.FullName} has reached the maximum value.");

            return unchecked((int)id);
        }
        #endregion

        #region IStoreUniqueId<Guid>
        Guid IStoreUniqueId<Guid>.GetNewId<T>(
            IRepository repository)
        {
            if (!(repository is EFRepositoryBase))
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            return Facility.GuidGenerator.NewGuid();
        }

        Guid IStoreUniqueId<Guid>.GetNewId(
            Type objectsType,
            IRepository repository)
        {
            if (!(repository is EFRepositoryBase))
                throw new ArgumentException("The repository must be derived from EFRepositoryBase.", nameof(repository));

            return Facility.GuidGenerator.NewGuid();
        }
        #endregion

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
                    generator = GetOrCreateFreshGenerator(entitySetName);
                    id = generator.GetId();
                }
            }

            return id;
        }

        long DoGetNew(
            Type objectsType,
            EFRepositoryBase efRepository)
        {
            Contract.Requires<ArgumentNullException>(objectsType != null, nameof(objectsType));
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
                    generator = GetOrCreateFreshGenerator(entitySetName);
                    id = generator.GetId();
                }
            }

            return id;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "will do on the way back.")]
        HiLoIdentityGenerator GetOrCreateFreshGenerator(
            string entitySetName)
        {
            Contract.Requires<ArgumentNullException>(entitySetName != null, nameof(entitySetName));
            Contract.Requires<ArgumentException>(entitySetName.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(entitySetName)+" cannot be empty string or consist of whitespace characters only.");

            Contract.Ensures(Contract.Result<HiLoIdentityGenerator>() != null);

            HiLoIdentityGenerator generator;

            // Start a new, independent serializable transaction and use a repository with a *transient* lifetime manager.
            // We don't want a failed external transaction to invalidate our in-memory generators.
            TransactionScope transactionScope = null;

            // open transaction scope if requested
            if (_useTransactionScope)
                transactionScope = new TransactionScope(
                                                DefaultTransactionScopeOption,
                                                new TransactionOptions
                                                {
                                                    IsolationLevel = DefaultIsolationLevel,
                                                    Timeout        = DefaultTransactionTimeout,
                                                });

            try
            {
                using (var repository = _generatorsRepositoryFactory())
                {
                    // get a fresh generator object from the DB
                    generator = repository
                                    .Entities<HiLoIdentityGenerator>()
                                    .FirstOrDefault(g => g.EntitySetName == entitySetName)
                                    ;

                    if (generator == null)
                    {
                        // create a new generator
                        generator = repository.CreateEntity<HiLoIdentityGenerator>();
                        generator.EntitySetName = entitySetName;

                        repository.Add(generator);
                    }

                    generator.IncrementHighValue();

                    repository.CommitChanges();
                    transactionScope?.Complete();
                }

                // now the newly loaded generator owns the range from (HighValue-1)*MaxLowValue to HighValue*(MaxLowValue-1).
                // replace the old generator in the hash table with the fresh one
                _generators[entitySetName] = generator;
                return generator;
            }
            finally
            {
                if (_useTransactionScope)
                    transactionScope.Dispose();
            }
        }
    }
}
