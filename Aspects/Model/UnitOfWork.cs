using Microsoft.Practices.ServiceLocation;
using System;
using System.Data.Entity.Infrastructure;
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
    ///                             (r,_) =>
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
    ///                                 (r,_) =>
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
        public Func<TransactionScope> DefaultTransactionScopeFactory = () => new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);

        /// <summary>
        /// Resolve constant for transient repositories.
        /// </summary>
        public const string TransientRepository = "transient";

        /// <summary>
        /// The default repository factory
        /// </summary>
        public Func<IRepository> DefaultRepositoryFactory = () => ServiceLocator.Current.GetInstance<IRepository>(TransientRepository);

        /// <summary>
        /// Gets or sets a value indicating whether to create explicitly transaction scope.
        /// The use of this property must be very well justified.
        /// </summary>
        public bool CreateTransactionScope { get; set; }

        /// <summary>
        /// Gets or sets the optimistic concurrency strategy.
        /// </summary>
        public OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        OptimisticConcurrencyExceptionHandler _concurrencyExceptionHandler;
        Func<TransactionScope> _transactionScopeFactory;
        Func<IRepository> _repositoryFactory;
        Random _random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="optimisticConcurrencyStrategy">The optimistic concurrency strategy.</param>
        /// <param name="transactionScopeFactory">The transaction scope factory if <see langword="null" /> a default factory will be used.</param>
        /// <param name="createTransactionScope">if set to <see langword="true" /> the <see cref="ActionWork" /> and <see cref="FuncWork" /> will create a transaction scope for the unit of work.</param>
        /// <param name="maxOptimisticConcurrencyRetries">The maximum optimistic concurrency retries.</param>
        /// <param name="minDelayBeforeRetry">The minimum delay before retry.</param>
        /// <param name="maxDelayBeforeRetry">The maximum delay before retry.</param>
        /// <param name="logExceptionWarnings">if set to <see langword="true" /> will log a warning for concurrency exceptions.</param>
        public UnitOfWork(
            Func<IRepository> repositoryFactory = null,
            OptimisticConcurrencyStrategy optimisticConcurrencyStrategy = OptimisticConcurrencyExceptionHandler.DefaultOptimisticConcurrencyStrategy,
            Func<TransactionScope> transactionScopeFactory = null,
            bool createTransactionScope = false,
            int maxOptimisticConcurrencyRetries = OptimisticConcurrencyExceptionHandler.DefaultMaxOptimisticConcurrencyRetries,
            int minDelayBeforeRetry = OptimisticConcurrencyExceptionHandler.DefaultMinDelayBeforeRetry,
            int maxDelayBeforeRetry = OptimisticConcurrencyExceptionHandler.DefaultMaxDelayBeforeRetry,
            bool logExceptionWarnings = true)
        {
            Contract.Requires<ArgumentException>(maxOptimisticConcurrencyRetries >= 0, nameof(maxOptimisticConcurrencyRetries)+" cannot be negative");
            Contract.Requires<ArgumentException>(minDelayBeforeRetry             >= 0, nameof(minDelayBeforeRetry)+" cannot be negative");
            Contract.Requires<ArgumentException>(maxDelayBeforeRetry             >= 0, nameof(maxDelayBeforeRetry)+" cannot be negative");

            _repositoryFactory              = repositoryFactory ?? DefaultRepositoryFactory;
            _transactionScopeFactory        = transactionScopeFactory ?? DefaultTransactionScopeFactory;
            CreateTransactionScope          = createTransactionScope;

            _concurrencyExceptionHandler = new OptimisticConcurrencyExceptionHandler(
                optimisticConcurrencyStrategy,
                maxOptimisticConcurrencyRetries,
                minDelayBeforeRetry,
                maxDelayBeforeRetry,
                logExceptionWarnings);
        }

        /// <summary>
        /// The method offers a retrying capability for transient errors like connection, optimistic concurrency and transaction kill exceptions.
        /// </summary>
        /// <param name="logic">The action to be performed within the unit of work.</param>
        public void ActionWork(
            Action<IRepository, int> logic)
        {
            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory();

            try
            {
                var success = false;

                _concurrencyExceptionHandler.Attempts = 0;
                while (!success)
                    try
                    {
                        logic(repository, _concurrencyExceptionHandler.Attempts);
                        repository.CommitChanges();
                        transactionScope?.Complete();
                        success = true;
                    }
                    catch (DbUpdateConcurrencyException x)
                    {
                        _concurrencyExceptionHandler.HandleDbUpdateConcurrencyException(x);
                    }
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
        /// <param name="logic">The function to be performed within the unit of work.</param>
        /// <returns>TResult.</returns>
        public TResult FuncWork<TResult>(
            Func<IRepository, int, TResult> logic)
        {
            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory();

            try
            {
                TResult result = default(TResult);
                var success    = false;

                _concurrencyExceptionHandler.Attempts = 0;
                while (!success)
                    try
                    {
                        result = logic(repository, _concurrencyExceptionHandler.Attempts);
                        repository.CommitChanges();
                        transactionScope?.Complete();
                        return result;
                    }
                    catch (DbUpdateConcurrencyException x)
                    {
                        _concurrencyExceptionHandler.HandleDbUpdateConcurrencyException(x);
                    }
            }
            finally
            {
                repository.Dispose();
                transactionScope?.Dispose();
            }

            return default(TResult);
        }

        /// <summary>
        /// The method offers a retrying capability for transient errors like connection, optimistic concurrency and transaction kill exceptions.
        /// </summary>
        /// <param name="logic">The async action to be performed within the unit of work.</param>
        public async Task ActionWorkAsync(
            Func<IRepository, int, Task> logic)
        {
            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory();

            try
            {
                var success = false;

                _concurrencyExceptionHandler.Attempts = 0;
                while (!success)
                    try
                    {
                        await logic(repository, _concurrencyExceptionHandler.Attempts);
                        await repository.CommitChangesAsync();
                        transactionScope?.Complete();
                        return;
                    }
                    catch (DbUpdateConcurrencyException x)
                    {
                        await _concurrencyExceptionHandler.HandleDbUpdateConcurrencyExceptionAsync(x);
                    }
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
        /// <param name="logic">The async function to be performed within the unit of work.</param>
        /// <returns>TResult.</returns>
        public async Task<TResult> FuncWorkAsync<TResult>(
            Func<IRepository, int, Task<TResult>> logic)
        {
            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory();

            try
            {
                TResult result = default(TResult);
                var success    = false;

                _concurrencyExceptionHandler.Attempts = 0;
                while (!success)
                    try
                    {
                        result = await logic(repository, _concurrencyExceptionHandler.Attempts);
                        await repository.CommitChangesAsync();
                        transactionScope?.Complete();
                        return result;
                    }
                    catch (DbUpdateConcurrencyException x)
                    {
                        await _concurrencyExceptionHandler.HandleDbUpdateConcurrencyExceptionAsync(x);
                    }
            }
            finally
            {
                repository.Dispose();
                transactionScope?.Dispose();
            }

            return default(TResult);
        }
    }
}
