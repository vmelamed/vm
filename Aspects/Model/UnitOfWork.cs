using Microsoft.Practices.ServiceLocation;
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Transactions;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Represents a local 'unit of work' scope which:
    /// <list type="number">
    /// <item>Creates a transaction</item>
    /// <item>Obtains an enlisted <see cref="IRepository"/> from the DI container</item>
    /// <item>Performs a number of operations, including operations with the repository.</item>
    /// <item>Explicitly commits the changes in the repository</item>
    /// <item>Commits the transaction</item>
    /// <item>Disposes of the repository.</item>
    /// <item>Disposes of the transaction.</item>
    /// </list>
    /// See the examples below for recommended usage.
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// void DoWork(long fooId, Bar bar)
    /// {
    ///     var count = new UnitOfWork()
    ///                         .FuncWork(
    ///                             (r) =>
    ///                             {
    ///                                 var foo = r.Entities<Foo>.First(f => f.Id == fooId);
    ///                                 
    ///                                 foo.BarList.Add(bar);
    ///                                 return foo.BarList.Count();
    ///                             });
    ///     // etc.
    /// }
    /// 
    /// async Task DoWorkAsync(long fooId, Bar bar)
    /// {
    ///     var count = await new UnitOfWork()
    ///                             .FuncWorkAsync(
    ///                                 (r) =>
    ///                                 {
    ///                                     var foo = await r.Entities<Foo>.FirstAsync(f => f.Id == fooId);
    ///                                     
    ///                                     foo.BarList.Add(bar);
    ///                                     return foo.BarList.Count();
    ///                                 });
    ///     // etc.
    /// }
    /// ]]>
    /// </example>
    public class UnitOfWork
    {
        /// <summary>
        /// The default transaction scope factory
        /// </summary>
        public Func<TransactionScope> DefaultTransactionScopeFactory { get; } = () => new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);

        /// <summary>
        /// Resolve constant for transient repositories.
        /// </summary>
        public const string TransientRepository = "transient";

        /// <summary>
        /// The default repository factory
        /// </summary>
        public Func<IRepository> DefaultRepositoryFactory { get; } = () => ServiceLocator.Current.GetInstance<IRepository>(TransientRepository);

        /// <summary>
        /// Gets or sets a value indicating whether to create explicitly transaction scope.
        /// The use of this property must be very well justified.
        /// </summary>
        public bool CreateTransactionScope { get; set; }

        /// <summary>
        /// Gets or sets the optimistic concurrency strategy.
        /// </summary>
        public OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        readonly Func<TransactionScope> _transactionScopeFactory;
        readonly Func<IRepository> _repositoryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="transactionScopeFactory">The transaction scope factory if <see langword="null" /> a default factory will be used.</param>
        /// <param name="createTransactionScope">if set to <see langword="true" /> the <see cref="WorkAction" /> and <see cref="WorkFunc" /> will create a transaction scope for the unit of work.</param>
        public UnitOfWork(
            Func<IRepository> repositoryFactory = null,
            Func<TransactionScope> transactionScopeFactory = null,
            bool createTransactionScope = false)
        {
            _repositoryFactory       = repositoryFactory ?? DefaultRepositoryFactory;
            _transactionScopeFactory = transactionScopeFactory ?? DefaultTransactionScopeFactory;
            CreateTransactionScope   = createTransactionScope;
        }

        /// <summary>
        /// The method offers a retrying capability for transient errors like connection, optimistic concurrency and transaction kill exceptions.
        /// </summary>
        /// <param name="work">The action to be performed within the unit of work.</param>
        public void WorkAction(
            Action<IRepository> work)
        {
            Contract.Requires<ArgumentNullException>(work != null, nameof(work));

            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory();

            try
            {
                work(repository);

                repository.CommitChanges();
                transactionScope?.Complete();
            }
            finally
            {
                repository.Dispose();
                transactionScope?.Dispose();
            }
        }

        /// <summary>
        /// The method offers a retrying capability for transient errors like connection, optimistic concurrency and transaction kill exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="work">The function to be performed within the unit of work.</param>
        /// <returns>TResult.</returns>
        public TResult WorkFunc<TResult>(
            Func<IRepository, TResult> work)
        {
            Contract.Requires<ArgumentNullException>(work != null, nameof(work));

            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory();

            try
            {
                var result = work(repository);

                repository.CommitChanges();
                transactionScope?.Complete();
                return result;
            }
            finally
            {
                repository.Dispose();
                transactionScope?.Dispose();
            }
        }

        /// <summary>
        /// The method offers a retrying capability for transient errors like connection, optimistic concurrency and transaction kill exceptions.
        /// </summary>
        /// <param name="work">The async action to be performed within the unit of work.</param>
        public async Task WorkActionAsync(
            Func<IRepository, Task> work)
        {
            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory();

            try
            {
                await work(repository);

                await repository.CommitChangesAsync();
                transactionScope?.Complete();
            }
            finally
            {
                repository.Dispose();
                transactionScope?.Dispose();
            }
        }

        /// <summary>
        /// The method offers a retrying capability for transient errors like connection, optimistic concurrency and transaction kill exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="work">The async function to be performed within the unit of work.</param>
        /// <returns>TResult.</returns>
        public async Task<TResult> WorkFuncAsync<TResult>(
            Func<IRepository, Task<TResult>> work)
        {
            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory();

            try
            {
                var result = await work(repository);

                await repository.CommitChangesAsync();
                transactionScope?.Complete();

                return result;
            }
            finally
            {
                repository.Dispose();
                transactionScope?.Dispose();
            }
        }
    }
}
