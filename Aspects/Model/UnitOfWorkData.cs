using System.Transactions;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model
{
    /// <summary>
    /// The local data to carry between the phases of the call handler.
    /// </summary>
    public class UnitOfWorkData
    {
        /// <summary>
        /// Gets or sets the transaction scope.
        /// </summary>
        internal TransactionScope TransactionScope { get; set; }

        /// <summary>
        /// Gets or sets the synchronous repository.
        /// </summary>
        internal IRepository Repository { get; set; }

        /// <summary>
        /// Gets or sets the exception handler.
        /// </summary>
        internal OptimisticConcurrencyExceptionHandler ExceptionHandler { get; set; }
    }
}
