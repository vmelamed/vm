using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Facilities;
using vm.Aspects.Facilities.Diagnostics;
using vm.Aspects.Model.Repository;
using vm.Aspects.Policies;

namespace vm.Aspects.Model
{
    /// <summary>
    /// The class PerCallContextRepositoryCallHandler is meant to be used as a policy (AOP aspect) in the call context of a WCF call.
    /// It is assumed that the repository is resolved from the DI container and has <see cref="T:vm.Aspects.Wcf.PerCallContextLifetimeManager"/>, i.e. all
    /// resolutions for <see cref="IRepository"/> with the same resolve name in the same WCF call context will return one and the same repository object.
    /// This handler implements two post-call actions: if there are no exceptions, it calls <see cref="IRepository.CommitChanges"/> to commit the unit 
    /// of work, otherwise rolls back the current transaction and then removes the repository's lifetime manager from the container. In other words,
    /// the application developer does not need to worry about saving changes in the repository, committing and rolling-back transactions, 
    /// error handling, repository disposal, etc.
    /// </summary>
    public class UnitOfWorkCallHandler : BaseCallHandler<TransactionScope>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to create explicitly transaction scope.
        /// The use of this property must be very well justified.
        /// </summary>
        public bool CreateTransactionScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log a warning for the exception.
        /// </summary>
        public bool LogExceptionWarnings { get; set; } = false;

        /// <summary>
        /// Prepares per-call data specific to the handler.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "will do later")]
        protected override TransactionScope Prepare(
            IMethodInvocation input)
        {
            if (!(input.Target is IHasRepository))
                throw new InvalidOperationException($"{nameof(UnitOfWorkCallHandler)} can be used only with services that implement {nameof(IHasRepository)}. Either implement it in {input.Target.GetType().Name} or remove this handler from the pipeline.");

            if (CreateTransactionScope  &&  Transaction.Current != null)
            {
                Facility.LogWriter.LogInfo(
                    "WARNING: The method {0} is called in the context of an existing transaction {1}/{2} and a new transaction scope is requested. Is this intended?",
                    input.MethodBase.Name,
                    Transaction.Current.TransactionInformation.LocalIdentifier,
                    Transaction.Current.TransactionInformation.DistributedIdentifier);
                Debug.WriteLine(
                    "WARNING: The method {0} is called in the context of an existing transaction {1}/{2} and a new transaction scope is requested. Is this intended?",
                    input.MethodBase.Name,
                    Transaction.Current.TransactionInformation.LocalIdentifier,
                    Transaction.Current.TransactionInformation.DistributedIdentifier);
            }

            return CreateTransactionScope
                                ? new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
                                : null;
        }

        /// <summary>
        /// Actions that take place after invoking the next handler or the target in the chain.
        /// Here it saves all changes in the IRepository instance which lifetime is managed per call context;
        /// commits the transaction scope if there are no exceptions;
        /// and disposes the IRepository instance.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The result.</param>
        /// <param name="transactionScope">The call data.</param>
        /// <returns>IMethodReturn.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#", Justification = "better descriptive name")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "protocol")]
        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            TransactionScope transactionScope)
        {

            if (methodReturn.IsAsyncCall())
                return methodReturn;        // return the task, do not clean-up yet

            try
            {
                if (methodReturn.Exception != null)
                    return methodReturn;    // return the exception (and cleanup)

                var hasRepository = (IHasRepository)input.Target;

                // get the repository
                var repository = hasRepository.Repository;

                if (repository == null)
                    throw new InvalidOperationException(nameof(IHasRepository)+" must return a non-null repository.");

                // commit
                repository.CommitChanges();
                transactionScope?.Complete();

                return methodReturn;
            }
            catch (Exception x)
            {
                VmAspectsEventSource.Log.CallHandlerFails(input, x);
                return input.CreateExceptionMethodReturn(x);
            }
            finally
            {
                // and clean-up
                transactionScope?.Dispose();
            }
        }

        /// <summary>
        /// Gives the aspect a chance to do some final work after the main task is truly complete.
        /// The overriding implementations should begin by calling the base class' implementation first.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="transactionScope">The call data.</param>
        /// <returns>Task{TResult}.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
        protected override async Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            TransactionScope transactionScope)
        {
            try
            {
                if (methodReturn.Exception != null)
                    throw methodReturn.Exception;

                var result = await base.ContinueWith<TResult>(input, methodReturn, transactionScope);
                var hasRepository = (IHasRepository)input.Target;

                var repository = hasRepository.Repository;

                if (repository == null)
                    throw new InvalidOperationException(nameof(IHasRepository)+" must return a non-null repository.");

                await repository.CommitChangesAsync();
                transactionScope?.Complete();
                return result;
            }
            catch (Exception x)
            {
                VmAspectsEventSource.Log.CallHandlerFails(input, x);
                throw;
            }
            finally
            {
                transactionScope?.Dispose();
            }
        }
    }
}
