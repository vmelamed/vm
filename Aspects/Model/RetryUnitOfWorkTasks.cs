using System;
using System.Threading.Tasks;
using System.Transactions;

using vm.Aspects.Model.Repository;
using vm.Aspects.Threading;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Combines <see cref="RetryTasks{T}"/> with <see cref="UnitOfWork"/>. <see cref="RetryTasks{T}.StartAsync(int, int, int)"/> calls an async work delegate.
    /// </summary>
    public class RetryUnitOfWorkTasks<T> : RetryTasks<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryUnitOfWorkTasks{T}"/> class.
        /// </summary>
        /// <param name="work">The work.</param>
        /// <param name="isFailure">A delegate testing if the unit of work has failed. Can be <see langword="null" /> in which case a default implementation will be invoked:
        /// <code><![CDATA[(r, x, i) => Task.FromResult(x != null  &&  !(x is RepeatableOperationException)  &&  !x.IsTransient())]]></code>
        /// </param>
        /// <param name="isSuccess">A delegate testing if the unit of work has succeeded. Can be <see langword="null" /> in which case a default implementation will be invoked
        /// <code>
        /// <![CDATA[(r, x, i) => Task.FromResult(x == null)]]>
        /// </code></param>
        /// <param name="epilogue">A delegate invoked after the unit of work has been tried unsuccessfully <c>maxRetries</c>. Can be <see langword="null" /> in which case a default implementation will be invoked.
        /// <code>
        /// <![CDATA[(r, x, i) => { if (x != null) throw x; else return Task.FromResult(r); })]]>
        /// </code></param>
        /// <param name="repositoryResolveName">The resolve name of the repository.</param>
        /// <param name="optimisticConcurrencyStrategy">The optimistic concurrency strategy for the repository.</param>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="createTransactionScope">if set to <see langword="true" /> creates a transaction scope.</param>
        /// <param name="transactionScopeFactory">The transaction scope factory.</param>
        public RetryUnitOfWorkTasks(
            Func<IRepository, int, Task<T>> work,
            Func<T, Exception, int, Task<bool>> isFailure = null,
            Func<T, Exception, int, Task<bool>> isSuccess = null,
            Func<T, Exception, int, Task<T>> epilogue = null,
            string repositoryResolveName = null,  // ClientWins probably doesn't make a lot of sense here
            OptimisticConcurrencyStrategy optimisticConcurrencyStrategy = OptimisticConcurrencyStrategy.None,
            Func<OptimisticConcurrencyStrategy, string, IRepository> repositoryFactory = null,
            bool createTransactionScope = false,
            Func<TransactionScope> transactionScopeFactory = null)
            : base(
                async i => await new UnitOfWork(
                                    repositoryResolveName,
                    optimisticConcurrencyStrategy,
                    repositoryFactory,
                    createTransactionScope,
                    transactionScopeFactory)
                                .WorkFuncAsync(async r => await work(r, i)),
                isFailure ?? RetryUnitOfWorkConstants.IsFailureAsync,
                isSuccess ?? RetryConstants.IsSuccessResultAsync,
                epilogue ?? RetryConstants.EpilogueAsync)
        {
        }
    }
}
