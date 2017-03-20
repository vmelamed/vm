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
        public TransactionScope TransactionScope { get; set; }

        /// <summary>
        /// Gets or sets the synchronous repository.
        /// </summary>
        public IRepository Repository { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous repository.
        /// </summary>
        public IRepositoryAsync AsyncRepository { get; set; }
    }
}
