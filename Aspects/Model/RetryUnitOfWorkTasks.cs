using System;
using System.Threading.Tasks;
using System.Transactions;
using vm.Aspects.Exceptions;
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
        /// The default delegate testing if the unit of work has failed. It returns <see langword="true"/> 
        /// if the unit of work raised a repository related exception that does not allow the operation to be repeated.
        /// </summary>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public readonly static new Func<T, Exception, int, Task<bool>> DefaultIsFailure = (r,x,i) =>
        /// {
        ///     if (x == null)
        ///         return Task.FromResult(false);
        /// 
        ///     var ax = x as AggregateException;
        /// 
        ///     if (ax != null)
        ///         x = UnwrapAggregateExceptionHandler.Unwrap(ax as AggregateException);
        /// 
        ///     return Task.FromResult(!(x is RepeatableOperationException) && !x.IsTransient());
        /// };]]>
        /// </code>
        /// </remarks>
        public readonly static new Func<T, Exception, int, Task<bool>> DefaultIsFailure = (r,x,i) =>
        {
            if (x == null)
                return Task.FromResult(false);

            var ax = x as AggregateException;

            if (ax != null)
                x = UnwrapAggregateExceptionHandler.Unwrap(ax as AggregateException);

            return Task.FromResult(!(x is RepeatableOperationException) && !x.IsTransient());
        };
        /// <summary>
        /// The default delegate testing if the unit of work has succeeded. It returns <see langword="true"/> 
        /// if the unit of work didn't raise an exception.
        /// </summary>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public readonly static new Func<T, Exception, int, Task<bool>> DefaultIsSuccess = (r,x,i) => Task.FromResult(x == null);]]>
        /// </code>
        /// </remarks>
        public readonly static new Func<T, Exception, int, Task<bool>> DefaultIsSuccess = (r,x,i) => Task.FromResult(x == null);
        /// <summary>
        /// The default epilogue delegate returns the result of the unit of work if there were no exceptions or
        /// throws the exception itself.
        /// </summary>
        /// <remarks>
        /// The default implementation is:
        /// <code>
        /// <![CDATA[public readonly static new Func<T, Exception, int, Task<T>> DefaultEpilogue = (r,x,i) => { if (x != null) throw x; else return Task.FromResult(r); };]]>
        /// </code>
        /// </remarks>
        public readonly static new Func<T, Exception, int, Task<T>> DefaultEpilogue = (r,x,i) => { if (x != null) throw x; else return Task.FromResult(r); };

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryUnitOfWorkTasks{T}"/> class.
        /// </summary>
        /// <param name="work">The work.</param>
        /// <param name="isFailure">A delegate testing if the unit of work has failed. Can be <see langword="null" /> in which case <see cref="DefaultIsFailure" /> will be invoked.</param>
        /// <param name="isSuccess">A delegate testing if the unit of work has succeeded. Can be <see langword="null" /> in which case <see cref="DefaultIsSuccess" /> will be invoked.</param>
        /// <param name="epilogue">A delegate invoked after the unit of work has been tried unsuccessfully <c>maxRetries</c>. Can be <see langword="null" /> in which case <see cref="DefaultEpilogue" /> will be invoked.</param>
        /// <param name="optimisticConcurrencyStrategy">The optimistic concurrency strategy for the repository.</param>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="transactionScopeFactory">The transaction scope factory.</param>
        /// <param name="createTransactionScope">if set to <see langword="true" /> creates a transaction scope.</param>
        public RetryUnitOfWorkTasks(
            Func<IRepository, int, Task<T>> work,
            Func<T, Exception, int, Task<bool>> isFailure = null,
            Func<T, Exception, int, Task<bool>> isSuccess = null,
            Func<T, Exception, int, Task<T>> epilogue = null,
            OptimisticConcurrencyStrategy optimisticConcurrencyStrategy = OptimisticConcurrencyStrategy.StoreWins,  // ClientWins probably doesn't make a lot of sense here
            Func<OptimisticConcurrencyStrategy, IRepository> repositoryFactory = null,
            Func<TransactionScope> transactionScopeFactory = null,
            bool createTransactionScope = false)
            : base(
                async i => await new UnitOfWork(
                                    optimisticConcurrencyStrategy,
                                    repositoryFactory,
                                    transactionScopeFactory,
                                    createTransactionScope)
                                .WorkFuncAsync(async r => await work(r, i)),
                isFailure ?? DefaultIsFailure,
                isSuccess ?? DefaultIsSuccess,
                epilogue  ?? DefaultEpilogue)
        {
        }
    }
}
