using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using vm.Aspects.Model.EFRepository.HiLoIdentity;
using vm.Aspects.Validation;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Class EFRepositoryBase. Implements <see cref="T:IRepository"/> with Entity Framework <see cref="T:DbContext"/>.
    /// </summary>
    public abstract partial class EFRepositoryBase : DbContext
    {
#if EFProfiler
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static EFRepositoryBase()
        {
            try
            {
                HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
            }
            catch (Exception x)
            {
                EventLog.WriteEntry("vm.Aspects", x.ToString(), EventLogEntryType.Error);
                throw;
            }
        }
#endif
        /// <summary>
        /// Gets or sets the underlying store unique ID (PK) provider.
        /// </summary>
        public IStoreIdProvider StoreIdProvider { get; set; }

        /// <summary>
        /// Gets the object context.
        /// </summary>
        public ObjectContext ObjectContext
        {
            get
            {
                Contract.Ensures(Contract.Result<ObjectContext>() != null);

                return ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        /// <summary>
        /// Gets the repository instance identifier.
        /// </summary>
        public Guid InstanceId { get; } = Guid.NewGuid();

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EFRepository" /> class.
        /// </summary>
        /// <param name="storeIdProvider">
        /// The injected store id provider. If <see langword="null"/>, the constructor will try to resolve it from the service locator and if that fails,
        /// <see cref="T:SqlStoreIdentity"/> will be used.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected EFRepositoryBase(
            IStoreIdProvider storeIdProvider = null)
            : base()
        {
            GetUniqueIdProvider(storeIdProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EFRepository" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="storeIdProvider">
        /// The injected store id generator. If <see langword="null"/>, the constructor will try to resolve it from the service locator and if that fails,
        /// <see cref="T:SqlStoreIdentity"/> will be used.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected EFRepositoryBase(
            string connectionString,
            IStoreIdProvider storeIdProvider = null)
            : base(connectionString)
        {
            GetUniqueIdProvider(storeIdProvider);
        }
        #endregion

        void GetUniqueIdProvider(
            IStoreIdProvider storeIdProvider)
        {
            if (storeIdProvider != null)
                StoreIdProvider = storeIdProvider;
            else
            {
                try
                {
                    StoreIdProvider = ServiceLocator.Current?.GetInstance<IStoreIdProvider>();

                    if (StoreIdProvider == null)
                        StoreIdProvider = new SqlStoreIdProvider();
                }
                catch (InvalidOperationException)
                {
                    StoreIdProvider = new SqlStoreIdProvider();
                }
                catch (ActivationException)
                {
                    StoreIdProvider = new SqlStoreIdProvider();
                }
                catch (ResolutionFailedException)
                {
                    StoreIdProvider = new SqlStoreIdProvider();
                }
            }
        }

        /// <summary>
        /// Tests whether the entities of type <c>T</c> are properly defined as proxy-able and change-tracking.
        /// </summary>
        /// <typeparam name="T">The type of the entity to test.</typeparam>
        /// <returns><see langword="true"/> if the entity is properly defined; otherwise <see langword="false"/></returns>
        /// <remarks>
        /// Note that the test is performed only in debug builds.
        /// </remarks>
        protected bool IsChangeTrackingProxy<T>(
            Action<T> initialize) where T : class, new()
        {
            var instance = Set<T>().FirstOrDefault();
            var remove = false;

            if (instance == null)
            {
                instance = Set<T>().Create();
                if (instance == null)
                    return false;
                if (initialize != null)
                    initialize(instance);
                Set<T>().Add(instance);
                remove = true;
                CommitChanges();
            }

            var isChangeTracking = new EFSpecifics().IsChangeTracking(instance, this);

            if (remove)
            {
                Set<T>().Remove(instance);
                CommitChanges();
            }

            if (!isChangeTracking)
            {
                Debug.WriteLine("The repository does not create change-tracking dynamic proxies for instances of type {0}.", typeof(T).FullName, null);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        /// before it has been locked down and used to initialize the context.  Here we add the configuration of the <see cref="T:HiLoIdentityGenerator"/>. 
        /// It should be overridden in a derived class such that the model can be further configured before it is locked down.
        /// Note: in the overriding methods must always call <c>base.OnModelCreating(modelBuilder);</c> first to configure the
        /// <see cref="T:HiLoIdentityGenerator"/>.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        protected override void OnModelCreating(
            DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Conventions
                .Add(
                    new StringAttributeConvention(),
                    new NumericAttributeConvention(),
                    new DateTimeSqlTypeConvention());

            modelBuilder
                .Conventions
                .Remove<PluralizingTableNameConvention>();

            if (StoreIdProvider is HiLoStoreIdProvider)
                // Add the configuration of the HiLo PK generator generators
                modelBuilder.Configurations
                            .Add(new HiLoIdentityGeneratorConfiguration());
        }

        /// <summary>
        /// Saves all changes made in this context pending to be serialized to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        /// <remarks>Note: there isn't much that we can do here yet.</remarks>
        public override int SaveChanges()
        {
            PreprocessEntries();
            return base.SaveChanges();
        }

        /// <summary>
        /// save changes as an asynchronous operation.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).</returns>
        /// <remarks>Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.</remarks>
        public override Task<int> SaveChangesAsync()
        {
            PreprocessEntries();
            return base.SaveChangesAsync();
        }

        /// <summary>
        /// Pre-processes the entries that are to be committed. E.g: adds audit data, logs, etc.
        /// </summary>
        protected virtual void PreprocessEntries()
        {
        }

        /// <summary>
        /// Extension point allowing the user to customize validation of an entity or filter out validation results.
        /// Called by <see cref="M:System.Data.Entity.DbContext.GetValidationErrors"/>.
        /// </summary>
        /// <param name="entityEntry">DbEntityEntry instance to be validated.</param>
        /// <param name="items">User defined dictionary containing additional info for custom validation.
        /// It will be passed to <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext"/>
        /// and will be exposed as <see cref="P:System.ComponentModel.DataAnnotations.ValidationContext.Items"/>.
        /// This parameter is optional and can be null.</param>
        /// <returns>
        /// Entity validation result. Possibly null when overridden.
        /// </returns>
        /// <remarks>Note: validating each entity causes lazy loading of all related objects and this can get very expensive.</remarks>
        protected override DbEntityValidationResult ValidateEntity(
            DbEntityEntry entityEntry,
            IDictionary<object, object> items)
        {
            Contract.Ensures(Contract.Result<DbEntityValidationResult>() != null);

            if (entityEntry == null)
                throw new ArgumentNullException(nameof(entityEntry));

            IValidatable validatable = entityEntry.Entity as IValidatable;

            if (validatable == null)
                return base.ValidateEntity(entityEntry, items);

            var results = validatable.Validate();

            if (results.IsValid)
                return new DbEntityValidationResult(entityEntry, new DbValidationError[] { });

            return new DbEntityValidationResult(
                            entityEntry,
                            ToValidationErrors(
                                results,
                                new List<DbValidationError>()).ToArray());
        }


        /// <summary>
        /// Converts and adds a sequence of EL <see cref="ValidationResult"/>-s to a list of <see cref="DbValidationError"/>-s.
        /// </summary>
        /// <param name="results">The results to convert.</param>
        /// <param name="errors">The errors to add to.</param>
        /// <returns>The list of errors.</returns>
        IList<DbValidationError> ToValidationErrors(
            IEnumerable<ValidationResult> results,
            IList<DbValidationError> errors)
        {
            Contract.Requires<ArgumentNullException>(results != null, nameof(results));
            Contract.Requires<ArgumentNullException>(errors != null, nameof(errors));
            Contract.Ensures(Contract.Result<IList<DbValidationError>>() != null);

            foreach (var r in results)
            {
                errors.Add(new DbValidationError(r.Key, r.Message));
                if (r.NestedValidationResults != null)
                    ToValidationErrors(r.NestedValidationResults, errors);
            }

            return errors;
        }

        /// <summary>
        /// Disposes the context. The underlying <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> is also disposed if it was created
        /// is by this context or ownership was passed to this context when this context was created.
        /// The connection to the database (<see cref="T:System.Data.Common.DbConnection" /> object) is also disposed if it was created
        /// is by this context or ownership was passed to this context when this context was created.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                var disposable = StoreIdProvider as IDisposable;

                if (disposable != null)
                    disposable.Dispose();
            }
            base.Dispose(disposing);
        }

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(StoreIdProvider != null);
        }

    }
}
