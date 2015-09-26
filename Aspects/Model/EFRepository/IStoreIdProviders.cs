using System;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Interface IStoreIdProvider defines the behavior of the unique store ID-s providers.
    /// </summary>
    public interface IStoreIdProvider
    {
        /// <summary>
        /// Gets a provider which generates ID sequence of type <typeparamref name="TId"/>.
        /// </summary>
        /// <typeparam name="TId">The type of the generated ID-s.</typeparam>
        /// <returns>IStoreUniqueId&lt;TId&gt;.</returns>
        /// <exception cref="NotImplementedException">
        /// Thrown if the provider does not support ID-s of the specified type.
        /// </exception>
        IStoreUniqueId<TId> GetProvider<TId>() where TId : IEquatable<TId>;
    }
}
