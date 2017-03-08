using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace vm.Aspects.Model.Repository
{
    /// <summary>
    /// Class IRepositoryExtensions. Adds extension methods to the <see cref="IRepository"/>.
    /// </summary>
    public static class IRepositoryExtensions
    {
        /// <summary>
        /// Attaches the specified entity to the context of the repository and marks the entire entity or the specified properties as modified.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="repository">The repository.</param>
        /// <param name="entity">The entity to attach and mark as modified.</param>
        /// <param name="state">The repository state of the entity.</param>
        /// <param name="modifiedProperties">
        /// The modified the properties that actually changed their values. The property names are expressed as simple lambda expressions, e.g. <c>e => e.Name</c>.
        /// If the array is empty, the entire entity will be marked as modified and updated in the store 
        /// otherwise, only the modified properties will be updated in the store.
        /// </param>
        /// <returns>The repository.</returns>
        public static IRepository Attach<T>(
            this IRepository repository,
            T entity,
            EntityState state,
            params Expression<Func<T, object>>[] modifiedProperties) where T : BaseDomainEntity
        {
            Contract.Requires<ArgumentNullException>(repository != null, nameof(repository));
            Contract.Requires<ArgumentNullException>(entity != null, nameof(entity));
            Contract.Requires<ArgumentNullException>(modifiedProperties != null, nameof(modifiedProperties));
            Contract.Ensures(Contract.Result<IRepository>() != null);

            return repository.AttachEntity<T>(
                                    entity,
                                    state,
                                    modifiedProperties
                                        .Select(pe => pe.GetMemberName())
                                        .ToArray());
        }
    }
}
