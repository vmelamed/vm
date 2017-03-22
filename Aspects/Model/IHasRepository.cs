using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Provides access to the repository objects in the implementing object
    /// </summary>
    public interface IHasRepository
    {
        /// <summary>
        /// Gets the synchronous repository.
        /// </summary>
        IRepository Repository { get; }
    }
}
