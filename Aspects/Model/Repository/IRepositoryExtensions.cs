using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (modifiedProperties == null)
                throw new ArgumentNullException(nameof(modifiedProperties));

            return repository.AttachEntity<T>(
                                    entity,
                                    state,
                                    modifiedProperties
                                        .Select(pe => pe.GetMemberName())
                                        .ToArray());
        }

        /// <summary>
        /// Determines whether a principal object's associated object or collection of objects is already loaded in memory by the repository.
        /// </summary>
        /// <typeparam name="TPrincipal">The type of the principal object.</typeparam>
        /// <typeparam name="TAssociated">The type of the associated object.</typeparam>
        /// <param name="repository">The repository where the objects are or will be stored to.</param>
        /// <param name="principal">The principal object.</param>
        /// <param name="associateLambda">Must be a simple lambda expression of the type <c>principal =&gt; principal.Associated</c> which will supply the reference to and the name of the property or collection.</param>
        /// <returns><see langword="true" /> if the specified reference is loaded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// repository
        /// or
        /// principal
        /// or
        /// associateLambda
        /// </exception>
        public static bool IsLoaded<TPrincipal, TAssociated>(
            this IRepository repository,
            TPrincipal principal,
            Expression<Func<TPrincipal, TAssociated>> associateLambda)
            where TPrincipal : class
            where TAssociated : class
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (associateLambda == null)
                throw new ArgumentNullException(nameof(associateLambda));

            return OrmBridge.IsLoaded(principal, associateLambda, repository);
        }

        /// <summary>
        /// Loads the principal object's associated object or collection from the DB, if it is not loaded already.
        /// </summary>
        /// <typeparam name="TPrincipal">The type of the principal object.</typeparam>
        /// <typeparam name="TAssociated">The type of the associated object.</typeparam>
        /// <param name="repository">The repository where the objects are or will be stored to.</param>
        /// <param name="principal">The principal object.</param>
        /// <param name="associateLambda">Must be a simple lambda expression of the type <c>principal =&gt; principal.Associated</c> which will supply the value and name of the property.</param>
        /// <returns><see langword="true" /> if the specified reference is loaded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// repository
        /// or
        /// principal
        /// or
        /// associateLambda
        /// </exception>
        public static bool Load<TPrincipal, TAssociated>(
            this IRepository repository,
            TPrincipal principal,
            Expression<Func<TPrincipal, TAssociated>> associateLambda)
            where TPrincipal : class
            where TAssociated : class
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (associateLambda == null)
                throw new ArgumentNullException(nameof(associateLambda));

            return OrmBridge.Load(principal, associateLambda, repository);
        }


        /// <summary>
        /// Asynchronously loads the principal object's associated object or collection from the DB, if it is not loaded already.
        /// </summary>
        /// <typeparam name="TPrincipal">The type of the principal object.</typeparam>
        /// <typeparam name="TAssociated">The type of the associated object.</typeparam>
        /// <param name="repository">The repository where the objects are or will be stored to.</param>
        /// <param name="principal">The principal object.</param>
        /// <param name="associateLambda">Must be a simple lambda expression of the type <c>principal =&gt; principal.Associated</c> which will supply the value and name of the property.</param>
        /// <returns><see langword="true" /> if the specified reference is loaded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// repository
        /// or
        /// principal
        /// or
        /// associateLambda
        /// </exception>
        public static async Task<bool> LoadAsync<TPrincipal, TAssociated>(
            this IRepository repository,
            TPrincipal principal,
            Expression<Func<TPrincipal, TAssociated>> associateLambda)
            where TPrincipal : class
            where TAssociated : class
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (associateLambda == null)
                throw new ArgumentNullException(nameof(associateLambda));

            return await OrmBridge.LoadAsync(principal, associateLambda, repository);
        }
    }
}
