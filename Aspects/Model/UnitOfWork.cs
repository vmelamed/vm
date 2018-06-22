using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Transactions;

using CommonServiceLocator;

using vm.Aspects.Exceptions;
using vm.Aspects.Facilities.Diagnostics;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Represents a local 'unit of work' scope which:
    /// <list type="number">
    /// <item>Optionally creates a transaction scope</item>
    /// <item>Obtains an enlisted <see cref="IRepository"/> from the DI container</item>
    /// <item>Performs a number of operations, including operations with the repository</item>
    /// <item>Explicitly commits the changes in the repository</item>
    /// <item>Commits the transaction if any</item>
    /// <item>Disposes of the repository</item>
    /// <item>Disposes of the transaction if any</item>
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
        /// The default repository factory
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Func<OptimisticConcurrencyStrategy, string, IRepository> DefaultRepositoryFactory =
            (s,rn) =>
            {
                var r = ServiceLocator.Current.GetInstance<IRepository>(rn);
                r.OptimisticConcurrencyStrategy = s;
                return r;
            };

        /// <summary>
        /// The default transaction scope factory
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Func<TransactionScope> DefaultTransactionScopeFactory =
            () => new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);

        /// <summary>
        /// Gets or sets a value indicating whether to create explicitly transaction scope.
        /// The use of this property must be very well justified.
        /// </summary>
        public bool CreateTransactionScope { get; }

        /// <summary>
        /// Gets or sets the  resolve name of the repository.
        /// </summary>
        public string RepositoryResolveName { get; }

        /// <summary>
        /// Gets or sets the optimistic concurrency strategy.
        /// </summary>
        public OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        readonly Func<OptimisticConcurrencyStrategy,string,IRepository> _repositoryFactory;
        readonly Func<TransactionScope> _transactionScopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="repositoryResolveName">The repository resolve name.</param>
        /// <param name="optimisticConcurrencyStrategy">The optimistic concurrency strategy.</param>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="createTransactionScope">if set to <see langword="true" /> the <see cref="WorkAction" /> and <see cref="WorkFunc" /> will create a transaction scope for the unit of work.</param>
        /// <param name="transactionScopeFactory">The transaction scope factory if <see langword="null" /> a default factory will be used.</param>
        public UnitOfWork(
            string repositoryResolveName = null,
            OptimisticConcurrencyStrategy optimisticConcurrencyStrategy = OptimisticConcurrencyStrategy.None,
            Func<OptimisticConcurrencyStrategy, string, IRepository> repositoryFactory = null,
            bool createTransactionScope = false,
            Func<TransactionScope> transactionScopeFactory = null)
        {
            RepositoryResolveName          = repositoryResolveName;
            OptimisticConcurrencyStrategy  = optimisticConcurrencyStrategy;
            _repositoryFactory             = repositoryFactory ?? DefaultRepositoryFactory;
            CreateTransactionScope         = createTransactionScope;
            _transactionScopeFactory       = transactionScopeFactory ?? DefaultTransactionScopeFactory;
        }

        /// <summary>
        /// The method:
        /// <list type="number">
        /// <item>Optionally creates a transaction scope</item>
        /// <item>Creates a repository</item>
        /// <item>Executes the given <paramref name="work" /></item>
        /// <item>Commits the repository changes</item>
        /// <item>And commits the transaction scope, if any</item>
        /// </list>
        /// </summary>
        /// <param name="work">The action to be performed within the unit of work.</param>
        /// <exception cref="System.ArgumentNullException">work</exception>
        /// <exception cref="RepeatableOperationException"></exception>
        public void WorkAction(
            Action<IRepository> work)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));

            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory(OptimisticConcurrencyStrategy, RepositoryResolveName);

            try
            {
                VmAspectsEventSource.Log.UnitOfWorkStart();
                work(repository);
                repository.CommitChanges();
                transactionScope?.Complete();
            }
            catch (Exception x)
            {
                VmAspectsEventSource.Log.UnitOfWorkFailed(x);

                if (x.IsTransient())
                    throw new RepeatableOperationException(x);

                throw;
            }
            finally
            {
                VmAspectsEventSource.Log.UnitOfWorkStop();
                repository.Dispose();
                transactionScope?.Dispose();
            }
        }

        /// <summary>
        /// The method:
        /// <list type="number">
        /// <item>Optionally creates a transaction scope</item>
        /// <item>Creates a repository</item>
        /// <item>Executes the given <paramref name="work" /></item>
        /// <item>Commits the repository changes</item>
        /// <item>Commits the transaction scope, if any</item>
        /// <item>And returns the result of the <paramref name="work"/></item>
        /// </list>
        /// </summary>
        /// <param name="work">The action to be performed within the unit of work.</param>
        /// <exception cref="System.ArgumentNullException">work</exception>
        /// <exception cref="RepeatableOperationException"></exception>
        public TResult WorkFunc<TResult>(
            Func<IRepository, TResult> work)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));

            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory(OptimisticConcurrencyStrategy, RepositoryResolveName);

            try
            {
                VmAspectsEventSource.Log.UnitOfWorkStart();

                var result = work(repository);

                repository.CommitChanges();
                transactionScope?.Complete();

                return result;
            }
            catch (Exception x)
            {
                VmAspectsEventSource.Log.UnitOfWorkFailed(x);

                if (x.IsTransient())
                    throw new RepeatableOperationException(x);

                throw;
            }
            finally
            {
                VmAspectsEventSource.Log.UnitOfWorkStop();
                repository.Dispose();
                transactionScope?.Dispose();
            }
        }

        /// <summary>
        /// The asynchronous method:
        /// <list type="number">
        /// <item>Optionally creates a transaction scope</item>
        /// <item>Creates a repository</item>
        /// <item>Asynchronously executes the given <paramref name="work" /></item>
        /// <item>Asynchronously commits the repository changes</item>
        /// <item>And commits the transaction scope, if any</item>
        /// </list>
        /// </summary>
        /// <param name="work">The action to be performed within the unit of work.</param>
        /// <exception cref="System.ArgumentNullException">work</exception>
        /// <exception cref="RepeatableOperationException"></exception>
        public async Task WorkActionAsync(
            Func<IRepository, Task> work)
        {
            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory(OptimisticConcurrencyStrategy, RepositoryResolveName);

            try
            {
                VmAspectsEventSource.Log.UnitOfWorkStart();
                await work(repository);
                await repository.CommitChangesAsync();
                transactionScope?.Complete();
            }
            catch (AggregateException x)
            {
                VmAspectsEventSource.Log.UnitOfWorkFailed(x);

                if (x.InnerExceptions.Count != 1)
                    throw;

                // unwrap from AggregateException
                if (x.InnerExceptions[0].IsTransient())
                    throw new RepeatableOperationException(x.InnerExceptions[0]);
                else
                    throw x.InnerExceptions[0];
            }
            catch (Exception x)
            {
                VmAspectsEventSource.Log.UnitOfWorkFailed(x);
                throw;
            }
            finally
            {
                VmAspectsEventSource.Log.UnitOfWorkStop();
                repository.Dispose();
                transactionScope?.Dispose();
            }
        }

        /// <summary>
        /// The asynchronous method:
        /// <list type="number">
        /// <item>Optionally creates a transaction scope</item>
        /// <item>Creates a repository</item>
        /// <item>Asynchronously executes the given <paramref name="work" /></item>
        /// <item>Asynchronously commits the repository changes</item>
        /// <item>Commits the transaction scope, if any</item>
        /// <item>And returns the result of the <paramref name="work"/></item>
        /// </list>
        /// </summary>
        /// <param name="work">The action to be performed within the unit of work.</param>
        /// <exception cref="System.ArgumentNullException">work</exception>
        /// <exception cref="RepeatableOperationException"></exception>
        public async Task<TResult> WorkFuncAsync<TResult>(
            Func<IRepository, Task<TResult>> work)
        {
            var transactionScope = CreateTransactionScope ? _transactionScopeFactory() : null;
            var repository       = _repositoryFactory(OptimisticConcurrencyStrategy, RepositoryResolveName);

            try
            {
                VmAspectsEventSource.Log.UnitOfWorkStart();

                var result = await work(repository);

                await repository.CommitChangesAsync();
                transactionScope?.Complete();

                return result;
            }
            catch (AggregateException x)
            {
                VmAspectsEventSource.Log.UnitOfWorkFailed(x);

                if (x.InnerExceptions.Count != 1)
                    throw;

                if (x.InnerExceptions[0].IsTransient())
                    throw new RepeatableOperationException(x.InnerExceptions[0]);
                else
                    throw x.InnerExceptions[0];
            }
            catch (Exception x)
            {
                VmAspectsEventSource.Log.UnitOfWorkFailed(x);
                throw;
            }
            finally
            {
                VmAspectsEventSource.Log.UnitOfWorkStop();
                repository.Dispose();
                transactionScope?.Dispose();
            }
        }
    }
}
