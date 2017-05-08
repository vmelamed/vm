﻿using System;
using System.Transactions;
using vm.Aspects.Exceptions;
using vm.Aspects.Model.Repository;
using vm.Aspects.Threading;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Combines <see cref="Retry{T}" /> with <see cref="UnitOfWork" />. <see cref="Retry{T}.Start(int, int, int)" /> calls the unit of work delegate.
    /// </summary>
    /// <typeparam name="T">The result of the operation.
    /// Hint: if the operation does not have natural return value (i.e. has void return value) use some primitive type, e.g. <see cref="bool"/>.
    /// </typeparam>
    /// <seealso cref="vm.Aspects.Threading.Retry{T}" />
    public class RetryUnitOfWork<T> : Retry<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryUnitOfWork{T}" /> class.
        /// </summary>
        /// <param name="work">The delegate implementing the actual unit of work to be invoked between 1 and <c>maxRetries</c> in the method <see cref="Retry{T}.Start(int,int,int)"/>.</param>
        /// <param name="isFailure">A delegate testing if the unit of work has failed. Can be <see langword="null" /> in which case a default implementation will be invoked:
        /// <code><![CDATA[(r, x, i) => x != null  &&  !(x is RepeatableOperationException)  &&  !x.IsTransient()]]></code>
        /// </param>
        /// <param name="isSuccess">A delegate testing if the unit of work has succeeded. Can be <see langword="null" /> in which case a default implementation will be invoked
        /// <code>
        /// <![CDATA[x == null]]>
        /// </code></param>
        /// <param name="epilogue">A delegate invoked after the unit of work has been tried unsuccessfully <c>maxRetries</c>. Can be <see langword="null" /> in which case a default implementation will be invoked.
        /// <code>
        /// <![CDATA[(r, x, i) => { if (x != null) throw x; else return r; }]]>
        /// </code></param>
        /// <param name="optimisticConcurrencyStrategy">The optimistic concurrency strategy for the repository.</param>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="transactionScopeFactory">The transaction scope factory.</param>
        /// <param name="createTransactionScope">if set to <see langword="true" /> [create transaction scope].</param>
        public RetryUnitOfWork(
            Func<IRepository, int, T> work,
            Func<T, Exception, int, bool> isFailure = null,
            Func<T, Exception, int, bool> isSuccess = null,
            Func<T, Exception, int, T> epilogue = null,
            OptimisticConcurrencyStrategy optimisticConcurrencyStrategy = OptimisticConcurrencyStrategy.StoreWins,
            Func<OptimisticConcurrencyStrategy, IRepository> repositoryFactory = null,
            Func<TransactionScope> transactionScopeFactory = null,
            bool createTransactionScope = false)
            : base(
                i => new UnitOfWork(
                            optimisticConcurrencyStrategy,
                            repositoryFactory,
                            transactionScopeFactory,
                            createTransactionScope)
                        .WorkFunc(r => work(r, i)),
                isFailure ?? ((r, x, i) => x != null  &&  !(x is RepeatableOperationException)  &&  !x.IsTransient()),
                isSuccess ?? ((r, x, i) => x == null),
                epilogue  ?? ((r, x, i) => { if (x != null) throw x; else return r; }))
        {
        }
    }
}