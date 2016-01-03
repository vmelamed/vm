using System;
using System.Diagnostics.Contracts;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Interface IStoreIdProvider defines the behavior of the unique store ID-s providers.
    /// </summary>
    [ContractClass(typeof(IStoreIdProviderContract))]
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

    #region IStoreIdProvider contract binding
    [ContractClassFor(typeof(IStoreIdProvider))]
    abstract class IStoreIdProviderContract : IStoreIdProvider
    {
        public IStoreUniqueId<TId> GetProvider<TId>()
            where TId : IEquatable<TId>
        {
            Contract.Ensures(Contract.Result<IStoreUniqueId<TId>>() != null);

            throw new NotImplementedException();
        }
    }
    #endregion

}
